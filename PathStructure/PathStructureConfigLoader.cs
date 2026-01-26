using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using PathStructure.Abstracts;

namespace PathStructure
{
    /// <summary>
    /// Loads and processes JSON-based path structure configurations.
    /// </summary>
    public static class PathStructureConfigLoader
    {
        private const string RootPattern = "^";
        private static readonly Regex NamedGroupRegex = new Regex(@"\(\?<(?<name>[A-Za-z][A-Za-z0-9_]*)>", RegexOptions.Compiled);
        private static readonly Regex TemplateTokenRegex = new Regex(@"\{\{\s*(?<name>[^}]+)\s*\}\}", RegexOptions.Compiled);

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
            var sanitizedJson = StripJsonComments(rawJson);
            var serializer = new JavaScriptSerializer();
            var config = serializer.Deserialize<PathStructureConfig>(sanitizedJson) ?? new PathStructureConfig();

            config.Imports = config.Imports ?? new List<PathStructureImport>();
            config.Paths = config.Paths ?? new List<PathStructurePath>();
            config.Plugins = config.Plugins ?? new List<PathStructurePlugin>();
            foreach (var plugin in config.Plugins)
            {
                if (plugin != null && plugin.Options == null)
                {
                    plugin.Options = new Dictionary<string, object>();
                }
            }

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
                if (!Path.IsPathRooted(importPath))
                {
                    importPath = Path.GetFullPath(Path.Combine(baseDirectory, importPath));
                }

                var importedConfig = LoadFromFileInternal(importPath, activeImports);
                mergedPaths.AddRange(ApplyNamespace(importedConfig.Paths, import.Namespace));
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

        private static IPathNode BuildRootNode(IEnumerable<PathStructurePath> paths)
        {
            var root = new PathNode("Root", RootPattern);
            if (paths == null)
            {
                return root;
            }

            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path?.Regex))
                {
                    continue;
                }

                var name = path.Regex.Trim();
                var child = new PathNode(
                    name,
                    path.Regex,
                    path.FlavorTextTemplate,
                    path.BackgroundColor,
                    path.ForegroundColor,
                    path.Icon);
                root.Children.Add(child);
            }

            return root;
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

                yield return new PathStructurePath
                {
                    Regex = ApplyNamespaceToRegex(path.Regex, prefix),
                    FlavorTextTemplate = ApplyNamespaceToTemplate(path.FlavorTextTemplate, prefix),
                    BackgroundColor = path.BackgroundColor,
                    ForegroundColor = path.ForegroundColor,
                    Icon = path.Icon
                };
            }
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

        private static string StripJsonComments(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            var builder = new StringBuilder(json.Length);
            var inString = false;
            var inSingleLineComment = false;
            var inMultiLineComment = false;

            for (var i = 0; i < json.Length; i++)
            {
                var current = json[i];
                var next = i + 1 < json.Length ? json[i + 1] : '\0';

                if (inSingleLineComment)
                {
                    if (current == '\n')
                    {
                        inSingleLineComment = false;
                        builder.Append(current);
                    }

                    continue;
                }

                if (inMultiLineComment)
                {
                    if (current == '*' && next == '/')
                    {
                        inMultiLineComment = false;
                        i++;
                    }

                    continue;
                }

                if (inString)
                {
                    if (current == '\\' && i + 1 < json.Length)
                    {
                        builder.Append(current);
                        builder.Append(json[++i]);
                        continue;
                    }

                    if (current == '"')
                    {
                        inString = false;
                    }

                    builder.Append(current);
                    continue;
                }

                if (current == '"')
                {
                    inString = true;
                    builder.Append(current);
                    continue;
                }

                if (current == '/' && next == '/')
                {
                    inSingleLineComment = true;
                    i++;
                    continue;
                }

                if (current == '/' && next == '*')
                {
                    inMultiLineComment = true;
                    i++;
                    continue;
                }

                builder.Append(current);
            }

            return builder.ToString();
        }
    }
}
