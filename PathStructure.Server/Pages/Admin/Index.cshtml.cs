using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PathStructure.Server.Storage;

namespace PathStructure.Server.Pages.Admin
{
    public sealed class IndexModel : PageModel
    {
        private readonly ServerConfigStore _store;

        public IndexModel(ServerConfigStore store)
        {
            _store = store;
        }

        public IReadOnlyList<ClientRecord> Clients { get; private set; } = new List<ClientRecord>();
        public PathStructureManagementConfig Management { get; private set; }
        public string ActiveModelId { get; private set; }

        public void OnGet()
        {
            var config = _store.GetConfig();
            Clients = new List<ClientRecord>(config.Clients ?? new List<ClientRecord>());
            Management = config.Management;
            ActiveModelId = config.ActiveModelId;
        }
    }
}
