using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using FreedomITAS.Models;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Email or Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByNameAsync(Input.Username)
                   ?? await _userManager.FindByEmailAsync(Input.Username);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Input.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return RedirectToPage("/Index");
        }

        ModelState.AddModelError(string.Empty, "Invalid credentials.");
        return Page();
    }
}
