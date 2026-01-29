namespace PathStructure.Abstracts
{
    /// <summary>
    /// Represents a configuration import entry.
    /// </summary>
    public interface IPathStructureImport
    {
        /// <summary>
        /// Gets the path to the configuration file.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the namespace for variables imported from the configuration.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets the root path to prepend to the imported configuration's top-level regex patterns.
        /// </summary>
        string RootPath { get; }
    }
}
