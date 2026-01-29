namespace PathStructure.WatcherHost.Dtos
{
    /// <summary>
    /// Represents a JSON-RPC error response payload.
    /// </summary>
    internal sealed class JsonRpcErrorResponse
    {
        /// <summary>
        /// Gets or sets the JSON-RPC protocol version.
        /// </summary>
        public string Jsonrpc { get; set; } = "2.0";

        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        public JsonRpcErrorDetails Error { get; set; }
    }
}
