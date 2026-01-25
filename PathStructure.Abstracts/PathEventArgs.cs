using System;

namespace PathStructure.Abstracts
{
    /// <summary>
    /// Event payload for plugins requesting or setting path values.
    /// </summary>
    public class PathEventArgs : EventArgs
    {
        public PathEventArgs()
        {
        }

        public PathEventArgs(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Gets or sets the path value associated with the event.
        /// </summary>
        public string Path { get; set; }
    }
}
