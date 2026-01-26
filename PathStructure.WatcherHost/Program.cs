using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PathStructure;
using PathStructure.Abstracts;

namespace PathStructure.WatcherHost
{
    internal class Program
    {
        private const int DefaultPort = 49321;
        private static readonly ConcurrentDictionary<TcpClient, NetworkStream> Clients = new ConcurrentDictionary<TcpClient, NetworkStream>();
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static ExplorerWatcher _watcher;
        private static TcpListener _listener;
        private static PathStructure _pathStructure;

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
            _pathStructure = new PathStructure(config);

            _watcher = new ExplorerWatcher(_pathStructure, new ExplorerWatcherOptions
            {
                PollRateMs = 500
            });

            _watcher.ExplorerWatcherFound += OnExplorerFound;
            _watcher.ExplorerWatcherError += (sender, exception) =>
            {
                BroadcastEvent("error", "Explorer watcher error.", exception.Message);
            };
            _watcher.ExplorerWatcherAborted += (sender, exceptionArgs) =>
            {
                BroadcastEvent("aborted", "Explorer watcher aborted.", exceptionArgs?.ToString());
            };

            _watcher.StartWatcher();
        }

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

                await SendAsync(stream, BuildEventPayload("status", "Client connected.", "connected")).ConfigureAwait(false);
                _ = Task.Run(() => MonitorClientAsync(client, stream, token));
            }
        }

        private static async Task MonitorClientAsync(TcpClient client, NetworkStream stream, CancellationToken token)
        {
            var buffer = new byte[1];
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!client.Connected)
                    {
                        break;
                    }

                    if (stream.DataAvailable)
                    {
                        await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await Task.Delay(500, token).ConfigureAwait(false);
                    }
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

        private static void OnExplorerFound(string url)
        {
            var matchSummary = FindClosestMatches(url);
#if DEBUG
            LogMatches(url, matchSummary);
#endif
            BroadcastEvent("pathChanged", "Explorer path changed.", url);
        }

        private static void BroadcastEvent(string type, string message, string path)
        {
            var payload = BuildEventPayload(type, message, path);
            foreach (var client in Clients)
            {
                _ = SendAsync(client.Value, payload);
            }
        }

        private static string BuildEventPayload(string type, string message, string path)
        {
            var escapedMessage = EscapeJson(message ?? string.Empty);
            var escapedPath = EscapeJson(path ?? string.Empty);
            var timestamp = DateTimeOffset.Now.ToString("o");
            return $"{{\"type\":\"{type}\",\"message\":\"{escapedMessage}\",\"path\":\"{escapedPath}\",\"timestamp\":\"{timestamp}\"}}\n";
        }

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

        private static string EscapeJson(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

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

        private static bool IsFileRegex(IPathNode node)
        {
            if (node == null)
            {
                return false;
            }

            var pattern = node.Pattern ?? string.Empty;
            return pattern.Contains(@"\.") || Regex.IsMatch(pattern, @"\\\.[^\\]*\$?");
        }

        private static bool IsFolderRegex(IPathNode node)
        {
            return !IsFileRegex(node);
        }

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

                matches.Add(new PathPatternMatch(node.Pattern, match.Value, match.Value.Length));
            }

            if (matches.Count == 0)
            {
                return Array.Empty<PathPatternMatch>();
            }

            var bestLength = matches.Max(item => item.MatchLength);
            return matches.Where(item => item.MatchLength == bestLength).ToArray();
        }

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

        private static string FormatMatches(IEnumerable<PathPatternMatch> matches)
        {
            return string.Join(", ", matches.Select(match => $"{match.Pattern} (match: {match.MatchedValue})"));
        }
#endif

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

        private readonly struct PathPatternMatch
        {
            public PathPatternMatch(string pattern, string matchedValue, int matchLength)
            {
                Pattern = pattern;
                MatchedValue = matchedValue;
                MatchLength = matchLength;
            }

            public string Pattern { get; }
            public string MatchedValue { get; }
            public int MatchLength { get; }
        }
    }
}
