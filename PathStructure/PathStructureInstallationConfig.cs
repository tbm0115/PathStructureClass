using System.Collections.Generic;

namespace PathStructure
{
    /// <summary>
    /// Describes managed installation profiles.
    /// </summary>
    public class PathStructureInstallationConfig
    {
        /// <summary>
        /// Gets or sets the available installation profiles.
        /// </summary>
        public IList<PathStructureInstallationProfile> Profiles { get; set; } = new List<PathStructureInstallationProfile>();

        /// <summary>
        /// Gets or sets the default profile identifier to assign.
        /// </summary>
        public string DefaultProfileId { get; set; }
    }
}
