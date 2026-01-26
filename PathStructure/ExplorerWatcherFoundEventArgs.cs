using System;
using System.Collections.Generic;
using SHDocVw;

namespace PathStructure
{
    /// <summary>
    /// Tracks the set of watched windows and raises path-change notifications.
    /// </summary>
    public class ExplorerWatcherFoundEventArgs
    {
        private List<WindowWatch> _wins;
        private static int NormalizeHandle(long windowHandle) => unchecked((int)windowHandle);

        /// <summary>
        /// Occurs when a watched window changes its path.
        /// </summary>
        public event ExplorerWatcherFoundEventHandler PathChanged;

        /// <summary>
        /// Gets or sets a tracked window by index.
        /// </summary>
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

        /// <summary>
        /// Adds a window to the tracking set.
        /// </summary>
        public void Add(ShellBrowserWindow windowObject)
        {
            if (_wins == null)
            {
                _wins = new List<WindowWatch>();
            }

            var windowHandle = NormalizeHandle(windowObject.HWND);
            if (!Contains(windowHandle))
            {
                _wins.Add(new WindowWatch(windowObject));
            }
            else
            {
                if (_wins[IndexOf(windowHandle)].CheckWindow(windowObject))
                {
                    PathChanged?.Invoke(_wins[IndexOf(windowHandle)].URL);
                }
            }
        }

        /// <summary>
        /// Checks a window for a path change.
        /// </summary>
        public void Check(ShellBrowserWindow windowObject)
        {
            var windowHandle = NormalizeHandle(windowObject.HWND);
            if (Contains(windowHandle))
            {
                if (_wins[IndexOf(windowHandle)].CheckWindow(windowObject))
                {
                    PathChanged?.Invoke(_wins[IndexOf(windowHandle)].URL);
                }
            }
        }

        /// <summary>
        /// Removes a window from tracking.
        /// </summary>
        public void Remove(WindowWatch watch)
        {
            if (Contains(watch.WindowHandle))
            {
                RemoveAt(IndexOf(watch.WindowHandle));
                return;
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Removes a window from tracking by index.
        /// </summary>
        public void RemoveAt(int index)
        {
            if (index < _wins.Count)
            {
                _wins.RemoveAt(index);
                return;
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Relays path changes from child watchers.
        /// </summary>
        private void ChildPathChanged(string url, EventArgs e)
        {
            PathChanged?.Invoke(url);
        }

        /// <summary>
        /// Removes a window when it is closed.
        /// </summary>
        private void RemoveWindow(ShellBrowserWindow hWindow, EventArgs e)
        {
            var windowHandle = NormalizeHandle(hWindow.HWND);
            if (Contains(windowHandle))
            {
                RemoveAt(IndexOf(windowHandle));
            }
        }

        /// <summary>
        /// Determines whether the specified handle is being tracked.
        /// </summary>
        public bool Contains(int windowHandle)
        {
            return IndexOf(windowHandle) >= 0;
        }

        /// <summary>
        /// Determines whether the specified handle is being tracked.
        /// </summary>
        public bool Contains(long windowHandle)
        {
            return Contains(NormalizeHandle(windowHandle));
        }

        /// <summary>
        /// Gets the index of a tracked window handle.
        /// </summary>
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

        /// <summary>
        /// Gets the index of a tracked window handle.
        /// </summary>
        public int IndexOf(long windowHandle)
        {
            return IndexOf(NormalizeHandle(windowHandle));
        }
    }
}
