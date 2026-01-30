using System;
using System.Collections.Generic;

namespace PathStructure
{
    /// <summary>
    /// Represents a reusable path structure model.
    /// </summary>
    public class PathStructureModel
    {
        /// <summary>
        /// Gets or sets the model identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the model name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the model description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the model version label.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the last updated timestamp.
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the import descriptors for this model.
        /// </summary>
        public IList<PathStructureImport> Imports { get; set; } = new List<PathStructureImport>();

        /// <summary>
        /// Gets or sets the path patterns for this model.
        /// </summary>
        public IList<PathStructurePath> Paths { get; set; } = new List<PathStructurePath>();

        /// <summary>
        /// Gets or sets the plugin descriptors for this model.
        /// </summary>
        public IList<PathStructurePlugin> Plugins { get; set; } = new List<PathStructurePlugin>();
    }
}
