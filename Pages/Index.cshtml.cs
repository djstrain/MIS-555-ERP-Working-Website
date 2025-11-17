using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApplication1.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
<<<<<<< Updated upstream
using Microsoft.EntityFrameworkCore;
=======
>>>>>>> Stashed changes

namespace WebApplication1.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContents _context;
    private readonly ILogger<IndexModel> _logger;

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public IndexModel(AppDbContents context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }


<<<<<<< Updated upstream
    // All authentication is now handled via the database
=======
    // Removed in-memory credentials - using database only
>>>>>>> Stashed changes

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Model Error: {Error}", error.ErrorMessage);
                }
                return Page();
            }

            // normalize the email for comparison
            var normalizedEmail = Email?.Trim().ToLowerInvariant() ?? string.Empty;
            _logger.LogInformation("Attempting login for email: {Email}", normalizedEmail);

<<<<<<< Updated upstream
=======
            // Debug: Check all users in database
            var allUsers = _context.UserCredentials.ToList();
            _logger.LogInformation("Found {Count} users in database", allUsers.Count);
            foreach (var dbUser in allUsers)
            {
                _logger.LogInformation("DB User - Email: {Email}, Role: {Role}", 
                    dbUser.Email, dbUser.Role);
            }

>>>>>>> Stashed changes
            // Check if user exists in database
            var user = _context.UserCredentials
                .AsNoTracking() // Optimize query for read-only
                .FirstOrDefault(u => u.Email.ToLower() == normalizedEmail);

            if (user != null)
            {
                _logger.LogInformation("User found in database. Role: {Role}", user.Role);
<<<<<<< Updated upstream
                _logger.LogDebug("Comparing passwords - Input length: {InputLength}, Stored length: {StoredLength}", 
                    Password?.Length ?? 0, user.Password?.Length ?? 0);
                
                if (string.Equals(user.Password, Password, StringComparison.Ordinal))
=======

                // Trim stored and input passwords to avoid whitespace mismatch
                var storedPwd = (user.Password ?? string.Empty).Trim();
                var inputPwd = (Password ?? string.Empty).Trim();

                // Detailed debug logging (only for local troubleshooting)
                _logger.LogDebug("Stored password (raw): '{StoredPwd}'", storedPwd);
                _logger.LogDebug("Input password (raw): '{InputPwd}'", inputPwd);
                _logger.LogDebug("Stored length: {StoredLen}, Input length: {InputLen}", storedPwd.Length, inputPwd.Length);

                // Compare using an explicit ordinal comparison
                if (string.Equals(storedPwd, inputPwd, StringComparison.Ordinal))
>>>>>>> Stashed changes
                {
                    // Login successful - you should use proper password hashing in production
                    // Store user information in session
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserEmail", user.Email);
<<<<<<< Updated upstream
                    
                    var userRole = user.Role?.ToLower() ?? "";
                    _logger.LogInformation("Password matched. Session set. Redirecting based on role: {Role}", userRole);
                    
                    if (userRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
=======

                    var userRole = (user.Role ?? string.Empty).ToLowerInvariant();
                    _logger.LogInformation("Password matched. Session set. Redirecting based on role: {Role}", userRole);

                    if (userRole == "admin")
>>>>>>> Stashed changes
                    {
                        return RedirectToPage("/HRM");
                    }
                    else if (userRole.Equals("user", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToPage("/Privacy");
                    }
                    else
                    {
                        // Handle any other roles if needed
                        _logger.LogWarning("Unknown role: {Role}", userRole);
                        ModelState.AddModelError(string.Empty, "Invalid role assignment.");
                        return Page();
                    }
                }
                else
                {
                    _logger.LogWarning("Password mismatch for email: {Email}. StoredLen={StoredLen}, InputLen={InputLen}", normalizedEmail, storedPwd.Length, inputPwd.Length);
                }
            }
            else
            {
                _logger.LogWarning("User not found in database: {Email}", normalizedEmail);
            }

            // Invalid login attempt
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for email: {Email}", Email);
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            return Page();
        }
    }
}
