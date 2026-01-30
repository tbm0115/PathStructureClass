using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PathStructureServer.Storage;

namespace PathStructureServer.Controllers
{
    [ApiController]
    [Route("api/standard-path-structures")]
    public sealed class StandardPathStructuresController : ControllerBase
    {
        private readonly ServerConfigStore _configStore;
        private readonly StandardPathStructureStore _standardStore;

        public StandardPathStructuresController(ServerConfigStore configStore, StandardPathStructureStore standardStore)
        {
            _configStore = configStore;
            _standardStore = standardStore;
        }

        [HttpGet]
        public IActionResult ListVersions()
        {
            var config = _configStore.GetConfig();
            return Ok(new
            {
                versions = config.StandardPathStructures,
                releasedVersionId = config.ReleasedStandardPathStructureId,
                releasedAt = config.ReleasedStandardPathStructureAt
            });
        }

        [HttpGet("released")]
        public IActionResult GetReleased()
        {
            var config = _configStore.GetConfig();
            if (string.IsNullOrWhiteSpace(config.ReleasedStandardPathStructureId))
            {
                return NotFound(new { message = "No released path structure has been published yet." });
            }

            var version = config.StandardPathStructures.FirstOrDefault(entry =>
                string.Equals(entry.Id, config.ReleasedStandardPathStructureId, System.StringComparison.OrdinalIgnoreCase));
            if (version == null)
            {
                return NotFound(new { message = "Released version metadata could not be found." });
            }

            var structure = _standardStore.LoadConfig(version);
            if (structure == null)
            {
                return NotFound(new { message = "Released configuration file could not be loaded." });
            }

            return Ok(new
            {
                version,
                releasedAt = config.ReleasedStandardPathStructureAt,
                structure
            });
        }
    }
}
