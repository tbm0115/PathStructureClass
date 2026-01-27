using System.Text.Json;

namespace PathStructure.Abstracts
{
    /// <summary>
    /// Represents a plugin entry in the configuration.
    /// </summary>
    public interface IPathStructurePlugin
    {
        /// <summary>
        /// Gets the path to the plugin assembly.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the plugin options payload.
        /// </summary>
        JsonElement Options { get; }
    }
}
