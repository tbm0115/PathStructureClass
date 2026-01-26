using System;
using System.Text.RegularExpressions;
using PathStructure.Abstracts;

namespace PathStructure
{
    /// <inheritdoc />
    public class PathStructureConfig : IPathStructureConfig
    {
        /// <inheritdoc />
        public PathStructureConfig(IPathNode root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <inheritdoc />
        public IPathNode Root { get; }

        /// <inheritdoc />
        public RegexOptions RegexOptions { get; set; } = RegexOptions.IgnoreCase;
    }
}
