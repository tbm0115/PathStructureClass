using System;
using Microsoft.AspNetCore.Mvc;
using PathStructure.Server.Storage;

namespace PathStructure.Server.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public sealed class ClientsController : ControllerBase
    {
        private readonly ServerConfigStore _store;

        public ClientsController(ServerConfigStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult ListClients()
        {
            var config = _store.GetConfig();
            return Ok(new { clients = config.Clients });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] ClientRegistrationRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body required." });
            }

            var config = _store.GetConfig();
            var clientId = _store.NormalizeString(request.ClientId) ?? Guid.NewGuid().ToString("N");
            var client = _store.GetOrCreateClient(config, clientId);
            client.DeviceName = _store.NormalizeString(request.DeviceName);
            client.UserName = _store.NormalizeString(request.UserName);
            client.Principal = _store.NormalizeString(request.Principal);
            client.Provider = _store.NormalizeString(request.Provider);
            client.LastSeen = _store.Now();

            var authorized = _store.IsAuthorized(config.Management?.Authorization, clientId, client.Principal, client.Provider);
            client.Authorized = authorized;
            client.InstallProfileId = _store.ResolveProfileId(request.InstallProfileId, config.Management?.Installation);

            _store.SaveConfig(config);

            return Ok(new
            {
                message = authorized ? "Client registered." : "Client registration pending authorization.",
                clientId,
                authorized,
                authorizationMode = config.Management?.Authorization?.Mode ?? "none",
                installProfileId = client.InstallProfileId,
                usageReportingRequired = config.Management?.UsageReporting?.Required ?? false,
                minimumUsageIntervalSeconds = config.Management?.UsageReporting?.MinimumReportIntervalSeconds ?? 0
            });
        }

        [HttpPost("{clientId}/usage")]
        public IActionResult ReportUsage(string clientId, [FromBody] UsageReportRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body required." });
            }

            var config = _store.GetConfig();
            var client = _store.GetOrCreateClient(config, clientId);
            client.LastSeen = _store.Now();

            client.LastUsageReport = new UsageReportRecord
            {
                ClientId = clientId,
                Path = _store.NormalizeString(request.Path),
                SelectionKind = _store.NormalizeString(request.SelectionKind),
                DurationSeconds = request.DurationSeconds,
                StartedAt = _store.NormalizeString(request.StartedAt),
                EndedAt = _store.NormalizeString(request.EndedAt),
                ReportedAt = _store.Now()
            };

            _store.SaveConfig(config);

            return Ok(new { message = "Usage report received.", clientId, reportedAt = client.LastUsageReport.ReportedAt?.ToString("o") });
        }
    }

    public sealed class ClientRegistrationRequest
    {
        public string ClientId { get; set; }
        public string DeviceName { get; set; }
        public string UserName { get; set; }
        public string Principal { get; set; }
        public string Provider { get; set; }
        public string InstallProfileId { get; set; }
    }

    public sealed class UsageReportRequest
    {
        public string Path { get; set; }
        public string SelectionKind { get; set; }
        public int? DurationSeconds { get; set; }
        public string StartedAt { get; set; }
        public string EndedAt { get; set; }
    }
}
