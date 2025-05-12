using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreedomITAS.Data;
using FreedomITAS.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FreedomITAS.Pages.Clients
{
    public class ClientEditModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly RouteProtector _protector;

        public ClientEditModel(AppDbContext context, RouteProtector protector)
        {
            _context = context;
            _protector = protector;
        }

        [BindProperty]
        public ClientModel Client { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            string decryptedId;
            try
            {
                decryptedId = _protector.Unprotect(id);
            }
            catch
            {
                return BadRequest("Invalid or tampered link.");
            }

            Client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == decryptedId);

            if (Client == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var clientToUpdate = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == Client.ClientId);
            if (clientToUpdate == null)
                return NotFound();

            // Update fields
            clientToUpdate.CompanyName = Client.CompanyName;
            clientToUpdate.NumberStreet = Client.NumberStreet;
            clientToUpdate.City = Client.City;
            clientToUpdate.StateName = Client.StateName;
            clientToUpdate.Country = Client.Country;
            clientToUpdate.Postcode = Client.Postcode;
            clientToUpdate.CompanyPhone = Client.CompanyPhone;
            clientToUpdate.CompanyABN = Client.CompanyABN;
            clientToUpdate.ContactFirstName = Client.ContactFirstName;
            clientToUpdate.ContactMiddleName = Client.ContactMiddleName;
            clientToUpdate.ContactLastName = Client.ContactLastName;
            clientToUpdate.ContactEmail = Client.ContactEmail;
            clientToUpdate.ContactMobile = Client.ContactMobile;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Index");
        }
    }
}
