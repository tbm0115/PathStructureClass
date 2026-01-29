namespace PathStructure.WatcherHost.Dtos
{
    /// <summary>
    /// Represents status notification parameters.
    /// </summary>
    internal sealed class StatusNotificationParams
    {
        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the connection state string.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the ISO timestamp for the status event.
        /// </summary>
        public string Timestamp { get; set; }
    }
}
