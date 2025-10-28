using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApplication1.Data;

namespace WebApplication1.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContents _context;

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public IndexModel(AppDbContents context)
    {
        _context = context;
    }


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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // normalize the email for comparison
        var normalizedEmail = Email?.Trim().ToLowerInvariant() ?? string.Empty;

        // Check if user exists in database
        var user = _context.UserCredentials
            .FirstOrDefault(u => u.Email.ToLower() == normalizedEmail);

        if (user != null && user.Password == Password)
        {
            // Login successful - you should use proper password hashing in production
            var userRole = user.Role.ToLower();
            
            if (userRole == "admin")
            {
                return RedirectToPage("/HRM");
            }
            else if (userRole == "user")
            {
                return RedirectToPage("/Privacy");
            }
            else
            {
                // Handle any other roles if needed
                ModelState.AddModelError(string.Empty, "Invalid role assignment.");
                return Page();
            }
        }

        // Invalid login attempt
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }
}
