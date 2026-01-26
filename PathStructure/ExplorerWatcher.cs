using System;
using System.Threading;
using Shell32;
using SHDocVw;

namespace PathStructure
{
    /// <summary>
    /// Uses Shell to get the users selection(s) within Windows Explorer/Internet Explorer.
    /// </summary>
    public class ExplorerWatcher
    {
        private readonly PathStructure _pathStruct;
        private readonly ExplorerWatcherOptions _options;
        private Timer _watcher;
        private readonly AutoResetEvent _pollSignal = new AutoResetEvent(false);
        private Thread _staThread;
        private bool _cancel;
        private ExplorerWatcherFoundEventArgs _evt;

        /// <summary>
        /// Gets the current snapshot of watched windows.
        /// </summary>
        public ExplorerWatcherFoundEventArgs CurrentFoundPaths => _evt;

        /// <summary>
        /// Occurs when a watched window's path changes.
        /// </summary>
        public event ExplorerWatcherFoundEventHandler ExplorerWatcherFound;

        /// <summary>
        /// Occurs when the watcher stops unexpectedly.
        /// </summary>
        public event EventHandler ExplorerWatcherAborted;

        /// <summary>
        /// Occurs when an error is encountered while polling windows.
        /// </summary>
        public event EventHandler<Exception> ExplorerWatcherError;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorerWatcher"/> class.
        /// </summary>
        public ExplorerWatcher(PathStructure pathStruct, ExplorerWatcherOptions options = null)
        {
            _pathStruct = pathStruct;
            _options = options ?? new ExplorerWatcherOptions();
            _cancel = false;
        }

        /// <summary>
        /// Begins polling Shell for current contexts of Windows Explorer/Internet Explorer.
        /// </summary>
        public void StartWatcher()
        {
            _cancel = false;

            if (_staThread == null || !_staThread.IsAlive)
            {
                _staThread = new Thread(PollLoop)
                {
                    IsBackground = true
                };
                _staThread.SetApartmentState(ApartmentState.STA);
                _staThread.Start();
            }

            _watcher?.Dispose();
            _watcher = new Timer(_ => _pollSignal.Set(), null, 0, _options.PollRateMs);
        }

        /// <summary>
        /// Requests that the polling abort.
        /// </summary>
        public void StopWatcher()
        {
            _cancel = true;
            try
            {
                _watcher?.Change(Timeout.Infinite, Timeout.Infinite);
                _watcher?.Dispose();
                _watcher = null;
                _pollSignal.Set();
            }
            catch (Exception ex)
            {
                _options.Logger?.LogError("Abort failed.", ex);
            }
        }

        /// <summary>
        /// Relays path updates to the external event.
        /// </summary>
        private void WindowPathChanged(string url)
        {
            ExplorerWatcherFound?.Invoke(url);
        }

        /// <summary>
        /// Executes polling on a dedicated STA thread.
        /// </summary>
        private void PollLoop()
        {
            while (true)
            {
                _pollSignal.WaitOne();
                if (_cancel)
                {
                    break;
                }

                ExplorerQueryInternal();
            }
        }

        private void ExplorerQueryInternal()
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
                    foreach (ShellBrowserWindow w in exShell.Windows() as IShellWindows)
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
}
