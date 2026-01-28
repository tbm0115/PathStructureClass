using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text.Json;
using PathStructure.Abstracts;

namespace PathStructure
{
    /// <summary>
    /// Loads and processes JSON-based path structure configurations.
    /// </summary>
    public static class PathStructureConfigLoader
    {
        private const string RootPattern = ".*";
        private static readonly Regex NamedGroupRegex = new Regex(@"\(\?<(?<name>[A-Za-z][A-Za-z0-9_]*)>", RegexOptions.Compiled);
        private static readonly Regex TemplateTokenRegex = new Regex(@"\{\{\s*(?<name>[^}]+)\s*\}\}", RegexOptions.Compiled);
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Loads a configuration file, resolving any imports and producing a root node.
        /// </summary>
        public static PathStructureConfig LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path was required.", nameof(filePath));
            }

            var fullPath = Path.GetFullPath(filePath);
            var activeImports = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return LoadFromFileInternal(fullPath, activeImports);
        }

        private static PathStructureConfig LoadFromFileInternal(string filePath, HashSet<string> activeImports)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Config file not found at '{filePath}'.", filePath);
            }

            if (!activeImports.Add(filePath))
            {
                throw new InvalidOperationException($"Circular configuration import detected at '{filePath}'.");
            }

            var rawJson = File.ReadAllText(filePath);
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            var config = JsonSerializer.Deserialize<PathStructureConfig>(rawJson, serializerOptions) ?? new PathStructureConfig();

            config.Imports = config.Imports ?? new List<PathStructureImport>();
            config.Paths = config.Paths ?? new List<PathStructurePath>();
            config.Plugins = config.Plugins ?? new List<PathStructurePlugin>();

            var mergedPaths = new List<PathStructurePath>();
            var mergedPlugins = new List<PathStructurePlugin>();
            var baseDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

            foreach (var import in config.Imports)
            {
                if (string.IsNullOrWhiteSpace(import?.Path))
                {
                    continue;
                }

                var importPath = import.Path;
                PathStructureConfig importedConfig;
                if (TryGetUri(importPath, out var importUri))
                {
                    importedConfig = LoadFromUrlInternal(importUri, activeImports);
                }
                else
                {
                    if (!Path.IsPathRooted(importPath))
                    {
                        importPath = Path.GetFullPath(Path.Combine(baseDirectory, importPath));
                    }

                    importedConfig = LoadFromFileInternal(importPath, activeImports);
                }
                var importedPaths = ApplyImportRoot(importedConfig.Paths, import.RootPath);
                mergedPaths.AddRange(ApplyNamespace(importedPaths, import.Namespace));
                mergedPlugins.AddRange(importedConfig.Plugins);
            }

            mergedPaths.AddRange(config.Paths);
            mergedPlugins.AddRange(config.Plugins);

            config.Paths = mergedPaths;
            config.Plugins = mergedPlugins;
            config.SetRoot(BuildRootNode(config.Paths));

            activeImports.Remove(filePath);
            return config;
        }

        private static PathStructureConfig LoadFromUrlInternal(Uri uri, HashSet<string> activeImports)
        {
            var normalized = uri.AbsoluteUri;
            if (!activeImports.Add(normalized))
            {
                throw new InvalidOperationException($"Circular configuration import detected at '{normalized}'.");
            }

            var rawJson = HttpClient.GetStringAsync(uri).GetAwaiter().GetResult();
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            var config = JsonSerializer.Deserialize<PathStructureConfig>(rawJson, serializerOptions) ?? new PathStructureConfig();

            config.Imports = config.Imports ?? new List<PathStructureImport>();
            config.Paths = config.Paths ?? new List<PathStructurePath>();
            config.Plugins = config.Plugins ?? new List<PathStructurePlugin>();

            var mergedPaths = new List<PathStructurePath>();
            var mergedPlugins = new List<PathStructurePlugin>();

            foreach (var import in config.Imports)
            {
                if (string.IsNullOrWhiteSpace(import?.Path))
                {
                    continue;
                }

                var importPath = import.Path;
                PathStructureConfig importedConfig;
                if (TryGetUri(importPath, out var importUri))
                {
                    importedConfig = LoadFromUrlInternal(importUri, activeImports);
                }
                else if (Path.IsPathRooted(importPath))
                {
                    importedConfig = LoadFromFileInternal(importPath, activeImports);
                }
                else
                {
                    var resolvedUri = new Uri(uri, importPath);
                    importedConfig = LoadFromUrlInternal(resolvedUri, activeImports);
                }

                var importedPaths = ApplyImportRoot(importedConfig.Paths, import.RootPath);
                mergedPaths.AddRange(ApplyNamespace(importedPaths, import.Namespace));
                mergedPlugins.AddRange(importedConfig.Plugins);
            }

            mergedPaths.AddRange(config.Paths);
            mergedPlugins.AddRange(config.Plugins);

            config.Paths = mergedPaths;
            config.Plugins = mergedPlugins;
            config.SetRoot(BuildRootNode(config.Paths));

            activeImports.Remove(normalized);
            return config;
        }

        private static bool TryGetUri(string value, out Uri uri)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                uri = null;
                return false;
            }

            if (Uri.TryCreate(value, UriKind.Absolute, out uri))
            {
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }

            uri = null;
            return false;
        }

        private static IPathNode BuildRootNode(IEnumerable<PathStructurePath> paths)
        {
            var root = new PathNode("Root", RootPattern, null, null, null, null, false);
            if (paths == null)
            {
                return root;
            }

            foreach (var path in paths)
            {
                var child = BuildPathNode(path);
                if (child != null)
                {
                    root.Children.Add(child);
                }
            }

            return root;
        }

        private static PathNode BuildPathNode(PathStructurePath path)
        {
            if (string.IsNullOrWhiteSpace(path?.Regex))
            {
                return null;
            }

            var name = string.IsNullOrWhiteSpace(path.Name) ? path.Regex.Trim() : path.Name.Trim();
            var node = new PathNode(
                name,
                path.Regex,
                path.FlavorTextTemplate,
                path.BackgroundColor,
                path.ForegroundColor,
                path.Icon,
                path.IsRequired);

            if (path.Paths == null)
            {
                return node;
            }

            foreach (var childPath in path.Paths)
            {
                var child = BuildPathNode(childPath);
                if (child != null)
                {
                    node.Children.Add(child);
                }
            }

            return node;
        }

        private static IEnumerable<PathStructurePath> ApplyNamespace(IEnumerable<PathStructurePath> paths, string importNamespace)
        {
            if (paths == null)
            {
                yield break;
            }

            var prefix = NormalizeNamespace(importNamespace);
            foreach (var path in paths)
            {
                if (path == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(prefix))
                {
                    yield return path;
                    continue;
                }

                yield return ApplyNamespaceToPath(path, prefix);
            }
        }

        private static IEnumerable<PathStructurePath> ApplyImportRoot(IEnumerable<PathStructurePath> paths, string importRoot)
        {
            if (paths == null)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(importRoot))
            {
                foreach (var path in paths)
                {
                    if (path != null)
                    {
                        yield return path;
                    }
                }

                yield break;
            }

            foreach (var path in paths)
            {
                if (path == null)
                {
                    continue;
                }

                yield return ApplyImportRootToPath(path, importRoot);
            }
        }

        private static PathStructurePath ApplyImportRootToPath(PathStructurePath path, string importRoot)
        {
            if (path == null)
            {
                return null;
            }

            return new PathStructurePath
            {
                Regex = ApplyImportRootToRegex(path.Regex, importRoot),
                Name = path.Name,
                FlavorTextTemplate = path.FlavorTextTemplate,
                BackgroundColor = path.BackgroundColor,
                ForegroundColor = path.ForegroundColor,
                Icon = path.Icon,
                IsRequired = path.IsRequired,
                Paths = path.Paths
            };
        }

        private static string ApplyImportRootToRegex(string regex, string importRoot)
        {
            if (string.IsNullOrWhiteSpace(regex) || string.IsNullOrWhiteSpace(importRoot))
            {
                return regex;
            }

            var trimmedRoot = importRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(trimmedRoot))
            {
                return regex;
            }

            var prefix = Regex.Escape($"{trimmedRoot}{Path.DirectorySeparatorChar}");
            if (regex.StartsWith("^", StringComparison.Ordinal))
            {
                return "^" + prefix + regex.Substring(1);
            }

            return prefix + regex;
        }

        private static PathStructurePath ApplyNamespaceToPath(PathStructurePath path, string prefix)
        {
            if (path == null)
            {
                return null;
            }

            var namespaced = new PathStructurePath
            {
                Regex = ApplyNamespaceToRegex(path.Regex, prefix),
                Name = path.Name,
                FlavorTextTemplate = ApplyNamespaceToTemplate(path.FlavorTextTemplate, prefix),
                BackgroundColor = path.BackgroundColor,
                ForegroundColor = path.ForegroundColor,
                Icon = path.Icon,
                IsRequired = path.IsRequired
            };

            if (path.Paths == null)
            {
                return namespaced;
            }

            foreach (var child in path.Paths)
            {
                var namespacedChild = ApplyNamespaceToPath(child, prefix);
                if (namespacedChild != null)
                {
                    namespaced.Paths.Add(namespacedChild);
                }
            }

            return namespaced;
        }

        private static string ApplyNamespaceToRegex(string regex, string prefix)
        {
            if (string.IsNullOrWhiteSpace(regex))
            {
                return regex;
            }

            return NamedGroupRegex.Replace(regex, match =>
            {
                var name = match.Groups["name"].Value;
                // Do not close the group here; the remainder of the regex includes the closing parenthesis.
                return $"(?<{prefix}{name}>";
            });
        }

        private static string ApplyNamespaceToTemplate(string template, string prefix)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return template;
            }

            return TemplateTokenRegex.Replace(template, match =>
            {
                var name = match.Groups["name"].Value.Trim();
                return $"{{{{ {prefix}{name} }}}}";
            });
        }

        private static string NormalizeNamespace(string importNamespace)
        {
            if (string.IsNullOrWhiteSpace(importNamespace))
            {
                return null;
            }

            var normalized = Regex.Replace(importNamespace.Trim(), @"\W", "_");
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            if (char.IsDigit(normalized[0]))
            {
                normalized = $"ns_{normalized}";
            }

            return normalized + "_";
        }

    }
}
