using PathStructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PathStructureServer.Storage
{
    public sealed class ServerConfigStore
    {
        private const string DefaultConfigFileName = "pathstructure-enterprise.json";
        private readonly object _syncRoot = new object();
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
        private readonly TimeProvider _timeProvider;
        private string _configPath;
        private ServerConfig _cachedConfig;

        public ServerConfigStore(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        public DateTimeOffset Now() => _timeProvider.GetLocalNow();

        public ServerConfig GetConfig()
        {
            lock (_syncRoot)
            {
                if (_cachedConfig != null)
                {
                    return _cachedConfig;
                }

                var path = EnsureConfigPath();
                if (!File.Exists(path))
                {
                    _cachedConfig = BuildDefaultConfig();
                    SaveConfig(_cachedConfig);
                    return _cachedConfig;
                }

                var json = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<ServerConfig>(json, _serializerOptions) ?? BuildDefaultConfig();
                config.Management = NormalizeManagement(config.Management);
                config.Models = config.Models ?? new List<PathStructureModel>();
                config.Clients = config.Clients ?? new List<ClientRecord>();
                _cachedConfig = config;
                return _cachedConfig;
            }
        }

        public void SaveConfig(ServerConfig config)
        {
            if (config == null)
            {
                return;
            }

            lock (_syncRoot)
            {
                config.Management = NormalizeManagement(config.Management);
                config.Models = config.Models ?? new List<PathStructureModel>();
                config.Clients = config.Clients ?? new List<ClientRecord>();
                config.UpdatedAt = Now();
                var path = EnsureConfigPath();
                var json = JsonSerializer.Serialize(config, _serializerOptions);
                File.WriteAllText(path, json);
                _cachedConfig = config;
            }
        }

        public PathStructureManagementConfig NormalizeManagement(PathStructureManagementConfig config)
        {
            var normalized = config ?? new PathStructureManagementConfig();
            normalized.Authorization = normalized.Authorization ?? new PathStructureAuthorizationConfig();
            normalized.Authorization.AllowedClientIds = normalized.Authorization.AllowedClientIds ?? new List<string>();
            normalized.Authorization.AllowedPrincipals = normalized.Authorization.AllowedPrincipals ?? new List<string>();
            normalized.Authorization.Entra = normalized.Authorization.Entra ?? new PathStructureEntraAuthorizationSettings();
            normalized.Authorization.Ldap = normalized.Authorization.Ldap ?? new PathStructureLdapAuthorizationSettings();
            normalized.Authorization.Entra.AllowedGroups = normalized.Authorization.Entra.AllowedGroups ?? new List<string>();
            normalized.Authorization.Ldap.AllowedGroups = normalized.Authorization.Ldap.AllowedGroups ?? new List<string>();
            normalized.UsageReporting = normalized.UsageReporting ?? new PathStructureUsageReportingConfig();
            normalized.Installation = normalized.Installation ?? new PathStructureInstallationConfig();
            normalized.Installation.Profiles = normalized.Installation.Profiles ?? new List<PathStructureInstallationProfile>();
            return normalized;
        }

        public string NormalizeString(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public ClientRecord GetOrCreateClient(ServerConfig config, string clientId)
        {
            if (config == null)
            {
                return null;
            }

            config.Clients = config.Clients ?? new List<ClientRecord>();
            var existing = config.Clients.FirstOrDefault(client =>
                string.Equals(client.ClientId, clientId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return existing;
            }

            var created = new ClientRecord
            {
                ClientId = clientId,
                RegisteredAt = Now()
            };
            config.Clients.Add(created);
            return created;
        }

        public bool IsAuthorized(PathStructureAuthorizationConfig config, string clientId, string principal, string provider)
        {
            var mode = NormalizeString(config?.Mode);
            if (string.IsNullOrWhiteSpace(mode) || string.Equals(mode, "none", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(provider) &&
                !string.Equals(mode, provider, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (config?.AllowedClientIds != null && config.AllowedClientIds.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(clientId) ||
                    !config.AllowedClientIds.Any(id => string.Equals(id, clientId, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }

            if (config?.AllowedPrincipals != null && config.AllowedPrincipals.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(principal) ||
                    !config.AllowedPrincipals.Any(id => string.Equals(id, principal, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }

            return true;
        }

        public string ResolveProfileId(string requestedProfileId, PathStructureInstallationConfig config)
        {
            var normalized = NormalizeString(requestedProfileId);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                return normalized;
            }

            return NormalizeString(config?.DefaultProfileId);
        }

        private string EnsureConfigPath()
        {
            if (!string.IsNullOrWhiteSpace(_configPath))
            {
                return _configPath;
            }

            var baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PathStructure");
            Directory.CreateDirectory(baseDirectory);
            _configPath = Path.Combine(baseDirectory, DefaultConfigFileName);
            return _configPath;
        }

        private ServerConfig BuildDefaultConfig()
        {
            var defaultProfile = new PathStructureInstallationProfile
            {
                Id = "default",
                Name = "Default",
                Description = "Default managed installation profile."
            };

            return new ServerConfig
            {
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
                Models = new List<PathStructureModel>(),
                Clients = new List<ClientRecord>(),
                ActiveModelId = null,
                UpdatedAt = Now()
            };
        }
    }
}
