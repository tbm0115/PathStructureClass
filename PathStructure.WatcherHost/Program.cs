using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PathStructure;
using PathStructure.Abstracts;
using PathStructure.WatcherHost.Dtos;

namespace PathStructure.WatcherHost
{
    /// <summary>
    /// Hosts the Explorer watcher and streams JSON-RPC notifications/responses to clients.
    /// </summary>
    internal class Program
    {
        private const int DefaultPort = 49321;
        private const string DefaultConfigFileName = "pathstructure-default.json";
        private static readonly ConcurrentDictionary<TcpClient, NetworkStream> Clients = new ConcurrentDictionary<TcpClient, NetworkStream>();
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static ExplorerWatcher _watcher;
        private static TcpListener _listener;
        private static PathStructure _pathStructure;
        private static PathStructureConfig _pathConfig;
        private static string _configFilePath;
        private static string _defaultConfigFilePath;
        private static string _lastSelectionPath;
        private static readonly object ConfigSync = new object();
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private static readonly JsonSerializerOptions ConfigReadOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        private static readonly JsonSerializerOptions ConfigWriteOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        /// <summary>
        /// Entry point for the watcher host process.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            var port = DefaultPort;
            string configPath = null;
            foreach (var arg in args)
            {
                if (port == DefaultPort && int.TryParse(arg, out var parsedPort))
                {
                    port = parsedPort;
                }
                else if (configPath == null)
                {
                    configPath = arg;
                }
            }

            Console.WriteLine($"Starting PathStructure.WatcherHost on port {port}.");

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Stop();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Stop();

            StartWatcher(configPath);
            StartServerAsync(port, _cts.Token).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initializes the Explorer watcher and begins polling.
        /// </summary>
        private static void StartWatcher(string configPath)
        {
            PathStructureConfig config;
            if (!string.IsNullOrWhiteSpace(configPath))
            {
                var resolvedPath = Path.GetFullPath(configPath);
                Console.WriteLine($"Loading configuration from {resolvedPath}.");
                config = PathStructureConfigLoader.LoadFromFile(resolvedPath);
                _configFilePath = resolvedPath;
            }
            else
            {
                var resolvedPath = EnsureDefaultConfigFile();
                Console.WriteLine($"Loading configuration from {resolvedPath}.");
                config = PathStructureConfigLoader.LoadFromFile(resolvedPath);
                _configFilePath = resolvedPath;
            }
            _pathConfig = config;
            _pathStructure = new PathStructure(config);

            _watcher = new ExplorerWatcher(_pathStructure, new ExplorerWatcherOptions
            {
                PollRateMs = 500
            });

            _watcher.ExplorerWatcherFound += OnExplorerFound;
            _watcher.ExplorerWatcherError += (sender, exception) =>
            {
                BroadcastNotification(new JsonRpcNotification<WatcherErrorNotificationParams>
                {
                    Method = "watcherError",
                    Params = new WatcherErrorNotificationParams
                    {
                        Message = "Explorer watcher error.",
                        Error = exception?.Message,
                        Timestamp = DateTimeOffset.Now.ToString("o")
                    }
                });
            };
            _watcher.ExplorerWatcherAborted += (sender, exceptionArgs) =>
            {
                BroadcastNotification(new JsonRpcNotification<WatcherAbortedNotificationParams>
                {
                    Method = "watcherAborted",
                    Params = new WatcherAbortedNotificationParams
                    {
                        Message = "Explorer watcher aborted.",
                        Error = exceptionArgs?.ToString(),
                        Timestamp = DateTimeOffset.Now.ToString("o")
                    }
                });
            };

            _watcher.StartWatcher();
        }

        /// <summary>
        /// Starts the TCP listener that accepts JSON-RPC client connections.
        /// </summary>
        private static async Task StartServerAsync(int port, CancellationToken token)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _listener.Start();
            Console.WriteLine("Watcher host listening.");

            while (!token.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                var stream = client.GetStream();
                Clients.TryAdd(client, stream);
                Console.WriteLine("Client connected.");

                var statusPayload = new JsonRpcNotification<StatusNotificationParams>
                {
                    Method = "status",
                    Params = new StatusNotificationParams
                    {
                        Message = "Client connected.",
                        State = "connected",
                        Timestamp = DateTimeOffset.Now.ToString("o")
                    }
                };
                await SendAsync(stream, SerializeJson(statusPayload)).ConfigureAwait(false);
                _ = Task.Run(() => MonitorClientAsync(client, stream, token));
            }
        }

