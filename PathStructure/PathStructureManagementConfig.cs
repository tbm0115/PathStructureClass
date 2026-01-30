namespace PathStructure
{
    /// <summary>
    /// Describes server-side management settings.
    /// </summary>
    public class PathStructureManagementConfig
    {
        /// <summary>
        /// Gets or sets authorization configuration.
        /// </summary>
        public PathStructureAuthorizationConfig Authorization { get; set; } = new PathStructureAuthorizationConfig();

        /// <summary>
        /// Gets or sets usage reporting requirements.
        /// </summary>
        public PathStructureUsageReportingConfig UsageReporting { get; set; } = new PathStructureUsageReportingConfig();

        /// <summary>
        /// Gets or sets managed installation profiles.
        /// </summary>
        public PathStructureInstallationConfig Installation { get; set; } = new PathStructureInstallationConfig();
    }
}
