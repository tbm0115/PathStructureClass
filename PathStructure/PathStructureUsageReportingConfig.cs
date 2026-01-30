namespace PathStructure
{
    /// <summary>
    /// Describes usage reporting requirements.
    /// </summary>
    public class PathStructureUsageReportingConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether usage reporting is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the minimum interval, in seconds, between reports.
        /// </summary>
        public int MinimumReportIntervalSeconds { get; set; } = 60;
    }
}
