using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApplication1.Data;

namespace WebApplication1.Pages;

[RequestSizeLimit(104857600)] // 100 MB request limit for uploads on this page
public class VendorManagementModel : PageModel
{
    private readonly AppDbContents _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VendorManagementModel> _logger;

    public VendorManagementModel(AppDbContents context, IConfiguration configuration, ILogger<VendorManagementModel> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;

        // Load configured limits/paths
        _maxUploadSizeBytes = GetConfiguredMaxUploadSizeBytes();
        _uploadsRoot = ResolveUploadsRoot();
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

    // Attachments - selection and listing
    [BindProperty(SupportsGet = true)]
    public int? SelectedVendorId { get; set; }

    public List<VendorFile> VendorFiles { get; set; } = new();

    // Upload bindings
    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    [BindProperty]
    [StringLength(500)]
    public string? FileDescription { get; set; }

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

        // Load files only when the user has explicitly selected a vendor
        if (SelectedVendorId.HasValue)
        {
            VendorFiles = await _context.VendorFiles
                .Where(f => f.VendorID == SelectedVendorId.Value)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }
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
        // maintain selected vendor and files list
        if (SelectedVendorId.HasValue)
        {
            VendorFiles = await _context.VendorFiles
                .Where(f => f.VendorID == SelectedVendorId.Value)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }
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

    // Max upload size (defaults to 100 MB if not configured)
    private readonly long _maxUploadSizeBytes = 100L * 1024L * 1024L;

    // Root path where files are stored (defaults to App_Data/vendor-files)
    private readonly string _uploadsRoot = string.Empty;

    private long GetConfiguredMaxUploadSizeBytes()
    {
        // appsettings: Uploads:MaxSizeMB (int)
        var mb = _configuration.GetValue<int?>("Uploads:MaxSizeMB") ?? 100;
        if (mb <= 0) mb = 100;
        return (long)mb * 1024L * 1024L;
    }

    private string ResolveUploadsRoot()
    {
        var configured = _configuration["Uploads:RootPath"];
        string root;
        if (string.IsNullOrWhiteSpace(configured))
        {
            root = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "vendor-files");
        }
        else
        {
            // If relative, make it relative to content root
            root = Path.IsPathRooted(configured)
                ? configured
                : Path.Combine(Directory.GetCurrentDirectory(), configured);
        }
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        return root;
    }

    private string GetVendorFolder(int vendorId)
    {
        var vendorFolder = Path.Combine(_uploadsRoot, vendorId.ToString());
        if (!Directory.Exists(vendorFolder)) Directory.CreateDirectory(vendorFolder);
        return vendorFolder;
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to upload files.";
            return RedirectToPage("/Privacy");
        }

        if (!SelectedVendorId.HasValue)
        {
            TempData["ErrorMessage"] = "Please select a vendor before uploading.";
            return RedirectToPage(new { selectedVendorId = SelectedVendorId });
        }

        var vendor = await _context.Vendors.FindAsync(SelectedVendorId.Value);
        if (vendor == null)
        {
            TempData["ErrorMessage"] = "Selected vendor not found.";
            return RedirectToPage();
        }

        if (UploadFile == null || UploadFile.Length == 0)
        {
            TempData["ErrorMessage"] = "No file selected or file is empty.";
            return RedirectToPage(new { selectedVendorId = SelectedVendorId });
        }

        if (UploadFile.Length > _maxUploadSizeBytes)
        {
            TempData["ErrorMessage"] = $"File exceeds the maximum size of {_maxUploadSizeBytes / (1024 * 1024)} MB.";
            return RedirectToPage(new { selectedVendorId = SelectedVendorId });
        }

        // Generate stored filename (GUID + original extension)
        var originalName = Path.GetFileName(UploadFile.FileName);
        var ext = Path.GetExtension(originalName);
        var storedName = $"{Guid.NewGuid()}{ext}";

