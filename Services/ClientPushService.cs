using System.Text.Json;
using FreedomITAS.Models;
using FreedomITAS.API_Serv;

public class ClientPushService
{
    private readonly ZomentumService _zomentumService;
    private readonly HuduService _huduService;

    public ClientPushService(ZomentumService zomentumService, HuduService huduService)
    {
        _zomentumService = zomentumService;
        _huduService = huduService;
    }

    public async Task<Dictionary<string, string>> PushClientAsync(ClientModel client, List<string> systems)
    {
        var results = new Dictionary<string, string>();

        foreach (var system in systems)
        {
            switch (system)
            {
                case "Zomentum":
                    var zomentumPayload = new
                    {
                        name = client.CompanyName,
                        phone = client.CompanyPhone,
                        billing_address = new
                        {
                            address_line_1 = client.NumberStreet,
                            city = client.City,
                            state = client.StateName,
                            pincode = client.Postcode,
                            country = client.Country
                        }
                    };
                    results["Zomentum"] = await _zomentumService.CreateClientAsync(zomentumPayload);
                    break;

                case "Hudu":
                    var huduPayload = new
                    {
                        name = client.CompanyName,
                        city = client.City,
                        state = client.StateName,
                        zip = client.Postcode,
                        country_name = client.Country,
                        phone_number = client.CompanyPhone
                    };
                    var huduResponse = await _huduService.CreateCompanyAsync(huduPayload);
                    results["Hudu"] = await huduResponse.Content.ReadAsStringAsync();
                    break;

                default:
                    results[system] = "Unsupported system";
                    break;
            }
        }

        return results;
    }
}
