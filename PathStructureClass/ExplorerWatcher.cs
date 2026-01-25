using System;
using System.Collections.Generic;
using System.Timers;
using Shell32;
using SHDocVw;

namespace PathStructureClass
{
    /// <summary>
    /// Uses Shell to get the users selection(s) within Windows Explorer/Internet Explorer
    /// </summary>
    public class ExplorerWatcher
    {
        private readonly PathStructure _pathStruct;
        private readonly ExplorerWatcherOptions _options;
        private readonly Timer _watcher;
        private bool _cancel;
        private ExplorerWatcherFoundEventArgs _evt;

        public ExplorerWatcherFoundEventArgs CurrentFoundPaths => _evt;

        /// <summary>
        /// This event is raised whenever a path selected in Windows Explorer or navigated to in Internet Explorer
        /// is successfully validated in Path.IsNamedStructure() as it would in an audit.
        /// </summary>
        public event ExplorerWatcherFoundEventHandler ExplorerWatcherFound;
        public event EventHandler ExplorerWatcherAborted;
        public event EventHandler<Exception> ExplorerWatcherError;

        public ExplorerWatcher(PathStructure pathStruct, ExplorerWatcherOptions options = null)
        {
            _pathStruct = pathStruct;
            _options = options ?? new ExplorerWatcherOptions();
            _watcher = new Timer(_options.PollRateMs);
            _watcher.Elapsed += ExplorerQuery;
            _cancel = false;
        }

        /// <summary>
        /// Begins polling Shell for current contexts of Windows Explorer/Internet Explorer
        /// </summary>
        public void StartWatcher()
        {
            _watcher.Start();
        }

        /// <summary>
        /// Requests that the polling abort
        /// </summary>
        public void StopWatcher()
        {
            _cancel = true;
            try
            {
                _watcher.Stop();
                _cancel = false;
            }
            catch (Exception ex)
            {
                _options.Logger?.LogError("Abort failed.", ex);
            }
        }

        private void WindowPathChanged(string url)
        {
            ExplorerWatcherFound?.Invoke(url);
        }

        private void ExplorerQuery(object sender, ElapsedEventArgs e)
        {
            ExplorerQuery();
        }

        private void ExplorerQuery()
        {
            var exShell = new Shell();
            if (_evt == null)
            {
                _evt = new ExplorerWatcherFoundEventArgs();
                _evt.PathChanged += WindowPathChanged;
            }

            if (_pathStruct != null && !_cancel)
            {
                // Get all the open Explorer windows
                try
                {
                    foreach (ShellBrowserWindow w in (IShellWindows)exShell.Windows)
                    {
                        if (_cancel)
                        {
                            break;
                        }

                        // Somehow these are different. They're known to fail so try everything.
                        if (!_evt.Contains(w.HWND))
                        {
                            _evt.Add(w);
                        }

                        if (_evt[_evt.IndexOf(w.HWND)].CheckWindow(w))
                        {
                            ExplorerWatcherFound?.Invoke(_evt[_evt.IndexOf(w.HWND)].URL);
                        }
                    }
                }
                catch (Exception ex)
                {
                    StopWatcher();
                    _options.Logger?.LogError("ExplorerWatcher aborted.", ex);
                    ExplorerWatcherError?.Invoke(this, ex);
                    // Go ahead and quit. Chances are that someone closed/opened windows too quickly
                    ExplorerWatcherAborted?.Invoke(this, new UnhandledExceptionEventArgs(ex, true));
                }
            }
            else
            {
                // Go ahead and quit. Chances are that someone closed/opened windows too quickly
                StopWatcher();
                ExplorerWatcherAborted?.Invoke(this, new UnhandledExceptionEventArgs(new Exception("Cancel Requested"), true));
            }
        }
    }

    public delegate void ExplorerWatcherFoundEventHandler(string url);

    public class ExplorerWatcherFoundEventArgs
    {
        private List<WindowWatch> _wins;

        public event ExplorerWatcherFoundEventHandler PathChanged;

