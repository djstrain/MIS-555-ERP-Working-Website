using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Data;

namespace WebApplication1.Pages;

public class VendorManagementModel : PageModel
{
    private readonly AppDbContents _context;

    public VendorManagementModel(AppDbContents context)
    {
        _context = context;
    }

    // Properties for display
    public List<Vendor> Vendors { get; set; } = new();
    public int TotalVendors { get; set; }
    public int ActiveVendors { get; set; }
    public decimal AverageRating { get; set; }

    // Input model for adding/editing vendors
    [BindProperty]
    public VendorInputModel Input { get; set; } = new();

    // Property to track which vendor is being edited
    [BindProperty]
    public int? EditingVendorId { get; set; }

    // Search and filter properties
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TypeFilter { get; set; }

    public class VendorInputModel
    {
        [Required(ErrorMessage = "Vendor name is required")]
        [StringLength(150)]
        public string VendorName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? VendorType { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public decimal? Rating { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check admin access
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to access the Vendor Management page.";
            return RedirectToPage("/Privacy");
        }

        await LoadVendorsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        // Check admin access
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to access the Vendor Management page.";
            return RedirectToPage("/Privacy");
        }

        if (!ModelState.IsValid)
        {
            await LoadVendorsAsync();
            return Page();
        }

        var vendor = new Vendor
        {
            VendorName = Input.VendorName,
            ContactPerson = Input.ContactPerson,
            Email = Input.Email,
            VendorType = Input.VendorType,
            Status = Input.Status,
            Rating = Input.Rating,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Vendor '{vendor.VendorName}' added successfully!";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        // Check admin access
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to access the Vendor Management page.";
            return RedirectToPage("/Privacy");
        }

        if (!EditingVendorId.HasValue)
        {
            TempData["ErrorMessage"] = "Invalid vendor ID.";
            return RedirectToPage();
        }

        if (!ModelState.IsValid)
        {
            await LoadVendorsAsync();
            return Page();
        }

        var vendor = await _context.Vendors.FindAsync(EditingVendorId.Value);
        if (vendor == null)
        {
            TempData["ErrorMessage"] = "Vendor not found.";
            return RedirectToPage();
        }

        vendor.VendorName = Input.VendorName;
        vendor.ContactPerson = Input.ContactPerson;
        vendor.Email = Input.Email;
        vendor.VendorType = Input.VendorType;
        vendor.Status = Input.Status;
        vendor.Rating = Input.Rating;
        vendor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Vendor '{vendor.VendorName}' updated successfully!";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int vendorId)
    {
        // Check admin access
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to access the Vendor Management page.";
            return RedirectToPage("/Privacy");
        }

        var vendor = await _context.Vendors.FindAsync(vendorId);
        if (vendor != null)
        {
            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Vendor '{vendor.VendorName}' deleted successfully!";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSearchAsync()
    {
        // Check admin access
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to access the Vendor Management page.";
            return RedirectToPage("/Privacy");
        }

        await LoadVendorsAsync();
        return Page();
    }

    private async Task LoadVendorsAsync()
    {
        var query = _context.Vendors.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(v => 
                v.VendorName.Contains(SearchTerm) || 
                (v.ContactPerson != null && v.ContactPerson.Contains(SearchTerm)) ||
                (v.Email != null && v.Email.Contains(SearchTerm)));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
        {
            query = query.Where(v => v.Status == StatusFilter);
        }

        // Apply type filter
        if (!string.IsNullOrWhiteSpace(TypeFilter) && TypeFilter != "All")
        {
            query = query.Where(v => v.VendorType == TypeFilter);
        }

        Vendors = await query.OrderBy(v => v.VendorName).ToListAsync();

        // Calculate metrics
        var allVendors = await _context.Vendors.ToListAsync();
        TotalVendors = allVendors.Count;
        ActiveVendors = allVendors.Count(v => v.Status == "Active");
        AverageRating = allVendors.Any(v => v.Rating.HasValue) 
            ? allVendors.Where(v => v.Rating.HasValue).Average(v => v.Rating!.Value) 
            : 0;
    }
}
