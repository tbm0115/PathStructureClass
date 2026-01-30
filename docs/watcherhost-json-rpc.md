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

Adds a path regex to the configuration. If the current selection matches a folder, the new path is added beneath that match. If the current selection matches a file, the new path is added beneath the file's parent path structure. If there is no matching selection, the new path is added at the root level.

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

### `importPathStructure`

Registers an import configuration file. If `filePath` is provided, the watcher host copies the file into `%LOCALAPPDATA%\\PathStructure` and stores the copied path in the imports list. If `url` is provided, the URL is stored in the imports list.

```json
{"jsonrpc":"2.0","id":"2","method":"importPathStructure","params":{"filePath":"C:\\\\Configs\\\\AdditionalPaths.json"}}
```

```json
{"jsonrpc":"2.0","id":"3","method":"importPathStructure","params":{"url":"https://example.com/pathstructure.json"}}
```

#### Successful response

```json
{"jsonrpc":"2.0","id":"2","result":{"message":"Import added.","importPath":"C:\\\\Users\\\\User\\\\AppData\\\\Local\\\\PathStructure\\\\AdditionalPaths.json"}}
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
| -32003 | Missing config file path |
| -32004 | Unable to load config file |
| -32005 | Unable to save config file |
| -32006 | Import file not found |
| -32007 | Unable to copy import file |
| -32008 | Import not found |
