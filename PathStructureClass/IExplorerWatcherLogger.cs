using System;

namespace PathStructureClass
{
    /// <summary>
    /// Defines logging behavior for explorer watcher errors.
    /// </summary>
    public interface IExplorerWatcherLogger
    {
        /// <summary>
        /// Logs an error encountered by the watcher.
        /// </summary>
        void LogError(string message, Exception exception);
    }
}
