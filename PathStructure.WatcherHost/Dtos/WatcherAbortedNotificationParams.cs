namespace PathStructure.WatcherHost.Dtos
{
    /// <summary>
    /// Represents watcher aborted notification parameters.
    /// </summary>
    internal sealed class WatcherAbortedNotificationParams
    {
        /// <summary>
        /// Gets or sets the aborted message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the ISO timestamp for the aborted event.
        /// </summary>
        public string Timestamp { get; set; }
    }
}
