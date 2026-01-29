using System;

namespace PathStructure.WatcherHost.Dtos
{
    /// <summary>
    /// Represents a JSON-RPC notification payload.
    /// </summary>
    /// <typeparam name="TParams">The parameter payload type.</typeparam>
    internal sealed class JsonRpcNotification<TParams>
    {
        /// <summary>
        /// Gets or sets the JSON-RPC protocol version.
        /// </summary>
        public string Jsonrpc { get; set; } = "2.0";

        /// <summary>
        /// Gets or sets the notification method name.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the notification parameters.
        /// </summary>
        public TParams Params { get; set; }
    }
}
