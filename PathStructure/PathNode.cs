using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PathStructure.Abstracts;

namespace PathStructure
{
    /// <inheritdoc />
    public class PathNode : IPathNode
    {
        private readonly List<IPathNode> _children = new List<IPathNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PathNode"/> class.
        /// </summary>
        public PathNode(string name, string pattern)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Pattern { get; }

        /// <summary>
        /// Gets the child nodes for this node.
        /// </summary>
        public IList<IPathNode> Children => _children;

        /// <inheritdoc />
        IReadOnlyList<IPathNode> IPathNode.Children => _children;

        /// <inheritdoc />
        public Regex GetRegex(RegexOptions options)
        {
            return new Regex(Pattern, options | RegexOptions.Compiled);
        }
    }
}
