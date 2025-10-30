using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // Clear all session data
        HttpContext.Session.Clear();
        
        // Set a success message
        TempData["SuccessMessage"] = "You have been successfully logged out.";
        
        // Redirect to login page
        return RedirectToPage("/Index");
    }
}
