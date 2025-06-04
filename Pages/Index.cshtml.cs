using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomITAS.Models;
using FreedomITAS.Data;
using Newtonsoft.Json.Linq;
using FreedomITAS.API_Serv;
using System.Text.Json;

namespace FreedomITAS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly RouteProtector _protector;


        public IndexModel(AppDbContext context, RouteProtector protector, ZomentumService zomentumService)
        {
            _context = context;
            _protector = protector;
            _zomentumService = zomentumService;
        }

        public IList<ClientModel> Clients { get; set; }
        public Dictionary<string, string> EncryptedIds { get; set; }

        public async Task OnGetAsync()
        {
            Clients = await _context.Clients.ToListAsync();
            EncryptedIds = Clients.ToDictionary(c => c.ClientId, c => _protector.Protect(c.ClientId));
        }

        [BindProperty]
        public List<ClientModel> Client { get; set; } = new List<ClientModel>();

        [BindProperty]
        public ClientModel EditedClient { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }


        public IActionResult OnPostSave()
        {
            TempData["Message"] = "Client updated successfully!";
            return RedirectToPage(); // Refresh
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            string clientId;

            try
            {
                clientId = _protector.Unprotect(id);
            }
            catch
            {
                clientId = id; // Assume plain ID if unprotect fails
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
            if (client == null)
                return NotFound();

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Client {client.CompanyName} deleted successfully.";
            return RedirectToPage(); // Refresh page
        }

        //API Calls
        //private readonly HaloPSAService _haloService;

        //public JArray Clients { get; set; }
        //public string ClientsJson { get; set; }

        //public IndexModel(HaloPSAService haloService)
        //{
        //    _haloService = haloService;
        //}

        //public async Task OnGetAsync()
        //{
        //    try
        //    {
        //        ClientsJson = await _haloService.GetClientsAsync();

        //        var parsed = JObject.Parse(ClientsJson);
        //        Clients = (JArray)parsed["users"]; 
        //    }
        //    catch (Exception ex)
        //    {
        //        ClientsJson = $"Error: {ex.Message}";
        //    }
        //}

        private readonly ZomentumService _zomentumService;

        //public JsonElement ClientsJson { get; private set; }
        [BindProperty]
        public string SelectedSource { get; set; }

        [BindProperty]
        public string ClientId { get; set; }        
        public async Task<IActionResult> OnPostPushClientAsync()
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == ClientId);

            if (client == null)
            {
                ModelState.AddModelError("", "Client not found.");
                return Page();
            }

            try
            {
                var payload = new
                {
                    name = client.CompanyName,
                    billing_address = new
                    {
                        address_line_1 = client.NumberStreet,
                        city = client.City,
                        state = client.StateName,
                        pincode = client.Postcode,
                        country = client.Country
                    },
                    phone = client.CompanyPhone,                    
                };

                switch (SelectedSource)
                {
                    case "Zomentum":
                        var response = await _zomentumService.CreateClientAsync(payload);
                        TempData["Message"] = response;
                        break;
                    default:
                        TempData["Message"] = "No valid system selected.";
                        break;
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                return RedirectToPage();
            }
        }

    }
}
