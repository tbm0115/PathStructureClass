using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PathStructureClass
{
    /// <summary>
    /// Represents a single node in the configured path structure tree.
    /// </summary>
    public class PathNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathNode"/> class.
        /// </summary>
        public PathNode(string name, string pattern)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        /// <summary>
        /// Gets the display name for the node.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the regex pattern for this node.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Gets the child nodes for this node.
        /// </summary>
        public List<PathNode> Children { get; } = new List<PathNode>();

        /// <summary>
        /// Builds a regex for the node using the provided options.
        /// </summary>
        public Regex GetRegex(RegexOptions options)
        {
            return new Regex(Pattern, options | RegexOptions.Compiled);
        }
    }
}
