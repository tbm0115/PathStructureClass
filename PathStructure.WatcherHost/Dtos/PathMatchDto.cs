using System.Collections.Generic;

namespace PathStructure.WatcherHost.Dtos
{
    /// <summary>
    /// Represents a path match with styling metadata and nested child matches.
    /// </summary>
    internal sealed class PathMatchDto
    {
        /// <summary>
        /// Gets or sets the path structure name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the regex pattern used for matching.
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Gets or sets the matched value from the filesystem path.
        /// </summary>
        public string MatchedValue { get; set; }

        /// <summary>
        /// Gets or sets the length of the match.
        /// </summary>
        public int MatchLength { get; set; }

        /// <summary>
        /// Gets or sets the flavor text template string.
        /// </summary>
        public string FlavorTextTemplate { get; set; }

        /// <summary>
        /// Gets or sets the background color for the match.
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color for the match.
        /// </summary>
        public string ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the icon identifier for the match.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the path is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets nested child matches.
        /// </summary>
        public List<PathMatchDto> ChildMatches { get; set; }
    }
}
