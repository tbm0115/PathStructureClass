namespace PathStructure
{
    /// <summary>
    /// Describes a configuration file to import into a path structure configuration.
    /// </summary>
    public class PathStructureImport
    {
        /// <summary>
        /// Gets or sets the path to the configuration file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the namespace for variables imported from the configuration.
        /// </summary>
        public string Namespace { get; set; }
    }
}
