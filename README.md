# PathStructure

PathStructure is a C# library that validates file system paths against a configurable tree of regular-expression nodes and monitors Windows File Explorer selections. It is designed for enterprise environments where path naming conventions must be enforced and where a centralized watcher is needed to capture current Explorer focus and selections.

## Key Features

- **Regex-driven path validation**: Define a tree of path nodes using regular expressions with named captures; captured variables are enforced for consistency across the tree.
- **Explorer monitoring**: Polls open Windows File Explorer windows to detect focused items and emit events.
- **UNC path normalization**: Converts mapped drive paths to UNC paths via `GetUNCPath` for consistent comparisons.
- **Extensible validation rules**: Inject custom validation rules to add database lookups or advanced checks later.

## Usage

### Configure the Path Structure

```csharp
using PathStructure;

var root = new PathNode(
    "Root",
    @"^\\\\Server\\Customers\\(?<CustomerName>[^\\]+)\\");

root.Children.Add(new PathNode(
    "Taxes",
    @"^Taxes\\(?<Year>\d{4})-(?<Month>\d{2})-(?<Day>\d{2})_(?<CustomerName>[^_]+)_K1\.pdf$"));

var config = new PathStructureConfig(root)
{
    RegexOptions = System.Text.RegularExpressions.RegexOptions.IgnoreCase
};

var pathStructure = new PathStructure(config);
var result = pathStructure.ValidatePath(@"\\Server\Customers\OpenAI\Taxes\2026-01-25_OpenAI_K1.pdf");

if (!result.IsValid)
{
    Console.WriteLine(result.Error);
}
```

### Monitor Explorer Windows

```csharp
using PathStructure;

var watcher = new ExplorerWatcher(pathStructure, new ExplorerWatcherOptions
{
    PollRateMs = 500
});

watcher.ExplorerWatcherFound += url =>
{
    Console.WriteLine($"Focused item: {url}");
};

watcher.StartWatcher();
```

## Project Layout

- `PathStructure*` classes define the validation engine and configuration model.
- `ExplorerWatcher*` classes provide Explorer monitoring and events.
- `PathStructure_Helpers` includes UNC normalization utilities and logging hooks.
- `PathStructure.Abstracts` defines plugin contracts and discovery helpers.
- `ExamplePlugins` includes sample plugin implementations for validation.
- `PathStructureClient` contains the Electron-based client shell.

## Notes

- The library is intended to be generic and configurable; it does not include ERP integrations.
- Ensure COM references for `Shell32` and `SHDocVw` are available when using `ExplorerWatcher`.
