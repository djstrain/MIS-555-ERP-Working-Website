using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Pages;

public class IndexModel : PageModel
{
    // Bind the form fields
    [BindProperty] public string username { get; set; } = string.Empty;
    [BindProperty] public string password { get; set; } = string.Empty;

    // In-memory seeded credentials. THIS IS FOR DEMO PURPOSES ONLY.
    // Storing plaintext passwords is insecure. For production use ASP.NET Core Identity
    // and store hashed passwords.
    private static readonly Dictionary<string, string> _validUsers = new()
    {
        // Admin (existing)
        ["admin@ctrlfreak.com"] = "password",

        // Preset user credentials based on employees in the HRM sample data
         ["jane.doe@ctrlfreak.com"] = "a1b2c3d4",
        ["john.smith@ctrlfreak.com"] = "qWeRtY",
        ["alex.lee@ctrlfreak.com"] = "Pencil12",
        ["sarah.chen@ctrlfreak.com"] = "789_xyz",
        ["david.brown@ctrlfreak.com"] = "m0ckUp",
        ["maria.garcia@ctrlfreak.com"] = "zxcvbn",
        ["michael.clark@ctrlfreak.com"] = "t3stP4ss"
    };

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // normalize the username for comparison
        var user = username?.Trim().ToLowerInvariant() ?? string.Empty;

        if (!string.IsNullOrEmpty(user) && _validUsers.TryGetValue(user, out var expectedPassword))
        {
            if (password == expectedPassword)
            {
                // Login successful
                return RedirectToPage("/Privacy");
            }
        }

        // Invalid login attempt
        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return Page();
    }
}
