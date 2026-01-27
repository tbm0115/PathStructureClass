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

namespace PathStructure.WatcherHost
{
    /// <summary>
    /// Hosts the Explorer watcher and streams JSON-RPC notifications/responses to clients.
    /// </summary>
    internal class Program
    {
        private const int DefaultPort = 49321;
        private static readonly ConcurrentDictionary<TcpClient, NetworkStream> Clients = new ConcurrentDictionary<TcpClient, NetworkStream>();
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static ExplorerWatcher _watcher;
        private static TcpListener _listener;
        private static PathStructure _pathStructure;
        private static PathStructureConfig _pathConfig;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
            }
            else
            {
                var rootNode = new PathNode("Root", @"^.*$");
                config = new PathStructureConfig(rootNode);
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
                BroadcastNotification(new
                {
                    jsonrpc = "2.0",
                    method = "watcherError",
                    @params = new
                    {
                        message = "Explorer watcher error.",
                        error = exception?.Message,
                        timestamp = DateTimeOffset.Now.ToString("o")
                    }
                });
            };
            _watcher.ExplorerWatcherAborted += (sender, exceptionArgs) =>
            {
                BroadcastNotification(new
                {
                    jsonrpc = "2.0",
                    method = "watcherAborted",
                    @params = new
                    {
                        message = "Explorer watcher aborted.",
                        error = exceptionArgs?.ToString(),
                        timestamp = DateTimeOffset.Now.ToString("o")
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

                var statusPayload = new
                {
                    jsonrpc = "2.0",
                    method = "status",
                    @params = new
                    {
                        message = "Client connected.",
                        state = "connected",
                        timestamp = DateTimeOffset.Now.ToString("o")
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

                    await HandleClientCommandAsync(line, stream).ConfigureAwait(false);
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
            var matchSummary = FindClosestMatches(url);
#if DEBUG
            LogMatches(url, matchSummary);
#endif
            BroadcastNotification(new
            {
                jsonrpc = "2.0",
                method = "pathChanged",
                @params = new
                {
                    message = "Explorer path changed.",
                    path = url,
                    timestamp = DateTimeOffset.Now.ToString("o")
                }
            });
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
        private static async Task HandleClientCommandAsync(string payload, NetworkStream stream)
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

            if (_pathConfig.Paths.Any(path => string.Equals(path.Regex, regex, StringComparison.OrdinalIgnoreCase)))
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
                Icon = GetOptionalString(request.Params, "icon")
            };

            _pathConfig.Paths.Add(newPath);
            if (_pathConfig.Root is PathNode rootNode)
            {
                rootNode.Children.Add(BuildPathNode(newPath));
            }

            await SendJsonRpcResultAsync(stream, request.Id, new
            {
                message = "Path regex added.",
                path = regex
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
                path.Icon);
        }

        /// <summary>
        /// Sends a JSON-RPC success response.
        /// </summary>
        private static Task SendJsonRpcResultAsync(NetworkStream stream, string id, object result)
        {
            var payload = new
            {
                jsonrpc = "2.0",
                id,
                result
            };
            return SendAsync(stream, SerializeJson(payload));
        }

        /// <summary>
        /// Sends a JSON-RPC error response.
        /// </summary>
        private static Task SendJsonRpcErrorAsync(NetworkStream stream, string id, int code, string message, object data)
        {
            var payload = new
            {
                jsonrpc = "2.0",
                id,
                error = new
                {
                    code,
                    message,
                    data
                }
            };
            return SendAsync(stream, SerializeJson(payload));
        }

        /// <summary>
        /// Finds the closest matches for a URL against the configured path structure.
        /// </summary>
        private static MatchSummary FindClosestMatches(string url)
        {
            var nodes = EnumerateMatchNodes().ToList();
            if (nodes.Count == 0 || string.IsNullOrWhiteSpace(url))
            {
                return MatchSummary.Empty;
            }

            if (IsFilePath(url))
            {
                var fileMatches = FindBestMatches(nodes, url, IsFileRegex);
                var parentInfo = FindNearestParentMatches(nodes, Path.GetDirectoryName(url), IsFolderRegex);
                return new MatchSummary(fileMatches, Array.Empty<PathPatternMatch>(), parentInfo.Matches, parentInfo.MatchedPath);
            }

            var folderMatches = FindBestMatches(nodes, url, IsFolderRegex);
            var parentFolderPath = GetParentPath(url);
            var parentFolderInfo = FindNearestParentMatches(nodes, parentFolderPath, IsFolderRegex);
            return new MatchSummary(Array.Empty<PathPatternMatch>(), folderMatches, parentFolderInfo.Matches, parentFolderInfo.MatchedPath);
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
        /// Determines whether a configured regex represents a file entry.
        /// </summary>
        private static bool IsFileRegex(IPathNode node)
        {
            if (node == null)
            {
                return false;
            }

            var pattern = node.Pattern ?? string.Empty;
            var segments = pattern.Split(new[] { @"\\" }, StringSplitOptions.None);
            var lastSegment = segments.Length > 0 ? segments[segments.Length - 1] : pattern;
            if (!pattern.EndsWith("$", StringComparison.Ordinal))
            {
                return false;
            }

            return Regex.IsMatch(lastSegment, @"\\\.[^\\]+\\?\$?$") || Regex.IsMatch(lastSegment, @"\.[^\\]+\\?\$?$");
        }

        /// <summary>
        /// Determines whether a configured regex represents a folder entry.
        /// </summary>
        private static bool IsFolderRegex(IPathNode node)
        {
            return !IsFileRegex(node);
        }

        /// <summary>
        /// Finds the best matching regexes for a given path.
        /// </summary>
        private static IReadOnlyList<PathPatternMatch> FindBestMatches(
            IEnumerable<IPathNode> nodes,
            string path,
            Func<IPathNode, bool> filter)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Array.Empty<PathPatternMatch>();
            }

            var options = _pathStructure?.Config?.RegexOptions ?? RegexOptions.None;
            var matches = new List<PathPatternMatch>();

            foreach (var node in nodes)
            {
                if (!filter(node))
                {
                    continue;
                }

                var regex = node.GetRegex(options);
                var match = regex.Match(path);
                if (!match.Success)
                {
                    continue;
                }

                matches.Add(BuildPatternMatch(node, match));
            }

            if (matches.Count == 0)
            {
                return Array.Empty<PathPatternMatch>();
            }

            var bestLength = matches.Max(item => item.MatchLength);
            return matches.Where(item => item.MatchLength == bestLength).ToArray();
        }

        /// <summary>
        /// Walks up the directory tree to find the nearest parent folder matches.
        /// </summary>
        private static ParentMatchInfo FindNearestParentMatches(
            IEnumerable<IPathNode> nodes,
            string startPath,
            Func<IPathNode, bool> filter)
        {
            var current = startPath;
            while (!string.IsNullOrWhiteSpace(current))
            {
                var matches = FindBestMatches(nodes, current, filter);
                if (matches.Count > 0)
                {
                    return new ParentMatchInfo(current, matches);
                }

                current = GetParentPath(current);
            }

            return ParentMatchInfo.Empty;
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
                metadata?.Icon);
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
                string icon)
            {
                NodeName = nodeName;
                Pattern = pattern;
                MatchedValue = matchedValue;
                MatchLength = matchLength;
                FlavorTextTemplate = flavorTextTemplate;
                BackgroundColor = backgroundColor;
                ForegroundColor = foregroundColor;
                Icon = icon;
            }

            public string NodeName { get; }
            public string Pattern { get; }
            public string MatchedValue { get; }
            public int MatchLength { get; }
            public string FlavorTextTemplate { get; }
            public string BackgroundColor { get; }
            public string ForegroundColor { get; }
            public string Icon { get; }
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
