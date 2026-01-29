namespace PathStructure.WatcherHost.Dtos
{
    /// <summary>
    /// Represents watcher error notification parameters.
    /// </summary>
    internal sealed class WatcherErrorNotificationParams
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the ISO timestamp for the error event.
        /// </summary>
        public string Timestamp { get; set; }
    }
}
