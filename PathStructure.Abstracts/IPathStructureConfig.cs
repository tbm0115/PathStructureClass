using System.Text.RegularExpressions;

namespace PathStructure.Abstracts
{
    /// <summary>
    /// Represents configuration for the path structure service.
    /// </summary>
    public interface IPathStructureConfig
    {
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
