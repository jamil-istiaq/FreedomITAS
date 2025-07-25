using System.Text.Json;
using FreedomITAS.Models;
using FreedomITAS.API_Serv;
using FreedomITAS;

public class ClientPushService
{
    private readonly ZomentumService _zomentumService;
    private readonly HuduService _huduService;
    private readonly HaloPSAService _haloPSAService;

    public ClientPushService(ZomentumService zomentumService, HuduService huduService, HaloPSAService haloPSAService)
    {
        _zomentumService = zomentumService;
        _huduService = huduService;
        _haloPSAService = haloPSAService;
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

                case "HaloPSA":
                    var haloPayload = new[] {
                    new
                    {
                        name = client.CompanyName,
                        toplevel_name = "FIT_App",
                        override_org_address = new
                        {
                            line1 = client.NumberStreet,
                            line2 = client.City,
                            line3 = client.StateName,
                            line4 = client.Country,
                            postcode = client.Postcode
                        },
                        override_org_phone= client.CompanyPhone,
                        override_org_website= client.Website,
                        newclient_contactname= $"{client.ContactFirstName} {client.ContactMiddleName} {client.ContactLastName}".Trim(),
                        newclient_contactemail =client.ContactEmail
                    }
                    };
                    var response = await _haloPSAService.CreateClientAsync(haloPayload);
                    results["HaloPSA"] = await response.Content.ReadAsStringAsync(); ;
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
