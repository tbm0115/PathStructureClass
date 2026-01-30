namespace PathStructure.WatcherHost.Dtos
{
    /// <summary>
    /// Represents a JSON-RPC error payload.
    /// </summary>
    internal sealed class JsonRpcErrorDetails
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets any extra error data.
        /// </summary>
        public object Data { get; set; }
    }
}
