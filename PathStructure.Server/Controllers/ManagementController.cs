using Microsoft.AspNetCore.Mvc;
using PathStructure.Server.Storage;

namespace PathStructure.Server.Controllers
{
    [ApiController]
    [Route("api/management")]
    public sealed class ManagementController : ControllerBase
    {
        private readonly ServerConfigStore _store;

        public ManagementController(ServerConfigStore store)
        {
            _store = store;
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            var config = _store.GetConfig();
            return Ok(new
            {
                management = config.Management,
                activeModelId = config.ActiveModelId
            });
        }

        [HttpPut("config")]
        public IActionResult UpdateConfig([FromBody] ManagementConfigUpdate request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body required." });
            }

            var config = _store.GetConfig();
            config.Management = _store.NormalizeManagement(request.Management ?? config.Management);
            _store.SaveConfig(config);
            return Ok(new { message = "Management configuration updated.", management = config.Management });
        }
    }

    public sealed class ManagementConfigUpdate
    {
        public PathStructureManagementConfig Management { get; set; }
    }
}
