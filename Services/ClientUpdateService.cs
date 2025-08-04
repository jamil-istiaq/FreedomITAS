using FreedomITAS.API_Serv;
using FreedomITAS.Data;
using FreedomITAS.Models;

namespace FreedomITAS.Services
{
    public class ClientUpdateService
    {
        private readonly HuduService _huduService;
        private readonly HaloPSAService _haloPSAService;
        private readonly AppDbContext _dbContext;

        public ClientUpdateService(HuduService huduService, HaloPSAService haloPSAService, AppDbContext dbContext)
        {
            _huduService = huduService;
            _haloPSAService = haloPSAService;
            _dbContext = dbContext;
        }

        public async Task<Dictionary<string, string>> UpdateClientAsync(ClientModel client, List<string> systems)
        {
            var results = new Dictionary<string, string>();

            foreach (var system in systems)
            {
                switch (system)
                {
                    case "Hudu":
                        if (string.IsNullOrEmpty(client.HuduId))
                        {
                            results["Hudu"] = "No Hudu ID found for update.";
                            break;
                        }

                        var huduPayload = new
                        {
                            name = client.CompanyName,
                            compnay_type = client.CompanyType,
                            address_line_1 = client.NumberStreet,
                            city = client.City,
                            state = client.StateName,
                            zip = client.Postcode,
                            country_name = client.Country,
                            phone_number = client.CompanyPhone,
                            website = client.Website
                        };

                        await _huduService.UpdateCompanyAsync(client.HuduId, huduPayload);
                        results["Hudu"] = "Updated successfully.";
                        break;

                        // Add cases for other systems
                }
            }

            await _dbContext.SaveChangesAsync();
            return results;
        }
    }
}
