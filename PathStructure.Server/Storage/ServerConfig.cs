using PathStructure;
using System;
using System.Collections.Generic;

namespace PathStructureServer.Storage
{
    public sealed class ServerConfig
    {
        public PathStructureManagementConfig Management { get; set; } = new PathStructureManagementConfig();
        public IList<PathStructureModel> Models { get; set; } = new List<PathStructureModel>();
        public IList<ClientRecord> Clients { get; set; } = new List<ClientRecord>();
        public string ActiveModelId { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public sealed class ClientRecord
    {
        public string ClientId { get; set; }
        public string DeviceName { get; set; }
        public string UserName { get; set; }
        public string Principal { get; set; }
        public string Provider { get; set; }
        public bool Authorized { get; set; }
        public string InstallProfileId { get; set; }
        public DateTimeOffset? RegisteredAt { get; set; }
        public DateTimeOffset? LastSeen { get; set; }
        public UsageReportRecord LastUsageReport { get; set; }
    }

    public sealed class UsageReportRecord
    {
        public string ClientId { get; set; }
        public string Path { get; set; }
        public string SelectionKind { get; set; }
        public int? DurationSeconds { get; set; }
        public string StartedAt { get; set; }
        public string EndedAt { get; set; }
        public DateTimeOffset? ReportedAt { get; set; }
    }
}
