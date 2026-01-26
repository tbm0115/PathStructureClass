using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PathStructure;

namespace PathStructure.WatcherHost
{
    internal class Program
    {
        private const int DefaultPort = 49321;
        private static readonly ConcurrentDictionary<TcpClient, NetworkStream> Clients = new ConcurrentDictionary<TcpClient, NetworkStream>();
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static ExplorerWatcher _watcher;
        private static TcpListener _listener;

        [STAThread]
        private static void Main(string[] args)
        {
            var port = DefaultPort;
            if (args.Length > 0 && int.TryParse(args[0], out var parsedPort))
            {
                port = parsedPort;
            }

            Console.WriteLine($"Starting PathStructure.WatcherHost on port {port}.");

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Stop();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Stop();

            StartWatcher();
            StartServerAsync(port, _cts.Token).GetAwaiter().GetResult();
        }

        private static void StartWatcher()
        {
            var rootNode = new PathNode("Root", @"^.*$");
            var config = new PathStructureConfig(rootNode);
            var pathStructure = new PathStructure(config);

            _watcher = new ExplorerWatcher(pathStructure, new ExplorerWatcherOptions
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
                BroadcastEvent("aborted", "Explorer watcher aborted.", exceptionArgs.ExceptionObject?.ToString());
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
    }
}
