using System.Collections.Generic;

namespace PathStructure.WatcherHost.Dtos
{
    /// <summary>
    /// Represents path change notification parameters.
    /// </summary>
    internal sealed class PathChangedNotificationParams
    {
        /// <summary>
        /// Gets or sets the notification message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the raw Explorer path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the current matched path structure.
        /// </summary>
        public PathMatchDto CurrentMatch { get; set; }

        /// <summary>
        /// Gets or sets the resolved template variables.
        /// </summary>
        public IReadOnlyDictionary<string, string> Variables { get; set; }

        /// <summary>
        /// Gets or sets the full match trail for the selection.
        /// </summary>
        public List<PathMatchDto> Matches { get; set; }

        /// <summary>
        /// Gets or sets the immediate child matches.
        /// </summary>
        public List<PathMatchDto> ImmediateChildMatches { get; set; }

        /// <summary>
        /// Gets or sets the ISO timestamp for the change event.
        /// </summary>
        public string Timestamp { get; set; }
    }
}
