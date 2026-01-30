using System.Collections.Generic;

namespace PathStructure
{
    /// <summary>
    /// Represents an installation profile for managed clients.
    /// </summary>
    public class PathStructureInstallationProfile
    {
        /// <summary>
        /// Gets or sets the profile identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the profile name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the profile description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the WatcherHost configuration path.
        /// </summary>
        public string WatcherHostConfigPath { get; set; }

        /// <summary>
        /// Gets or sets the PathStructureClient configuration path.
        /// </summary>
        public string ClientConfigPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether approval is required.
        /// </summary>
        public bool RequireApproval { get; set; }

        /// <summary>
        /// Gets the optional tags for this profile.
        /// </summary>
        public IList<string> Tags { get; set; } = new List<string>();
    }
}
