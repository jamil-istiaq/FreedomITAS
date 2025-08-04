using FreedomITAS.API_Serv;
using FreedomITAS.Data;
using FreedomITAS.Models;
using FreedomITAS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace FreedomITAS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ClientCreateService _clientCreateService;
        private readonly AppDbContext _context;
        private readonly RouteProtector _protector;

        [BindProperty]
        public List<string> SelectedSource { get; set; }
        [BindProperty]
        public string ClientId { get; set; }
        [BindProperty]
        public ClientModel EditedClient { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }

        public IList<ClientModel> Clients { get; set; }
        public Dictionary<string, string> EncryptedIds { get; set; }

        public IndexModel(AppDbContext context, RouteProtector protector, ClientCreateService clientCreateService)
        {
            _context = context;
            _protector = protector;
            _clientCreateService = clientCreateService;
        }


        public async Task OnGetAsync()
        {
            Clients = await _context.Clients.ToListAsync();
            EncryptedIds = Clients.ToDictionary(c => c.ClientId, c => _protector.Protect(c.ClientId));
        }

        public IActionResult OnPostSave()
        {
            TempData["Message"] = "Client updated successfully!";
            return RedirectToPage();
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
                clientId = id;
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
            if (client == null)
                return NotFound();

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Client {client.CompanyName} deleted successfully.";
            return RedirectToPage();
        }

        //API Calls      
        public async Task<IActionResult> OnPostPushClientAsync()
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == ClientId);
            if (client == null)
            {
                ModelState.AddModelError("", "Client not found.");
                return Page();
            }

            if (SelectedSource == null || !SelectedSource.Any())
            {
                ModelState.AddModelError("", "Please select at least one system.");
                return Page();
            }

            try
            {
                var results = await _clientCreateService.CreateClientAsync(client, SelectedSource);

                foreach (var result in results)
                {
                    TempData[$"{result.Key}Message"] = result.Value;
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error pushing client: {ex.Message}";
                return RedirectToPage();
            }
        }

    }
}