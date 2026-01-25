using System;
using System.Text.RegularExpressions;

namespace PathStructureClass
{
    /// <summary>
    /// Represents configuration for the path structure service.
    /// </summary>
    public class PathStructureConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathStructureConfig"/> class.
        /// </summary>
        public PathStructureConfig(PathNode root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>
        /// Gets the root node of the structure.
        /// </summary>
        public PathNode Root { get; }

        /// <summary>
        /// Gets or sets the regex options used for matching.
        /// </summary>
        public RegexOptions RegexOptions { get; set; } = RegexOptions.IgnoreCase;
    }
}
