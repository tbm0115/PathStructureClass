# PathStructure Configuration

PathStructure uses a JSON configuration file to describe valid paths, optional UI metadata, and plugins. The configuration loader supports `//` and `/* */` comments and trailing commas.

## Top-Level Schema

```json
{
  "imports": [
    {
      "path": "C:\\\\ExampleFolder\\\\PathStructureUseCaseConfig.json",
      "namespace": "x"
    }
  ],
  "paths": [
    {
      "regex": "^\\\\\\\\Server\\\\Customers\\\\(?<CustomerName>[^\\\\]+)\\\\Taxes\\\\(?<Year>\\\\d{4})-(?<Month>\\\\d{2})-(?<Day>\\\\d{2})_(?<CustomerName>[^_]+)_K1\\\\.pdf$",
      "flavorTextTemplate": "Tax document for {{ CustomerName }} on {{ Year }}-{{ Month }}-{{ Day }}.",
      "backgroundColor": "white",
      "foregroundColor": "black",
      "icon": "C:\\\\ExampleFolder\\\\CustomerProfile.png"
    }
  ],
  "plugins": [
    {
      "path": "C:\\\\ExampleFolder\\\\Plugin.dll",
      "options": {
        "endpoint": "https://api.example.com",
        "enabled": true
      }
    }
  ]
}
```

### `imports`

List of other configuration files to load and merge. Imports are processed before the local file, so local entries override imported entries if your client merges them.

- `path` (string, required): path to the JSON configuration file. Relative paths are resolved relative to the current file.
- `namespace` (string, optional): namespace prefix applied to variables from the imported file. The loader prefixes named capture groups and template tokens with `<namespace>_` to avoid collisions.

### `paths`

List of regex-based path entries. Each entry describes a full-path regex pattern and optional metadata.

- `regex` (string, required): C# regex pattern for the full path. Use named capture groups (`(?<Name>...)`) to extract variables.
- `flavorTextTemplate` (string, optional): a text template that supports `{{ VariableName }}` tokens.
- `backgroundColor` (string, optional): background color for UI clients.
- `foregroundColor` (string, optional): foreground color for UI clients.
- `icon` (string, optional): path to an icon for UI clients.

### `plugins`

List of plugin assemblies to load.

- `path` (string, required): path to the plugin DLL.
- `options` (object, optional): plugin options passed as key/value pairs.

## Usage

```csharp
using PathStructure;

var config = PathStructureConfigLoader.LoadFromFile("C:\\\\Configs\\\\PathStructure.json");
var pathStructure = new PathStructure(config);
```

The loader resolves imports, applies namespace prefixes, and builds a root node so the `PathStructure` validator can use the configured paths.

## Examples

- `docs/tams-mtconnect-config.json` provides a ready-to-use configuration for the TAMS MTConnect filesystem layout.
