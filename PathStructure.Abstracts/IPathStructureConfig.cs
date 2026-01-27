using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PathStructure.Abstracts
{
    /// <summary>
    /// Represents configuration for the path structure service.
    /// </summary>
    public interface IPathStructureConfig
    {
        /// <summary>
        /// Gets the imports configured for the structure.
        /// </summary>
        IReadOnlyList<IPathStructureImport> Imports { get; }

        /// <summary>
        /// Gets the path patterns configured for the structure.
        /// </summary>
        IReadOnlyList<IPathStructurePath> Paths { get; }

        /// <summary>
        /// Gets the plugin descriptors configured for the structure.
        /// </summary>
        IReadOnlyList<IPathStructurePlugin> Plugins { get; }

        /// <summary>
        /// Gets the root node of the configured structure.
        /// </summary>
        IPathNode Root { get; }

        /// <summary>
        /// Gets or sets the regex options used for matching.
        /// </summary>
        RegexOptions RegexOptions { get; set; }
    }
}
