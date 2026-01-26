using System;
using System.Threading;
using System.Timers;
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
        private readonly Timer _watcher;
        private bool _cancel;
        private ExplorerWatcherFoundEventArgs _evt;
        private int _isPolling;

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
            _watcher = new Timer(_options.PollRateMs);
            _watcher.Elapsed += ExplorerQuery;
            _cancel = false;
        }

        /// <summary>
        /// Begins polling Shell for current contexts of Windows Explorer/Internet Explorer.
        /// </summary>
        public void StartWatcher()
        {
            _watcher.Start();
        }

        /// <summary>
        /// Requests that the polling abort.
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

        /// <summary>
        /// Relays path updates to the external event.
        /// </summary>
        private void WindowPathChanged(string url)
        {
            ExplorerWatcherFound?.Invoke(url);
        }

        /// <summary>
        /// Handles timer events for polling.
        /// </summary>
        private void ExplorerQuery(object sender, ElapsedEventArgs e)
        {
            ExplorerQuery();
        }

        /// <summary>
        /// Performs a poll of open shell windows.
        /// </summary>
        private void ExplorerQuery()
        {
            if (Interlocked.CompareExchange(ref _isPolling, 1, 0) != 0)
            {
                return;
            }

            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                var staThread = new Thread(() =>
                {
                    try
                    {
                        ExplorerQueryInternal();
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _isPolling, 0);
                    }
                })
                {
                    IsBackground = true
                };
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                return;
            }

            try
            {
                ExplorerQueryInternal();
            }
            finally
            {
                Interlocked.Exchange(ref _isPolling, 0);
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
