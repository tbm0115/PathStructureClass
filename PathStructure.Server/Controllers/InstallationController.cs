using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PathStructure.Server.Storage;

namespace PathStructure.Server.Controllers
{
    [ApiController]
    [Route("api/installation")]
    public sealed class InstallationController : ControllerBase
    {
        private readonly ServerConfigStore _store;

        public InstallationController(ServerConfigStore store)
        {
            _store = store;
        }

        [HttpGet("profiles")]
        public IActionResult ListProfiles()
        {
            var config = _store.GetConfig();
            return Ok(new
            {
                profiles = config.Management?.Installation?.Profiles ?? new List<PathStructureInstallationProfile>(),
                defaultProfileId = config.Management?.Installation?.DefaultProfileId
            });
        }
    }
}