        var vendorFolder = GetVendorFolder(SelectedVendorId.Value);
        var targetPath = Path.Combine(vendorFolder, storedName);

        try
        {
            string sha256Hex;
            using (var fileStream = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var sha = SHA256.Create())
            using (var crypto = new CryptoStream(fileStream, sha, CryptoStreamMode.Write))
            {
                await UploadFile.CopyToAsync(crypto);
                crypto.FlushFinalBlock();
                sha256Hex = Convert.ToHexString(sha.Hash!);
            }

            var record = new VendorFile
            {
                VendorID = SelectedVendorId.Value,
                OriginalFileName = originalName,
                StoredFileName = storedName,
                ContentType = UploadFile.ContentType,
                SizeBytes = (ulong)UploadFile.Length,
                Description = FileDescription,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = HttpContext.Session.GetString("UserEmail"),
                SHA256 = sha256Hex
            };

            _context.VendorFiles.Add(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Uploaded '{originalName}' for vendor '{vendor.VendorName}'.";
            _logger.LogInformation("Uploaded file {File} ({Size} bytes) for VendorID={VendorId}", originalName, UploadFile.Length, vendor.VendorID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed for VendorID={VendorId}", SelectedVendorId);
            TempData["ErrorMessage"] = $"Upload failed: {ex.Message}";
        }

        return RedirectToPage(new { selectedVendorId = SelectedVendorId });
    }

    public async Task<IActionResult> OnGetDownloadAsync(int fileId)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to download files.";
            return RedirectToPage("/Privacy");
        }

        var file = await _context.VendorFiles.FindAsync(fileId);
        if (file == null)
        {
            TempData["ErrorMessage"] = "File not found.";
            return RedirectToPage();
        }

        var vendorFolder = GetVendorFolder(file.VendorID);
        var path = Path.Combine(vendorFolder, file.StoredFileName);
        if (!System.IO.File.Exists(path))
        {
            TempData["ErrorMessage"] = "File is missing on server.";
            return RedirectToPage(new { selectedVendorId = file.VendorID });
        }

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        var downloadName = file.OriginalFileName;
        return File(stream, contentType, downloadName);
    }

    // Preview handler for images - returns inline content (no download name)
    public async Task<IActionResult> OnGetPreviewAsync(int fileId)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized();
        }

        var file = await _context.VendorFiles.FindAsync(fileId);
        if (file == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Preview only available for images.");
        }

        var vendorFolder = GetVendorFolder(file.VendorID);
        var path = Path.Combine(vendorFolder, file.StoredFileName);
        if (!System.IO.File.Exists(path)) return NotFound();

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, file.ContentType);
    }

    public async Task<IActionResult> OnPostDeleteFileAsync(int fileId, int selectedVendorId)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to delete files.";
            return RedirectToPage("/Privacy");
        }

        var file = await _context.VendorFiles.FindAsync(fileId);
        if (file != null)
        {
            var vendorFolder = GetVendorFolder(file.VendorID);
            var path = Path.Combine(vendorFolder, file.StoredFileName);
            if (System.IO.File.Exists(path))
            {
                try { System.IO.File.Delete(path); } catch { /* ignore IO errors */ }
            }

            _context.VendorFiles.Remove(file);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Deleted file '{file.OriginalFileName}'.";
        }
        else
        {
            TempData["ErrorMessage"] = "File not found.";
        }

        return RedirectToPage(new { selectedVendorId });
    }

    // Utility: human-readable file size for UI
    public string FormatBytes(ulong bytes)
    {
        const double kb = 1024.0, mb = kb * 1024.0, gb = mb * 1024.0;
        double b = bytes;
        if (b >= gb) return (b / gb).ToString("0.##") + " GB";
        if (b >= mb) return (b / mb).ToString("0.##") + " MB";
        if (b >= kb) return (b / kb).ToString("0.##") + " KB";
        return bytes + " B";
    }
}
