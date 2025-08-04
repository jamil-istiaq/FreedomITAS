using FreedomITAS.API_Serv;
using FreedomITAS.Models;

namespace FreedomITAS.Services
{
    public class ClientDeleteService
    {
        private readonly HuduService _huduService;
        private readonly HaloPSAService _haloPSAService;

        public ClientDeleteService(HuduService huduService, HaloPSAService haloPSAService)
        {
            _huduService = huduService;
            _haloPSAService = haloPSAService;
        }

        public async Task<Dictionary<string, string>> DeleteClientAsync(ClientModel client, List<string> systems)
        {
            var results = new Dictionary<string, string>();

            foreach (var system in systems)
            {
                switch (system)
                {
                    case "Hudu":
                        if (!string.IsNullOrWhiteSpace(client.HuduId))
                        {
                            await _huduService.DeleteCompanyAsync(client.HuduId);
                            results["Hudu"] = "Deleted successfully.";
                        }
                        else
                        {
                            results["Hudu"] = "No Hudu ID to delete.";
                        }
                        break;

                        // Add similar logic for other systems
                }
            }

            return results;
        }
    }
}
