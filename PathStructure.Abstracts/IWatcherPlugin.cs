using System;
using System.Windows.Forms;

namespace PathStructure.Abstracts
{
    /// <summary>
    /// Defines the contract for watcher plugins that integrate with PathStructure clients.
    /// </summary>
    public interface IWatcherPlugin
    {
        string Name { get; }
        string Description { get; }
        string Suite { get; }
        IPathStructure ReferenceStructure { get; set; }
        Keys ShortcutKeys { get; }

        void Run();
        void ChangedCurrentPath(string currentPath);

        event EventHandler<PathEventArgs> SetCurrentPath;
        event EventHandler<PathEventArgs> GetCurrentPath;
        event EventHandler<PathEventArgs> GetSelectedPath;
    }
}
