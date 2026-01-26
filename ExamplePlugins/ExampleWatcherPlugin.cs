using System;
using System.Windows.Forms;
using PathStructure.Abstracts;

namespace PathStructure.ExamplePlugins
{
    /// <summary>
    /// Example plugin implementation that demonstrates the watcher interface.
    /// </summary>
    public class ExampleWatcherPlugin : IWatcherPlugin
    {
        public string Name => "Example Watcher";

        public string Description => "Sample implementation for validating plugin loading and events.";

        public string Suite => "Example";

        public PathStructure.PathStructure ReferenceStructure { get; set; }

        public Keys ShortcutKeys => Keys.Control | Keys.Shift | Keys.E;

        public event EventHandler<PathEventArgs> SetCurrentPath;
        public event EventHandler<PathEventArgs> GetCurrentPath;
        public event EventHandler<PathEventArgs> GetSelectedPath;

        public void Run()
        {
            var args = new PathEventArgs();
            GetCurrentPath?.Invoke(this, args);

            if (!string.IsNullOrWhiteSpace(args.Path))
            {
                ChangedCurrentPath(args.Path);
            }
        }

        public void ChangedCurrentPath(string currentPath)
        {
            if (string.IsNullOrWhiteSpace(currentPath))
            {
                return;
            }

            SetCurrentPath?.Invoke(this, new PathEventArgs(currentPath));
        }
    }
}
