using System.Text.Json;
using FreedomITAS.Models;
using FreedomITAS.API_Serv;
using FreedomITAS;

public class ClientPushService
{
    private readonly ZomentumService _zomentumService;
    private readonly HuduService _huduService;
    private readonly HaloPSAService _haloPSAService;
    private readonly SyncroService _syncroService;
    private readonly DreamscapeService _dreamscapeService;
    private readonly Pax8Service _pax8Service; 

    public ClientPushService(Pax8Service pax8Service, ZomentumService zomentumService, HuduService huduService, HaloPSAService haloPSAService, SyncroService syncroService, DreamscapeService dreamscapeService  )
    {
        _zomentumService = zomentumService;
        _huduService = huduService;
        _haloPSAService = haloPSAService;
        _syncroService = syncroService;
        _dreamscapeService = dreamscapeService;
        _pax8Service = pax8Service;
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
                            address_line_2 = "",
                            city = client.City,
                            state = client.StateName,
                            pincode = client.Postcode,
                            country = client.Country,
                            phone = client.CompanyPhone
                        },
                        compnay_types= client.CompanyType,
                        website=client.Website,

                    };
                    results["Zomentum"] = await _zomentumService.CreateClientAsync(zomentumPayload);
                    break;

                case "HaloPSA":
                    var haloPayload = new[] {
                    new
                    {
                        name = client.CompanyName,
                        toplevel_name = "FIT_App",
                        from_address_override=$"{client.NumberStreet}{client.City}{client.StateName}{client.Country}{client.Postcode}".Trim(),
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
                        compnay_type = client.CompanyType,
                        address_line_1=client.NumberStreet,
                        city = client.City,
                        state = client.StateName,
                        zip = client.Postcode,
                        country_name = client.Country,
                        phone_number = client.CompanyPhone,
                        website = client.Website
                    };
                    var huduResponse = await _huduService.CreateCompanyAsync(huduPayload);
                    results["Hudu"] = await huduResponse.Content.ReadAsStringAsync();
                    break;

                case "Syncro":
                    var syncroPayload = new
                    {
                        business_name = client.CompanyLegalName,
                        firstname= client.ContactFirstName,
                        lastname= $"{client.ContactMiddleName} {client.ContactLastName}".Trim(),
                        email=client.ContactEmail, 
                        phone=client.CompanyPhone,
                        mobile=client.ContactMobile,                        
                        address = client.NumberStreet,
                        city = client.City,
                        state = client.StateName,
                        zip = client.Postcode,
                       
                    };
                    var syncroResponse = await _syncroService.CreateCompanyAsync(syncroPayload);
                    results["Syncro"] = await syncroResponse.Content.ReadAsStringAsync();
                    break;

                case "Dreamscape":
                    string companyName = client.CompanyName ?? string.Empty;
                    string cleaned = new string(companyName.Where(char.IsLetterOrDigit).ToArray()).ToLower();
                    string shortCode = "xx";
                    if (cleaned.Length >= 2)
                    {
                        char firstLetter = cleaned.FirstOrDefault(char.IsLetter);
                        char firstDigit = cleaned.FirstOrDefault(char.IsDigit);
                        if (firstLetter == default) firstLetter = 'x';
                        if (firstDigit == default) firstDigit = '0';
                        shortCode = $"{firstLetter}{firstDigit}";
                    }
                    string username = $"admin_{shortCode}";
                    
                    string rawType = (client.CompanyType ?? "").ToLower();
                    string accountType;
                    switch (rawType)
                    {
                        case "personal":
                        case "business":
                            accountType = rawType;
                            break;
                        default:
                            Console.WriteLine($"[Warning] Unknown company type '{rawType}', defaulting to 'business'");
                            accountType = "business";
                            break;
                    }
                    var dreamPayload = new
                    {
                        username = username,
                        first_name = client.ContactFirstName,
                        last_name = $"{client.ContactMiddleName} {client.ContactLastName}".Trim(),
                        address = client.NumberStreet,
                        city = client.City,
                        country = client.Country,
                        country_code=client.CountryCode,
                        state = client.StateName,
                        post_code = client.Postcode,
                        phone = client.CompanyPhone,
                        mobile = client.ContactMobile,
                        email = client.ContactEmail,
                        account_type = accountType,
                        business_name = client.CompanyLegalName,
                        business_number_type = "ABN",
                        business_number = client.CompanyABN,

                    };
                    var dreamResponse = await _dreamscapeService.CreateCompanyAsync(dreamPayload);
                    results["Dreamscape"] = await dreamResponse.Content.ReadAsStringAsync();                    
                    break;

                case "Pax8":
                    string postcode = string.IsNullOrWhiteSpace(client.Postcode) ? "0000" : client.Postcode.Trim();
                    var pax8Payload = 
                    new
                    {
                        name = client.CompanyName,
                        company_type = client.CompanyType,
                        address = new
                        {
                            street = client.NumberStreet,
                            city = client.City,
                            stateOrProvince = client.StateName,
                            postalCode = postcode,
                            country = client.Country
                            
                        },
                        phone = client.CompanyPhone,
                        website = client.Website,
                        billOnBehalfOfEnabled=true,
                        selfServiceAllowed=true,
                        orderApprovalRequired = true
                    };
                    var pax8Response = await _pax8Service.CreateClientAsync(pax8Payload);
                    results["Pax8"] = await pax8Response.Content.ReadAsStringAsync();
                    var resultJson = await pax8Response.Content.ReadAsStringAsync();
                    Console.WriteLine("Pax8 API Create Response:\n" + resultJson);

                    if (!pax8Response.IsSuccessStatusCode)
                    {
                        var error = await pax8Response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Pax8 Error: {pax8Response.StatusCode} - {error}");
                        throw new Exception($"Failed to create Pax8 customer: {error}");
                    }

                    break;

                default:
                    results[system] = "Unsupported system";
                    break;
            }
        }

        return results;
    }
}
