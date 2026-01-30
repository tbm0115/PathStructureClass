using System;
using System.IO;
using System.Text.Json;
using PathStructure;

namespace PathStructureServer.Storage
{
    public sealed class StandardPathStructureStore
    {
        private const string StandardFolderName = "StandardPathStructures";
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
        private readonly TimeProvider _timeProvider;

        public StandardPathStructureStore(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        public DateTimeOffset Now() => _timeProvider.GetLocalNow();

        public string EnsureStorageDirectory()
        {
            var baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PathStructure");
            var standardDirectory = Path.Combine(baseDirectory, StandardFolderName);
            Directory.CreateDirectory(standardDirectory);
            return standardDirectory;
        }

        public string GetVersionPath(StandardPathStructureVersion version)
        {
            if (version == null)
            {
                return null;
            }

            var fileName = string.IsNullOrWhiteSpace(version.FileName)
                ? $"{version.Id}.json"
                : version.FileName;
            var directory = EnsureStorageDirectory();
            return Path.Combine(directory, fileName);
        }

        public string ReadRawJson(StandardPathStructureVersion version)
        {
            var path = GetVersionPath(version);
            return path != null && File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public PathStructureConfig LoadConfig(StandardPathStructureVersion version)
        {
            var json = ReadRawJson(version);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<PathStructureConfig>(json, _serializerOptions);
        }

        public void SaveConfig(StandardPathStructureVersion version, PathStructureConfig config, string rawJson = null)
        {
            if (version == null || config == null)
            {
                return;
            }

            var path = GetVersionPath(version);
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(version.FileName))
            {
                version.FileName = Path.GetFileName(path);
            }

            var json = string.IsNullOrWhiteSpace(rawJson)
                ? JsonSerializer.Serialize(config, _serializerOptions)
                : rawJson;
            File.WriteAllText(path, json);
        }

        public bool TryParseConfig(string json, out PathStructureConfig config, out string error)
        {
            config = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Configuration JSON is required.";
                return false;
            }

            try
            {
                config = JsonSerializer.Deserialize<PathStructureConfig>(json, _serializerOptions);
                if (config == null)
                {
                    error = "Unable to deserialize the configuration payload.";
                    return false;
                }

                config.Imports = config.Imports ?? new System.Collections.Generic.List<PathStructureImport>();
                config.Paths = config.Paths ?? new System.Collections.Generic.List<PathStructurePath>();
                config.Plugins = config.Plugins ?? new System.Collections.Generic.List<PathStructurePlugin>();
                config.Models = config.Models ?? new System.Collections.Generic.List<PathStructureModel>();
                config.Management = config.Management ?? new PathStructureManagementConfig();
                return true;
            }
            catch (JsonException ex)
            {
                error = $"Invalid JSON: {ex.Message}";
                return false;
            }
        }
    }
}
