namespace PathStructure.WatcherHost.Dtos
{
    /// <summary>
    /// Represents a JSON-RPC success response payload.
    /// </summary>
    /// <typeparam name="TResult">The response result type.</typeparam>
    internal sealed class JsonRpcResponse<TResult>
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
        /// Gets or sets the response result payload.
        /// </summary>
        public TResult Result { get; set; }
    }
}
