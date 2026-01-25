using System;

namespace PathStructure
{
    /// <summary>
    /// Captures a node match during validation.
    /// </summary>
    public class PathMatchNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathMatchNode"/> class.
        /// </summary>
        public PathMatchNode(PathNode node, string matchedValue)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            MatchedValue = matchedValue;
        }

        /// <summary>
        /// Gets the matched configuration node.
        /// </summary>
        public PathNode Node { get; }

        /// <summary>
        /// Gets the matched substring for the node.
        /// </summary>
        public string MatchedValue { get; }
    }
}
