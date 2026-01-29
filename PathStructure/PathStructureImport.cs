using PathStructure.Abstracts;

namespace PathStructure
{
    /// <summary>
    /// Describes a configuration file to import into a path structure configuration.
    /// </summary>
    public class PathStructureImport : IPathStructureImport
    {
        /// <summary>
        /// Gets or sets the path to the configuration file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the namespace for variables imported from the configuration.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the root path to prepend to the imported configuration's top-level regex patterns.
        /// </summary>
        public string RootPath { get; set; }
    }
}
