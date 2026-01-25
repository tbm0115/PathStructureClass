namespace PathStructure
{
    /// <summary>
    /// Provides configuration for the explorer watcher.
    /// </summary>
    public class ExplorerWatcherOptions
    {
        /// <summary>
        /// Gets or sets the polling interval in milliseconds.
        /// </summary>
        public int PollRateMs { get; set; } = 500;

        /// <summary>
        /// Gets or sets the logger used for watcher errors.
        /// </summary>
        public IExplorerWatcherLogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the credential provider for secured resources.
        /// </summary>
        public ICredentialProvider CredentialProvider { get; set; }
    }
}