        /// <summary>
        /// Reads newline-delimited JSON-RPC requests from the client.
        /// </summary>
        private static async Task MonitorClientAsync(TcpClient client, NetworkStream stream, CancellationToken token)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!client.Connected)
                    {
                        break;
                    }

                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    await HandleClientCommandAsync(line, client, stream).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                RemoveClient(client);
            }
        }

        /// <summary>
        /// Handles Explorer selection updates and broadcasts path change notifications.
        /// </summary>
        private static void OnExplorerFound(string url)
        {
            _lastSelectionPath = url;
            var monitoredPath = GetParentPath(url) ?? url;
            var matchSummary = FindClosestMatches(monitoredPath);
            IReadOnlyList<PathPatternMatch> matches = Array.Empty<PathPatternMatch>();
            var hasMatches = TryGetMatchTrail(monitoredPath, out matches, out var variables);
            if (!hasMatches && IsFilePath(monitoredPath))
            {
                hasMatches = TryGetMatchTrail(Path.GetDirectoryName(monitoredPath), out matches, out variables);
            }
            var currentMatch = hasMatches ? matches[0] : default;
            var childMatches = hasMatches
                ? FindImmediateChildMatches(new[] { currentMatch })
                : FindImmediateChildMatches(monitoredPath, matchSummary);
            var matchesPayload = hasMatches
                ? matches.Select(match =>
                {
                    var dto = BuildPathMatchDto(match);
                    dto.ChildMatches = FindImmediateChildMatches(new[] { match })
                        .Select(BuildPathMatchDto)
                        .ToList();
                    return dto;
                }).ToList()
                : new List<PathMatchDto>();
#if DEBUG
            LogMatches(url, matchSummary);
#endif
            BroadcastNotification(new JsonRpcNotification<PathChangedNotificationParams>
            {
                Method = "pathChanged",
                Params = new PathChangedNotificationParams
                {
                    Message = "Explorer path changed.",
                    Path = monitoredPath,
                    CurrentMatch = hasMatches ? BuildPathMatchDto(currentMatch) : null,
                    Variables = variables,
                    Matches = matchesPayload,
                    ImmediateChildMatches = childMatches.Select(BuildPathMatchDto).ToList(),
                    Timestamp = DateTimeOffset.Now.ToString("o")
                }
            });
        }

        /// <summary>
        /// Builds a DTO for JSON-RPC payloads from a path pattern match.
        /// </summary>
        private static PathMatchDto BuildPathMatchDto(PathPatternMatch match)
        {
            return new PathMatchDto
            {
                Name = match.NodeName,
                Pattern = match.Pattern,
                MatchedValue = match.MatchedValue,
                MatchLength = match.MatchLength,
                FlavorTextTemplate = match.FlavorTextTemplate,
                BackgroundColor = match.BackgroundColor,
                ForegroundColor = match.ForegroundColor,
                Icon = match.Icon,
                IsRequired = match.IsRequired
            };
        }

        /// <summary>
        /// Broadcasts a JSON-RPC notification payload to all connected clients.
        /// </summary>
        private static void BroadcastNotification(object payload)
        {
            var serialized = SerializeJson(payload);
            foreach (var client in Clients)
            {
                _ = SendAsync(client.Value, serialized);
            }
        }

        /// <summary>
        /// Serializes payloads to newline-delimited JSON for transport.
        /// </summary>
        private static string SerializeJson(object payload)
        {
            return JsonSerializer.Serialize(payload, JsonOptions) + "\n";
        }

        /// <summary>
        /// Writes a payload to a client stream.
        /// </summary>
        private static async Task SendAsync(NetworkStream stream, string payload)
        {
            if (stream == null || !stream.CanWrite)
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(payload);
            try
            {
                await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Removes a client and disposes its stream.
        /// </summary>
        private static void RemoveClient(TcpClient client)
        {
            if (client == null)
            {
                return;
            }

            if (Clients.TryRemove(client, out var stream))
            {
                stream?.Dispose();
            }

            client.Close();
        }

        /// <summary>
        /// Stops the watcher and listener.
        /// </summary>
        private static void Stop()
        {
            if (_cts.IsCancellationRequested)
            {
                return;
            }

            _cts.Cancel();

            if (_listener != null)
            {
                _listener.Stop();
            }

            if (_watcher != null)
            {
                _watcher.StopWatcher();
            }

            foreach (var client in Clients.Keys)
            {
                RemoveClient(client);
            }
        }

        /// <summary>
        /// Handles a JSON-RPC request from a client.
        /// </summary>
        private static async Task HandleClientCommandAsync(string payload, TcpClient client, NetworkStream stream)
        {
            try
            {
                var request = JsonSerializer.Deserialize<JsonRpcRequest>(payload, JsonOptions);
                if (request == null || string.IsNullOrWhiteSpace(request.Method))
                {
                    await SendJsonRpcErrorAsync(stream, null, -32600, "Invalid Request", payload).ConfigureAwait(false);
                    return;
                }

                switch (request.Method)
                {
                    case "addPath":
                        await HandleAddPathCommandAsync(request, stream).ConfigureAwait(false);
                        break;
                    case "importPathStructure":
                        await HandleImportPathCommandAsync(request, stream).ConfigureAwait(false);
                        break;
                    case "listImports":
                        await HandleListImportsCommandAsync(request, stream).ConfigureAwait(false);
                        break;
                    case "updateImport":
                        await HandleUpdateImportCommandAsync(request, stream).ConfigureAwait(false);
                        break;
                    case "removeImport":
                        await HandleRemoveImportCommandAsync(request, stream).ConfigureAwait(false);
                        break;
                    default:
                        await SendJsonRpcErrorAsync(stream, request.Id, -32601, $"Unknown method '{request.Method}'.", null).ConfigureAwait(false);
                        break;
                }
            }
            catch (JsonException ex)
            {
                await SendJsonRpcErrorAsync(stream, null, -32700, "Parse error", ex.Message).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handles the JSON-RPC addPath request.
        /// </summary>
        private static async Task HandleAddPathCommandAsync(JsonRpcRequest request, NetworkStream stream)
        {
            if (_pathConfig == null)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32001, "PathStructure is not initialized.", null).ConfigureAwait(false);
                return;
            }

            if (request.Params.ValueKind != JsonValueKind.Object || !request.Params.TryGetProperty("regex", out var regexElement))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32602, "Missing required 'regex' property.", null).ConfigureAwait(false);
                return;
            }

            var regex = regexElement.GetString();
            if (string.IsNullOrWhiteSpace(regex))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32602, "Provided regex was empty.", null).ConfigureAwait(false);
                return;
            }

            if (ContainsRegex(_pathConfig.Paths, regex))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32002, "Path regex already exists.", regex).ConfigureAwait(false);
                return;
            }

            var newPath = new PathStructurePath
            {
                Regex = regex,
                Name = GetOptionalString(request.Params, "name") ?? regex.Trim(),
                FlavorTextTemplate = GetOptionalString(request.Params, "flavorTextTemplate"),
                BackgroundColor = GetOptionalString(request.Params, "backgroundColor"),
                ForegroundColor = GetOptionalString(request.Params, "foregroundColor"),
                Icon = GetOptionalString(request.Params, "icon"),
                IsRequired = GetOptionalBool(request.Params, "isRequired")
            };

            var configPath = GetActiveConfigPath();
            if (string.IsNullOrWhiteSpace(configPath))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32003, "Unable to determine config file path.", null).ConfigureAwait(false);
                return;
            }

            PathStructureConfig rawConfig;
            try
            {
                rawConfig = LoadRawConfig(configPath);
            }
            catch (Exception ex)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32004, "Unable to load config file.", ex.Message).ConfigureAwait(false);
                return;
            }

            var addedToRoot = true;
            var parentPath = ResolveTargetPathForSelection(rawConfig, out var targetMatchDescription, out var targetName);
            if (parentPath != null)
            {
                parentPath.Paths = parentPath.Paths ?? new List<PathStructurePath>();
                parentPath.Paths.Add(newPath);
                addedToRoot = false;
            }
            else
            {
                rawConfig.Paths.Add(newPath);
            }

            try
            {
                SaveConfig(configPath, rawConfig);
                ReloadConfiguration(configPath);
            }
            catch (Exception ex)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32005, "Unable to save config file.", ex.Message).ConfigureAwait(false);
                return;
            }

            await SendJsonRpcResultAsync(stream, request.Id, new
            {
                message = addedToRoot
                    ? "Path regex added at the root level."
                    : $"Path regex added under {targetName ?? targetMatchDescription ?? "matched path"}.",
                path = regex,
                parentMatch = addedToRoot ? null : targetMatchDescription
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the JSON-RPC importPathStructure request.
        /// </summary>
        private static async Task HandleImportPathCommandAsync(JsonRpcRequest request, NetworkStream stream)
        {
            if (request.Params.ValueKind != JsonValueKind.Object)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32602, "Missing request parameters.", null).ConfigureAwait(false);
                return;
            }

            var filePath = GetOptionalString(request.Params, "filePath");
            var url = GetOptionalString(request.Params, "url");
            var importNamespace = NormalizeOptionalString(GetOptionalString(request.Params, "namespace"));
            var rootPath = NormalizeOptionalString(GetOptionalString(request.Params, "rootPath"));
            if (string.IsNullOrWhiteSpace(filePath) && string.IsNullOrWhiteSpace(url))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32602, "Provide a filePath or url.", null).ConfigureAwait(false);
                return;
            }

            var defaultConfigPath = EnsureDefaultConfigFile();
            PathStructureConfig rawConfig;
            try
            {
                rawConfig = LoadRawConfig(defaultConfigPath);
            }
            catch (Exception ex)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32004, "Unable to load config file.", ex.Message).ConfigureAwait(false);
                return;
            }

            string importedLocation;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                if (!File.Exists(filePath))
                {
                    await SendJsonRpcErrorAsync(stream, request.Id, -32006, "Import file not found.", filePath).ConfigureAwait(false);
                    return;
                }

                try
                {
                    importedLocation = CopyImportFile(filePath, defaultConfigPath);
                }
                catch (Exception ex)
                {
                    await SendJsonRpcErrorAsync(stream, request.Id, -32007, "Unable to copy import file.", ex.Message).ConfigureAwait(false);
                    return;
                }
            }
            else
            {
                if (!TryGetHttpUrl(url, out var normalizedUrl))
                {
                    await SendJsonRpcErrorAsync(stream, request.Id, -32602, "Provided url was invalid.", url).ConfigureAwait(false);
                    return;
                }

                importedLocation = normalizedUrl;
            }

            if (rawConfig.Imports.Any(import => string.Equals(import.Path, importedLocation, StringComparison.OrdinalIgnoreCase)))
            {
                await SendJsonRpcResultAsync(stream, request.Id, new
                {
                    message = "Import already registered.",
                    importPath = importedLocation
                }).ConfigureAwait(false);
                return;
            }

            rawConfig.Imports.Add(new PathStructureImport
            {
                Path = importedLocation,
                Namespace = importNamespace,
                RootPath = rootPath
            });

            try
            {
                SaveConfig(defaultConfigPath, rawConfig);
                if (string.Equals(defaultConfigPath, _configFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    ReloadConfiguration(defaultConfigPath);
                }
            }
            catch (Exception ex)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32005, "Unable to save config file.", ex.Message).ConfigureAwait(false);
                return;
            }

            await SendJsonRpcResultAsync(stream, request.Id, new
            {
                message = "Import added.",
                importPath = importedLocation
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the JSON-RPC listImports request.
        /// </summary>
        private static async Task HandleListImportsCommandAsync(JsonRpcRequest request, NetworkStream stream)
        {
            var configPath = GetActiveConfigPath();
            if (string.IsNullOrWhiteSpace(configPath))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32003, "Unable to determine config file path.", null).ConfigureAwait(false);
                return;
            }

            PathStructureConfig rawConfig;
            try
            {
                rawConfig = LoadRawConfig(configPath);
            }
            catch (Exception ex)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32004, "Unable to load config file.", ex.Message).ConfigureAwait(false);
                return;
            }

            var imports = rawConfig.Imports?.Select(import => new
            {
                path = import.Path,
                @namespace = import.Namespace,
                rootPath = import.RootPath
            }) ?? Enumerable.Empty<object>();

            await SendJsonRpcResultAsync(stream, request.Id, new
            {
                imports
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the JSON-RPC updateImport request.
        /// </summary>
        private static async Task HandleUpdateImportCommandAsync(JsonRpcRequest request, NetworkStream stream)
        {
            if (request.Params.ValueKind != JsonValueKind.Object)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32602, "Missing request parameters.", null).ConfigureAwait(false);
                return;
            }

            var importPath = GetOptionalString(request.Params, "path");
            if (string.IsNullOrWhiteSpace(importPath))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32602, "Missing required 'path' property.", null).ConfigureAwait(false);
                return;
            }

            var configPath = GetActiveConfigPath();
            if (string.IsNullOrWhiteSpace(configPath))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32003, "Unable to determine config file path.", null).ConfigureAwait(false);
                return;
            }

            PathStructureConfig rawConfig;
            try
            {
                rawConfig = LoadRawConfig(configPath);
            }
            catch (Exception ex)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32004, "Unable to load config file.", ex.Message).ConfigureAwait(false);
                return;
            }

            var targetImport = rawConfig.Imports.FirstOrDefault(import =>
                string.Equals(import.Path, importPath, StringComparison.OrdinalIgnoreCase));

            if (targetImport == null)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32008, "Import not found.", importPath).ConfigureAwait(false);
                return;
            }

            if (request.Params.TryGetProperty("namespace", out var namespaceElement))
            {
                var value = namespaceElement.ValueKind == JsonValueKind.Null
                    ? null
                    : namespaceElement.GetString();
                targetImport.Namespace = NormalizeOptionalString(value);
            }

            if (request.Params.TryGetProperty("rootPath", out var rootPathElement))
            {
                var value = rootPathElement.ValueKind == JsonValueKind.Null
                    ? null
                    : rootPathElement.GetString();
                targetImport.RootPath = NormalizeOptionalString(value);
            }

            try
            {
                SaveConfig(configPath, rawConfig);
                if (string.Equals(configPath, _configFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    ReloadConfiguration(configPath);
                }
            }
            catch (Exception ex)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32005, "Unable to save config file.", ex.Message).ConfigureAwait(false);
                return;
            }

            await SendJsonRpcResultAsync(stream, request.Id, new
            {
                message = "Import updated.",
                import = new
                {
                    path = targetImport.Path,
                    @namespace = targetImport.Namespace,
                    rootPath = targetImport.RootPath
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the JSON-RPC removeImport request.
        /// </summary>
        private static async Task HandleRemoveImportCommandAsync(JsonRpcRequest request, NetworkStream stream)
        {
            if (request.Params.ValueKind != JsonValueKind.Object)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32602, "Missing request parameters.", null).ConfigureAwait(false);
                return;
            }

            var importPath = GetOptionalString(request.Params, "path");
            if (string.IsNullOrWhiteSpace(importPath))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32602, "Missing required 'path' property.", null).ConfigureAwait(false);
                return;
            }

            var configPath = GetActiveConfigPath();
            if (string.IsNullOrWhiteSpace(configPath))
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32003, "Unable to determine config file path.", null).ConfigureAwait(false);
                return;
            }

            PathStructureConfig rawConfig;
            try
            {
                rawConfig = LoadRawConfig(configPath);
            }
            catch (Exception ex)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32004, "Unable to load config file.", ex.Message).ConfigureAwait(false);
                return;
            }

            var removed = RemoveImport(rawConfig.Imports, importPath);

            if (removed == 0)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32008, "Import not found.", importPath).ConfigureAwait(false);
                return;
            }

            try
            {
                SaveConfig(configPath, rawConfig);
                if (string.Equals(configPath, _configFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    ReloadConfiguration(configPath);
                }
            }
            catch (Exception ex)
            {
                await SendJsonRpcErrorAsync(stream, request.Id, -32005, "Unable to save config file.", ex.Message).ConfigureAwait(false);
                return;
            }

            await SendJsonRpcResultAsync(stream, request.Id, new
            {
                message = "Import removed.",
                importPath
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads an optional string property from a JSON element.
        /// </summary>
        private static string GetOptionalString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var element) ? element.GetString() : null;
        }

        /// <summary>
        /// Reads an optional boolean property from a JSON element.
        /// </summary>
        private static bool GetOptionalBool(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var element) && element.ValueKind == JsonValueKind.True;
        }

        private static string NormalizeOptionalString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private static int RemoveImport(IList<PathStructureImport> imports, string importPath)
        {
            if (imports == null || string.IsNullOrWhiteSpace(importPath))
            {
                return 0;
            }

            var removed = 0;
            for (var index = imports.Count - 1; index >= 0; index -= 1)
            {
                var existing = imports[index];
                if (existing == null)
                {
                    continue;
                }

                if (!string.Equals(existing.Path, importPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                imports.RemoveAt(index);
                removed += 1;
            }

            return removed;
        }

        private static string GetActiveConfigPath()
        {
            return !string.IsNullOrWhiteSpace(_configFilePath) ? _configFilePath : EnsureDefaultConfigFile();
        }

        private static void ReloadConfiguration(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath))
            {
                return;
            }

            lock (ConfigSync)
            {
                var refreshed = PathStructureConfigLoader.LoadFromFile(configPath);
                _pathConfig = refreshed;
                _pathStructure = new PathStructure(refreshed);
            }
        }

        private static PathStructureConfig LoadRawConfig(string configPath)
        {
            var rawJson = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<PathStructureConfig>(rawJson, ConfigReadOptions) ?? new PathStructureConfig();
            config.Imports = config.Imports ?? new List<PathStructureImport>();
            config.Paths = config.Paths ?? new List<PathStructurePath>();
            config.Plugins = config.Plugins ?? new List<PathStructurePlugin>();
            config.Management = config.Management ?? new PathStructureManagementConfig();
            config.Management.Authorization = config.Management.Authorization ?? new PathStructureAuthorizationConfig();
            config.Management.UsageReporting = config.Management.UsageReporting ?? new PathStructureUsageReportingConfig();
            config.Management.Installation = config.Management.Installation ?? new PathStructureInstallationConfig();
            config.Models = config.Models ?? new List<PathStructureModel>();
            return config;
        }

        private static void SaveConfig(string configPath, PathStructureConfig config)
        {
            if (string.IsNullOrWhiteSpace(configPath) || config == null)
            {
                return;
            }

            var payload = new
            {
                imports = config.Imports ?? new List<PathStructureImport>(),
                paths = config.Paths ?? new List<PathStructurePath>(),
                plugins = config.Plugins ?? new List<PathStructurePlugin>(),
                management = config.Management ?? new PathStructureManagementConfig(),
                models = config.Models ?? new List<PathStructureModel>()
            };

            lock (ConfigSync)
            {
                var json = JsonSerializer.Serialize(payload, ConfigWriteOptions);
                File.WriteAllText(configPath, json);
            }
        }

        private static bool ContainsRegex(IEnumerable<PathStructurePath> paths, string regex)
        {
            if (paths == null || string.IsNullOrWhiteSpace(regex))
            {
                return false;
            }

            foreach (var path in paths)
            {
                if (path == null)
                {
                    continue;
                }

                if (string.Equals(path.Regex, regex, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (ContainsRegex(path.Paths, regex))
                {
                    return true;
                }
            }

            return false;
        }

        private static PathStructurePath ResolveTargetPathForSelection(
            PathStructureConfig rawConfig,
            out string targetPattern,
            out string targetName)
        {
            targetPattern = null;
            targetName = null;
            if (rawConfig == null)
            {
                return null;
            }

            if (!TryGetSelectionMatchTrail(out var matchTrail, out var isFileSelection))
            {
                return null;
            }

            var targetIndex = matchTrail.Count - 1;
            if (isFileSelection)
            {
                if (matchTrail.Count < 2)
                {
                    return null;
                }

                targetIndex = matchTrail.Count - 2;
            }

            var targetNode = matchTrail[targetIndex]?.Node;
            targetPattern = targetNode?.Pattern;
            targetName = targetNode?.Name;

            return FindOrCreateConfigPath(rawConfig, matchTrail, targetIndex);
        }

        private static bool TryGetSelectionMatchTrail(out IReadOnlyList<IPathMatchNode> matchTrail, out bool isFileSelection)
        {
            matchTrail = null;
            isFileSelection = false;

            if (_pathStructure == null || string.IsNullOrWhiteSpace(_lastSelectionPath))
            {
                return false;
            }

            var result = _pathStructure.ValidatePath(_lastSelectionPath);
            if (!result.IsValid || result.MatchTrail.Count == 0)
            {
                return false;
            }

            matchTrail = result.MatchTrail;
            isFileSelection = IsFilePath(_lastSelectionPath);
            return true;
        }

        private static PathStructurePath FindOrCreateConfigPath(
            PathStructureConfig rawConfig,
            IReadOnlyList<IPathMatchNode> matchTrail,
            int targetIndex)
        {
            if (rawConfig == null || matchTrail == null || targetIndex < 0 || targetIndex >= matchTrail.Count)
            {
                return null;
            }

            rawConfig.Paths = rawConfig.Paths ?? new List<PathStructurePath>();
            var currentPaths = rawConfig.Paths;
            PathStructurePath current = null;

            for (var index = 0; index <= targetIndex; index++)
            {
                var node = matchTrail[index]?.Node;
                var pattern = node?.Pattern;
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    return null;
                }

                current = currentPaths.FirstOrDefault(path =>
                    string.Equals(path.Regex, pattern, StringComparison.Ordinal));
                if (current == null)
                {
                    current = new PathStructurePath
                    {
                        Regex = pattern,
                        Name = node?.Name ?? pattern.Trim()
                    };
                    currentPaths.Add(current);
                }

                current.Paths = current.Paths ?? new List<PathStructurePath>();
                currentPaths = current.Paths;
            }

            return current;
        }

        private static string EnsureDefaultConfigFile()
        {
            if (!string.IsNullOrWhiteSpace(_defaultConfigFilePath) && File.Exists(_defaultConfigFilePath))
            {
                return _defaultConfigFilePath;
            }

            var baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PathStructure");
            Directory.CreateDirectory(baseDirectory);
            var configPath = Path.Combine(baseDirectory, DefaultConfigFileName);
            _defaultConfigFilePath = configPath;

            if (!File.Exists(configPath))
            {
                var defaultConfig = BuildDefaultConfig();
                SaveConfig(configPath, defaultConfig);
            }

            return configPath;
        }

        private static PathStructureConfig BuildDefaultConfig()
        {
            var defaultProfile = new PathStructureInstallationProfile
            {
                Id = "default",
                Name = "Default",
                Description = "Default managed installation profile."
            };

            var documents = new PathStructurePath
            {
                Regex = @"^Documents$",
                Name = "Documents",
                Paths = new List<PathStructurePath>
                {
                    new PathStructurePath { Regex = @"^.+\.txt$", Name = "Text file" },
                    new PathStructurePath { Regex = @"^.+\.docx$", Name = "Word document" },
                    new PathStructurePath { Regex = @"^.+\.xlsx$", Name = "Excel workbook" },
                    new PathStructurePath { Regex = @"^.+\.pdf$", Name = "PDF document" }
                }
            };

            var pictures = new PathStructurePath
            {
                Regex = @"^Pictures$",
                Name = "Pictures",
                Paths = new List<PathStructurePath>
                {
                    new PathStructurePath { Regex = @"^.+\.png$", Name = "PNG image" },
                    new PathStructurePath { Regex = @"^.+\.jpe?g$", Name = "JPEG image" }
                }
            };

            var videos = new PathStructurePath
            {
                Regex = @"^Videos$",
                Name = "Videos",
                Paths = new List<PathStructurePath>
                {
                    new PathStructurePath { Regex = @"^.+\.mp4$", Name = "MP4 video" }
                }
            };

            var music = new PathStructurePath
            {
                Regex = @"^Music$",
                Name = "Music",
                Paths = new List<PathStructurePath>
                {
                    new PathStructurePath { Regex = @"^.+\.mp3$", Name = "MP3 audio" }
                }
            };

            var userProfile = new PathStructurePath
            {
                Regex = @"^(?<UserName>[^\\]+)$",
                Name = "User Profile",
                Paths = new List<PathStructurePath> { documents, pictures, videos, music }
            };

            var users = new PathStructurePath
            {
                Regex = @"^Users$",
                Name = "Users",
                Paths = new List<PathStructurePath> { userProfile }
            };

            var programFiles = new PathStructurePath
            {
                Regex = @"^Program Files$",
                Name = "Program Files"
            };

            var programFilesX86 = new PathStructurePath
            {
                Regex = @"^Program Files \(x86\)$",
                Name = "Program Files (x86)"
            };

            var drive = new PathStructurePath
            {
                Regex = @"^[A-Z]:$",
                Name = "Drive",
                Paths = new List<PathStructurePath> { users, programFiles, programFilesX86 }
            };

            return new PathStructureConfig
            {
                Imports = new List<PathStructureImport>(),
                Paths = new List<PathStructurePath> { drive },
                Plugins = new List<PathStructurePlugin>(),
                Management = new PathStructureManagementConfig
                {
                    Authorization = new PathStructureAuthorizationConfig(),
                    UsageReporting = new PathStructureUsageReportingConfig
                    {
                        Required = false,
                        MinimumReportIntervalSeconds = 60
                    },
                    Installation = new PathStructureInstallationConfig
                    {
                        DefaultProfileId = defaultProfile.Id,
                        Profiles = new List<PathStructureInstallationProfile> { defaultProfile }
                    }
                },
                Models = new List<PathStructureModel>()
            };
        }

        private static string CopyImportFile(string sourcePath, string defaultConfigPath)
        {
            var baseDirectory = Path.GetDirectoryName(defaultConfigPath);
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new InvalidOperationException("Default config directory could not be resolved.");
            }

            var fileName = Path.GetFileName(sourcePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "pathstructure-import.json";
            }

            var destinationPath = Path.Combine(baseDirectory, fileName);
            var extension = Path.GetExtension(destinationPath);
            var baseName = Path.GetFileNameWithoutExtension(destinationPath);
            var counter = 1;
            while (File.Exists(destinationPath))
            {
                destinationPath = Path.Combine(baseDirectory, $"{baseName}-{counter}{extension}");
                counter++;
            }

            File.Copy(sourcePath, destinationPath, false);
            return destinationPath;
        }

        private static bool TryGetHttpUrl(string value, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }

            normalized = uri.AbsoluteUri;
            return true;
        }

        /// <summary>
        /// Builds a path node for runtime additions.
        /// </summary>
        private static PathNode BuildPathNode(PathStructurePath path)
        {
            var name = string.IsNullOrWhiteSpace(path.Name) ? path.Regex.Trim() : path.Name.Trim();
            return new PathNode(
                name,
                path.Regex,
                path.FlavorTextTemplate,
                path.BackgroundColor,
                path.ForegroundColor,
                path.Icon,
                path.IsRequired);
        }

        /// <summary>
        /// Sends a JSON-RPC success response.
        /// </summary>
        private static Task SendJsonRpcResultAsync(NetworkStream stream, string id, object result)
        {
            var payload = new JsonRpcResponse<object>
            {
                Id = id,
                Result = result
            };
            return SendAsync(stream, SerializeJson(payload));
        }

        /// <summary>
        /// Sends a JSON-RPC error response.
        /// </summary>
        private static Task SendJsonRpcErrorAsync(NetworkStream stream, string id, int code, string message, object data)
        {
            var payload = new JsonRpcErrorResponse
            {
                Id = id,
                Error = new JsonRpcErrorDetails
                {
                    Code = code,
                    Message = message,
                    Data = data
                }
            };
            return SendAsync(stream, SerializeJson(payload));
        }

        /// <summary>
        /// Finds the closest matches for a URL against the configured path structure.
        /// </summary>
        private static MatchSummary FindClosestMatches(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return MatchSummary.Empty;
            }

            if (IsFilePath(url))
            {
                var fileMatches = TryGetLastNodeMatch(url, out var fileMatch)
                    ? new[] { fileMatch }
                    : Array.Empty<PathPatternMatch>();
                var parentInfo = FindNearestParentMatches(Path.GetDirectoryName(url));
                return new MatchSummary(fileMatches, Array.Empty<PathPatternMatch>(), parentInfo.Matches, parentInfo.MatchedPath);
            }

            var folderMatches = TryGetLastNodeMatch(url, out var folderMatch)
                ? new[] { folderMatch }
                : Array.Empty<PathPatternMatch>();
            var parentFolderPath = GetParentPath(url);
            var parentFolderInfo = FindNearestParentMatches(parentFolderPath);
            return new MatchSummary(Array.Empty<PathPatternMatch>(), folderMatches, parentFolderInfo.Matches, parentFolderInfo.MatchedPath);
        }

        /// <summary>
        /// Finds immediate child path structures for the current selection.
        /// </summary>
        private static IReadOnlyList<PathPatternMatch> FindImmediateChildMatches(string url, MatchSummary summary)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Array.Empty<PathPatternMatch>();
            }

            IReadOnlyList<PathPatternMatch> baseNodes = null;
            if (summary?.FileMatches.Count > 0)
            {
                baseNodes = summary.ParentFolderMatches.Count > 0 ? summary.ParentFolderMatches : summary.FileMatches;
            }
            else if (summary?.FolderMatches.Count > 0)
            {
                baseNodes = summary.FolderMatches;
            }
            if (baseNodes == null || baseNodes.Count == 0)
            {
                return Array.Empty<PathPatternMatch>();
            }

            return FindImmediateChildMatches(baseNodes);
        }

        private static IReadOnlyList<PathPatternMatch> FindImmediateChildMatches(IReadOnlyList<PathPatternMatch> baseNodes)
        {
            if (baseNodes == null || baseNodes.Count == 0)
            {
                return Array.Empty<PathPatternMatch>();
            }

            var nodes = EnumerateMatchNodes().ToList();
            if (nodes.Count == 0)
            {
                return Array.Empty<PathPatternMatch>();
            }

            var childMatches = new List<PathPatternMatch>();

            foreach (var baseMatch in baseNodes)
            {
                if (string.IsNullOrWhiteSpace(baseMatch.MatchedValue))
                {
                    continue;
                }

                var baseIndex = nodes.FindIndex(node =>
                    string.Equals(node.Name, baseMatch.NodeName, StringComparison.Ordinal) &&
                    string.Equals(node.Pattern, baseMatch.Pattern, StringComparison.Ordinal));
                if (baseIndex < 0)
                {
                    continue;
                }

                var baseNode = nodes[baseIndex];
                foreach (var child in baseNode.Children ?? Array.Empty<IPathNode>())
                {
                    childMatches.Add(BuildPatternMatch(child));
                }
            }

            if (childMatches.Count == 0)
            {
                return Array.Empty<PathPatternMatch>();
            }

            var bestLength = childMatches.Max(item => item.MatchLength);
            if (bestLength == 0)
            {
                return childMatches.ToArray();
            }
            return childMatches.Where(item => item.MatchLength == bestLength).ToArray();
        }

        /// <summary>
        /// Enumerates configured path nodes excluding the root.
        /// </summary>
        private static IEnumerable<IPathNode> EnumerateMatchNodes()
        {
            if (_pathStructure?.Config?.Root == null)
            {
                yield break;
            }

            foreach (var node in EnumerateNodes(_pathStructure.Config.Root))
            {
                if (node == _pathStructure.Config.Root)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(node.Pattern))
                {
                    continue;
                }

                yield return node;
            }
        }

        /// <summary>
        /// Enumerates all nodes in the structure.
        /// </summary>
        private static IEnumerable<IPathNode> EnumerateNodes(IPathNode node)
        {
            if (node == null)
            {
                yield break;
            }

            yield return node;
            foreach (var child in node.Children ?? Array.Empty<IPathNode>())
            {
                foreach (var descendant in EnumerateNodes(child))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// Determines whether the URL points to a file based on extension and trailing separators.
        /// </summary>
        private static bool IsFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (path.EndsWith("\\", StringComparison.Ordinal) || path.EndsWith("/", StringComparison.Ordinal))
            {
                return false;
            }

            return Path.HasExtension(path);
        }

        /// <summary>
        /// Walks up the directory tree to find the nearest parent folder matches.
        /// </summary>
        private static ParentMatchInfo FindNearestParentMatches(string startPath)
        {
            var current = startPath;
            while (!string.IsNullOrWhiteSpace(current))
            {
                if (TryGetLastNodeMatch(current, out var match))
                {
                    return new ParentMatchInfo(current, new[] { match });
                }

                current = GetParentPath(current);
            }

            return ParentMatchInfo.Empty;
        }

        /// <summary>
        /// Attempts to resolve the deepest matching node for the provided path.
        /// </summary>
        private static bool TryGetLastNodeMatch(string path, out PathPatternMatch match)
        {
            match = default;
            if (_pathStructure == null || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var result = _pathStructure.ValidatePath(path);
            if (!result.IsValid || result.MatchTrail.Count == 0)
            {
                return false;
            }

            var lastMatch = result.MatchTrail[result.MatchTrail.Count - 1];
            match = BuildPatternMatch(lastMatch);
            return true;
        }

        private static bool TryGetValidationContext(
            string path,
            out PathPatternMatch match,
            out IReadOnlyDictionary<string, string> variables)
        {
            match = default;
            variables = null;
            if (_pathStructure == null || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var result = _pathStructure.ValidatePath(path);
            if (!result.IsValid || result.MatchTrail.Count == 0)
            {
                return false;
            }

            var lastMatch = result.MatchTrail[result.MatchTrail.Count - 1];
            match = BuildPatternMatch(lastMatch);
            variables = result.Variables;
            return true;
        }

        private static bool TryGetMatchTrail(
            string path,
            out IReadOnlyList<PathPatternMatch> matches,
            out IReadOnlyDictionary<string, string> variables)
        {
            matches = Array.Empty<PathPatternMatch>();
            variables = null;
            if (_pathStructure == null || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var result = _pathStructure.ValidatePath(path);
            if (!result.IsValid || result.MatchTrail.Count == 0)
            {
                return false;
            }

            variables = result.Variables;
            var normalizedLength = GetNormalizedPathLength(path);
            matches = result.MatchTrail
                .Select(BuildPatternMatch)
                .Where(match => IsFullPathMatch(match, normalizedLength))
                .OrderByDescending(match => match.MatchLength)
                .ThenByDescending(match => match.MatchedValue?.Length ?? 0)
                .ToArray();
            return matches.Count > 0;
        }

        private static int GetNormalizedPathLength(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return 0;
            }

            var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .TrimEnd(Path.DirectorySeparatorChar);
            return normalized.Length;
        }

        private static bool IsFullPathMatch(PathPatternMatch match, int normalizedLength)
        {
            if (normalizedLength == 0)
            {
                return false;
            }

            if (match.MatchLength == normalizedLength)
            {
                return true;
            }

            var matchedLength = match.MatchedValue?.Length ?? 0;
            return matchedLength == normalizedLength;
        }

        /// <summary>
        /// Returns the parent directory path.
        /// </summary>
        private static string GetParentPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return null;
            }

            return Path.GetDirectoryName(trimmed);
        }

#if DEBUG
        /// <summary>
        /// Logs matching details for debugging.
        /// </summary>
        private static void LogMatches(string url, MatchSummary summary)
        {
            if (summary == null || summary.IsEmpty)
            {
                Console.WriteLine($"[DEBUG] No PathStructure matches for '{url}'.");
                return;
            }

            if (summary.FileMatches.Count > 0)
            {
                Console.WriteLine($"[DEBUG] File matches for '{url}': {FormatMatches(summary.FileMatches)}");
            }

            if (summary.FolderMatches.Count > 0)
            {
                Console.WriteLine($"[DEBUG] Folder matches for '{url}': {FormatMatches(summary.FolderMatches)}");
            }

            if (summary.ParentFolderMatches.Count > 0)
            {
                Console.WriteLine($"[DEBUG] Parent folder matches for '{summary.ParentFolderPath}': {FormatMatches(summary.ParentFolderMatches)}");
            }
        }

        /// <summary>
        /// Formats match results for logging.
        /// </summary>
        private static string FormatMatches(IEnumerable<PathPatternMatch> matches)
        {
            return string.Join(", ", matches.Select(match => $"{match.NodeName}: {match.Pattern} (match: {match.MatchedValue})"));
        }
#endif

        /// <summary>
        /// Builds a match result with node metadata.
        /// </summary>
        private static PathPatternMatch BuildPatternMatch(IPathNode node, Match match)
        {
            var metadata = node as PathNode;
            return new PathPatternMatch(
                node?.Name,
                node?.Pattern,
                match?.Value,
                match?.Value?.Length ?? 0,
                metadata?.FlavorTextTemplate,
                metadata?.BackgroundColor,
                metadata?.ForegroundColor,
                metadata?.Icon,
                metadata?.IsRequired ?? false);
        }

        private static PathPatternMatch BuildPatternMatch(IPathNode node)
        {
            var metadata = node as PathNode;
            return new PathPatternMatch(
                node?.Name,
                node?.Pattern,
                null,
                0,
                metadata?.FlavorTextTemplate,
                metadata?.BackgroundColor,
                metadata?.ForegroundColor,
                metadata?.Icon,
                metadata?.IsRequired ?? false);
        }

        private static PathPatternMatch BuildPatternMatch(IPathMatchNode matchNode)
        {
            if (matchNode == null)
            {
                return BuildPatternMatch((IPathNode)null);
            }

            var metadata = matchNode.Node as PathNode;
            var matchedValue = matchNode.MatchedValue;
            return new PathPatternMatch(
                matchNode.Node?.Name,
                matchNode.Node?.Pattern,
                matchedValue,
                matchedValue?.Length ?? 0,
                metadata?.FlavorTextTemplate,
                metadata?.BackgroundColor,
                metadata?.ForegroundColor,
                metadata?.Icon,
                metadata?.IsRequired ?? false);
        }

        /// <summary>
        /// Represents the match summary for a selection.
        /// </summary>
        private sealed class MatchSummary
        {
            public static readonly MatchSummary Empty = new MatchSummary(Array.Empty<PathPatternMatch>(), Array.Empty<PathPatternMatch>(), Array.Empty<PathPatternMatch>(), null);

            public MatchSummary(
                IReadOnlyList<PathPatternMatch> fileMatches,
                IReadOnlyList<PathPatternMatch> folderMatches,
                IReadOnlyList<PathPatternMatch> parentFolderMatches,
                string parentFolderPath)
            {
                FileMatches = fileMatches ?? Array.Empty<PathPatternMatch>();
                FolderMatches = folderMatches ?? Array.Empty<PathPatternMatch>();
                ParentFolderMatches = parentFolderMatches ?? Array.Empty<PathPatternMatch>();
                ParentFolderPath = parentFolderPath;
            }

            public IReadOnlyList<PathPatternMatch> FileMatches { get; }
            public IReadOnlyList<PathPatternMatch> FolderMatches { get; }
            public IReadOnlyList<PathPatternMatch> ParentFolderMatches { get; }
            public string ParentFolderPath { get; }
            public bool IsEmpty => FileMatches.Count == 0 && FolderMatches.Count == 0 && ParentFolderMatches.Count == 0;
        }

        /// <summary>
        /// Represents nearest parent folder match results.
        /// </summary>
        private readonly struct ParentMatchInfo
        {
            public static readonly ParentMatchInfo Empty = new ParentMatchInfo(null, Array.Empty<PathPatternMatch>());

            public ParentMatchInfo(string matchedPath, IReadOnlyList<PathPatternMatch> matches)
            {
                MatchedPath = matchedPath;
                Matches = matches ?? Array.Empty<PathPatternMatch>();
            }

            public string MatchedPath { get; }
            public IReadOnlyList<PathPatternMatch> Matches { get; }
        }

        /// <summary>
        /// Represents a single path pattern match with metadata.
        /// </summary>
        private readonly struct PathPatternMatch
        {
            public PathPatternMatch(
                string nodeName,
                string pattern,
                string matchedValue,
                int matchLength,
                string flavorTextTemplate,
                string backgroundColor,
                string foregroundColor,
                string icon,
                bool isRequired)
            {
                NodeName = nodeName;
                Pattern = pattern;
                MatchedValue = matchedValue;
                MatchLength = matchLength;
                FlavorTextTemplate = flavorTextTemplate;
                BackgroundColor = backgroundColor;
                ForegroundColor = foregroundColor;
                Icon = icon;
                IsRequired = isRequired;
            }

            public string NodeName { get; }
            public string Pattern { get; }
            public string MatchedValue { get; }
            public int MatchLength { get; }
            public string FlavorTextTemplate { get; }
            public string BackgroundColor { get; }
            public string ForegroundColor { get; }
            public string Icon { get; }
            public bool IsRequired { get; }
        }

        /// <summary>
        /// JSON-RPC request payload.
        /// </summary>
        private sealed class JsonRpcRequest
        {
            public string Jsonrpc { get; set; }
            public string Id { get; set; }
            public string Method { get; set; }
            public JsonElement Params { get; set; }
        }
    }
}
