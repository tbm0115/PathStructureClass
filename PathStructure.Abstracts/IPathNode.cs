using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PathStructure.Abstracts
{
    /// <summary>
    /// Represents a node in the configured path structure tree.
    /// </summary>
    public interface IPathNode
    {
        /// <summary>
        /// Gets the display name for the node.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the regex pattern for this node.
        /// </summary>
        string Pattern { get; }

        /// <summary>
        /// Gets a value indicating whether this node is required.
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// Gets the child nodes for this node.
        /// </summary>
        IReadOnlyList<IPathNode> Children { get; }

        /// <summary>
        /// Builds a regex for the node using the provided options.
        /// </summary>
        Regex GetRegex(RegexOptions options);
    }
}
