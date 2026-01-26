using System;
using PathStructure.Abstracts;

namespace PathStructure
{
    /// <inheritdoc />
    public class PathMatchNode : IPathMatchNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathMatchNode"/> class.
        /// </summary>
        public PathMatchNode(IPathNode node, string matchedValue)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            MatchedValue = matchedValue;
        }

        /// <inheritdoc />
        public IPathNode Node { get; }

        /// <inheritdoc />
        public string MatchedValue { get; }
    }
}
