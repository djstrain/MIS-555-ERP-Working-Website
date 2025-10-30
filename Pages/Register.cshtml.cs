using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebApplication1.Data;

namespace WebApplication1.Pages;

public class RegisterModel : PageModel
{
    private readonly AppDbContents _context;
    private readonly ILogger<RegisterModel> _logger;

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public RegisterModel(AppDbContents context, ILogger<RegisterModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if user already exists
            var existingUser = _context.UserCredentials
                .FirstOrDefault(u => u.Email.ToLower() == Input.Email.ToLower());

            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this email already exists.");
                _logger.LogWarning("Registration attempt with existing email: {Email}", Input.Email);
                return Page();
            }

            // Create new user
            var newUser = new UserCredentials
            {
                Email = Input.Email.Trim(),
                Password = Input.Password, // Note: In production, hash this password!
                Role = Input.Role,
                CreatedAt = DateTime.UtcNow
            };

            // Add to database
            _context.UserCredentials.Add(newUser);
            _context.SaveChanges();

            _logger.LogInformation("New user registered: {Email} with role: {Role}", Input.Email, Input.Role);

            // Redirect to login page with success message
            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", Input.Email);
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            return Page();
        }
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";
    }
}