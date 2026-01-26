using System;
using Shell32;
using SHDocVw;

namespace PathStructure
{
    /// <summary>
    /// Tracks a single explorer window and its current focused path.
    /// </summary>
    public class WindowWatch
    {
        private ShellBrowserWindow _win;
        private string _path;
        private readonly int _handle;
        private static int NormalizeHandle(long windowHandle) => unchecked((int)windowHandle);

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowWatch"/> class.
        /// </summary>
        public WindowWatch(ShellBrowserWindow hWindow)
        {
            _win = hWindow;
            _handle = NormalizeHandle(hWindow.HWND);
            CheckWindow(hWindow);
        }

        /// <summary>
        /// Gets the current window URL.
        /// </summary>
        public string URL => _path;

        /// <summary>
        /// Gets or sets the underlying shell window.
        /// </summary>
        public ShellBrowserWindow Window
        {
            get => _win;
            set => _win = value;
        }

        /// <summary>
        /// Gets the window handle.
        /// </summary>
        public int WindowHandle => _handle;

        /// <summary>
        /// Returns true when the path changes.
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
