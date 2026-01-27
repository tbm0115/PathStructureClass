# PathStructure.WatcherHost JSON-RPC

The watcher host speaks newline-delimited JSON-RPC 2.0 over a TCP connection (default `127.0.0.1:49321`). Each JSON object is serialized as one line of UTF-8 JSON and terminated with `\n`.

## Notifications (server → client)

### `pathChanged`

Emitted when the ExplorerWatcher reports a focused item change.

```json
{"jsonrpc":"2.0","method":"pathChanged","params":{"message":"Explorer path changed.","path":"C:\\\\Some\\\\Folder\\\\File.txt","timestamp":"2024-01-01T00:00:00.0000000+00:00"}}
```

### `watcherError`

Emitted when the watcher encounters a recoverable error.

```json
{"jsonrpc":"2.0","method":"watcherError","params":{"message":"Explorer watcher error.","error":"Details","timestamp":"2024-01-01T00:00:00.0000000+00:00"}}
```

### `watcherAborted`

Emitted when the watcher aborts unexpectedly.

```json
{"jsonrpc":"2.0","method":"watcherAborted","params":{"message":"Explorer watcher aborted.","error":"Details","timestamp":"2024-01-01T00:00:00.0000000+00:00"}}
```

### `status`

Emitted when a client connects.

```json
{"jsonrpc":"2.0","method":"status","params":{"message":"Client connected.","state":"connected","timestamp":"2024-01-01T00:00:00.0000000+00:00"}}
```

## Requests (client → server)

### `addPath`

Adds a path regex to the current in-memory configuration and root node if it does not already exist.

```json
{"jsonrpc":"2.0","id":"1","method":"addPath","params":{"regex":"^C:\\\\Example\\\\(?<Name>[^\\\\]+)\\\\File\\\\.txt$","name":"Example File","flavorTextTemplate":"Example {{ Name }}","backgroundColor":"#000000","foregroundColor":"#ffffff","icon":"icon.png","isRequired":true}}
```

#### Successful response

```json
{"jsonrpc":"2.0","id":"1","result":{"message":"Path regex added.","path":"^C:\\\\Example\\\\(?<Name>[^\\\\]+)\\\\File\\\\.txt$"}}
```

#### Error responses

Errors follow JSON-RPC error shapes with a numeric `code`, `message`, and optional `data`.

```json
{"jsonrpc":"2.0","id":"1","error":{"code":-32002,"message":"Path regex already exists.","data":"^C:\\\\Example\\\\(?<Name>[^\\\\]+)\\\\File\\\\.txt$"}}
```

## Error codes

| Code | Meaning |
| --- | --- |
| -32700 | Parse error |
| -32600 | Invalid request |
| -32601 | Method not found |
| -32602 | Invalid params |
| -32001 | PathStructure not initialized |
| -32002 | Duplicate path regex |