        public WindowWatch this[int index]
        {
            get
            {
                if (index < _wins.Count)
                {
                    return _wins[index];
                }

                throw new IndexOutOfRangeException();
            }
            set
            {
                if (index < _wins.Count)
                {
                    _wins[index] = value;
                    return;
                }

                throw new IndexOutOfRangeException();
            }
        }

        public void Add(ShellBrowserWindow windowObject)
        {
            if (_wins == null)
            {
                _wins = new List<WindowWatch>();
            }

            if (!Contains(windowObject.HWND))
            {
                _wins.Add(new WindowWatch(windowObject));
            }
            else
            {
                if (_wins[IndexOf(windowObject.HWND)].CheckWindow(windowObject))
                {
                    PathChanged?.Invoke(_wins[IndexOf(windowObject.HWND)].URL);
                }
            }
        }

        public void Check(ShellBrowserWindow windowObject)
        {
            if (Contains(windowObject.HWND))
            {
                if (_wins[IndexOf(windowObject.HWND)].CheckWindow(windowObject))
                {
                    PathChanged?.Invoke(_wins[IndexOf(windowObject.HWND)].URL);
                }
            }
        }

        public void Remove(WindowWatch watch)
        {
            if (Contains(watch.WindowHandle))
            {
                RemoveAt(IndexOf(watch.WindowHandle));
                return;
            }

            throw new IndexOutOfRangeException();
        }

        public void RemoveAt(int index)
        {
            if (index < _wins.Count)
            {
                _wins.RemoveAt(index);
                return;
            }

            throw new IndexOutOfRangeException();
        }

        private void ChildPathChanged(string url, EventArgs e)
        {
            PathChanged?.Invoke(url);
        }

        private void RemoveWindow(ShellBrowserWindow hWindow, EventArgs e)
        {
            if (Contains(hWindow.HWND))
            {
                RemoveAt(IndexOf(hWindow.HWND));
            }
        }

        public bool Contains(int windowHandle)
        {
            return IndexOf(windowHandle) >= 0;
        }

        public int IndexOf(int windowHandle)
        {
            if (_wins != null)
            {
                for (var i = 0; i < _wins.Count; i += 1)
                {
                    if (_wins[i].WindowHandle == windowHandle)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public class WindowWatch
        {
            private ShellBrowserWindow _win;
            private string _path;
            private readonly int _handle;

            public string URL => _path;

            public ShellBrowserWindow Window
            {
                get => _win;
                set => _win = value;
            }

            public int WindowHandle => _handle;

            public WindowWatch(ShellBrowserWindow hWindow)
            {
                _win = hWindow;
                _handle = hWindow.HWND;
                CheckWindow(hWindow);
            }

            /// <summary>
            /// Return true when the path changes
            /// </summary>
            public bool CheckWindow(ShellBrowserWindow windowObject)
            {
                try
                {
                    string newPath = null;
                    if (_win.Document is IShellFolderViewDual shellFolderViewDual)
                    {
                        if (shellFolderViewDual.FocusedItem != null)
                        {
                            newPath = PathStructure_Helpers.GetUNCPath(shellFolderViewDual.FocusedItem.Path);
                        }
                    }
                    else if (_win.Document is ShellFolderView shellFolderView)
                    {
                        if (shellFolderView.FocusedItem != null)
                        {
                            newPath = PathStructure_Helpers.GetUNCPath(shellFolderView.FocusedItem.Path);
                        }
                    }

                    if (!string.Equals(_path, newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _path = newPath;
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    PathStructure_Helpers.Log("{ExplorerWatcher}(CheckWindow) PathStructure error: " + ex.Message);
                    return false;
                }
            }
        }
    }

    public class ExplorerWatcherOptions
    {
        public int PollRateMs { get; set; } = 500;
        public IExplorerWatcherLogger Logger { get; set; }
        public ICredentialProvider CredentialProvider { get; set; }
    }

    public interface IExplorerWatcherLogger
    {
        void LogError(string message, Exception exception);
    }

    public interface ICredentialProvider
    {
        bool TryGetCredential(string target, out ExplorerCredential credential);
    }

    public sealed class ExplorerCredential
    {
        public ExplorerCredential(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }
        public string Password { get; }
    }
}
