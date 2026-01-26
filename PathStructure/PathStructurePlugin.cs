using System.Text.Json;

namespace PathStructure
{
    /// <summary>
    /// Describes a plugin entry in the configuration file.
    /// </summary>
    public class PathStructurePlugin
    {
        /// <summary>
        /// Gets or sets the path to the plugin assembly.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the plugin options payload.
        /// </summary>
        public JsonElement Options { get; set; }
    }
}
