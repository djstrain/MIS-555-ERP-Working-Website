using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApplication1.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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


    // All authentication is now handled via the database

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid");
                return Page();
            }

            // normalize the email for comparison
            var normalizedEmail = Email?.Trim().ToLowerInvariant() ?? string.Empty;
            _logger.LogInformation("Attempting login for email: {Email}", normalizedEmail);

            // Check if user exists in database
            var user = _context.UserCredentials
                .AsNoTracking() // Optimize query for read-only
                .FirstOrDefault(u => u.Email.ToLower() == normalizedEmail);

            if (user != null)
            {
                _logger.LogInformation("User found in database. Role: {Role}", user.Role);
                _logger.LogDebug("Comparing passwords - Input length: {InputLength}, Stored length: {StoredLength}", 
                    Password?.Length ?? 0, user.Password?.Length ?? 0);
                
                if (string.Equals(user.Password, Password, StringComparison.Ordinal))
                {
                    // Login successful - you should use proper password hashing in production
                    // Store user information in session
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    
                    var userRole = user.Role?.ToLower() ?? "";
                    _logger.LogInformation("Password matched. Session set. Redirecting based on role: {Role}", userRole);
                    
                    if (userRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
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
                    _logger.LogWarning("Password mismatch for email: {Email}", normalizedEmail);
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
