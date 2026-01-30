using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PathStructure.Server.Storage;

namespace PathStructure.Server.Controllers
{
    [ApiController]
    [Route("api/models")]
    public sealed class ModelsController : ControllerBase
    {
        private readonly ServerConfigStore _store;

        public ModelsController(ServerConfigStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult ListModels()
        {
            var config = _store.GetConfig();
            return Ok(new { models = config.Models });
        }

        [HttpPost]
        public IActionResult Upsert([FromBody] PathModelUpsertRequest request)
        {
            if (request?.Model == null)
            {
                return BadRequest(new { message = "Model payload required." });
            }

            var config = _store.GetConfig();
            var model = request.Model;
            model.Id = _store.NormalizeString(model.Id) ?? Guid.NewGuid().ToString("N");
            model.Name = _store.NormalizeString(model.Name);
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return BadRequest(new { message = "Model name is required." });
            }

            model.UpdatedAt = _store.Now();
            model.Imports = model.Imports ?? new List<PathStructureImport>();
            model.Paths = model.Paths ?? new List<PathStructurePath>();
            model.Plugins = model.Plugins ?? new List<PathStructurePlugin>();

            var existing = config.Models.FirstOrDefault(entry =>
                string.Equals(entry.Id, model.Id, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                config.Models.Add(model);
            }
            else
            {
                existing.Name = model.Name;
                existing.Description = model.Description;
                existing.Version = model.Version;
                existing.UpdatedAt = model.UpdatedAt;
                existing.Imports = model.Imports;
                existing.Paths = model.Paths;
                existing.Plugins = model.Plugins;
            }

            _store.SaveConfig(config);
            return Ok(new { message = "Path model saved.", model });
        }

        [HttpPost("{modelId}/apply")]
        public IActionResult Apply(string modelId)
        {
            var config = _store.GetConfig();
            var model = config.Models.FirstOrDefault(entry =>
                string.Equals(entry.Id, modelId, StringComparison.OrdinalIgnoreCase));
            if (model == null)
            {
                return NotFound(new { message = "Model not found." });
            }

            config.ActiveModelId = model.Id;
            _store.SaveConfig(config);
            return Ok(new { message = "Model applied.", modelId = model.Id, modelName = model.Name });
        }
    }

    public sealed class PathModelUpsertRequest
    {
        public PathStructureModel Model { get; set; }
    }
}
