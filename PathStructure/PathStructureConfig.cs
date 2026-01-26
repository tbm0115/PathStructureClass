using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PathStructure.Abstracts;

namespace PathStructure
{
    /// <inheritdoc />
    public class PathStructureConfig : IPathStructureConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathStructureConfig"/> class.
        /// </summary>
        public PathStructureConfig()
        {
        }

        /// <inheritdoc />
        public PathStructureConfig(IPathNode root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>
        /// Gets the import descriptors for this configuration.
        /// </summary>
        public IList<PathStructureImport> Imports { get; set; } = new List<PathStructureImport>();

        /// <summary>
        /// Gets the path patterns configured for this structure.
        /// </summary>
        public IList<PathStructurePath> Paths { get; set; } = new List<PathStructurePath>();

        /// <summary>
        /// Gets the plugin descriptors configured for this structure.
        /// </summary>
        public IList<PathStructurePlugin> Plugins { get; set; } = new List<PathStructurePlugin>();

        /// <inheritdoc />
        public IPathNode Root { get; private set; }

        /// <inheritdoc />
        public RegexOptions RegexOptions { get; set; } = RegexOptions.IgnoreCase;

        internal void SetRoot(IPathNode root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }
    }
}
