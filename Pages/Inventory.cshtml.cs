using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;

public class InventoryModel : PageModel
{
    private readonly AppDbContents _db;

    public InventoryModel(AppDbContents db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    public List<string> Categories { get; set; } = new();
    public List<InventoryItem> Items { get; set; } = new();

    public decimal StockValue { get; set; }
    public int LowStockCount { get; set; }

    // Helper method to check if user is allowed to access Inventory
    private bool IsInventoryAllowed()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole)) return false;
        
        return userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
            || userRole.Equals("Guest", StringComparison.OrdinalIgnoreCase)
            || userRole.Equals("InventoryManager", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!IsInventoryAllowed())
        {
            TempData["ErrorMessage"] = "You do not have permission to access the Inventory page.";
            return RedirectToPage("/Privacy");
        }

        Categories = _db.InventoryItems
            .Select(i => i.Category)
            .Where(c => c != null && c != "")
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var query = _db.InventoryItems.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Category))
        {
            query = query.Where(i => i.Category == Category);
        }

        Items = query
            .OrderBy(i => i.Name)
            .ToList();

        StockValue = Items.Sum(i => i.UnitCost * i.QuantityOnHand);
        LowStockCount = Items.Count(i => i.QuantityOnHand <= i.ReorderLevel);

        return Page();
    }

    public IActionResult OnPostAdd(InventoryItem input)
    {
        if (!IsInventoryAllowed())
        {
            TempData["ErrorMessage"] = "You do not have permission to add inventory items.";
            return RedirectToPage("/Privacy");
        }

        if (!ModelState.IsValid)
        {
            return RedirectToPage(new { Category });
        }
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _db.InventoryItems.Add(input);
        _db.SaveChanges();
        return RedirectToPage(new { Category });
    }

    public IActionResult OnPostEdit(InventoryItem input)
    {
        if (!IsInventoryAllowed())
        {
            TempData["ErrorMessage"] = "You do not have permission to edit inventory items.";
            return RedirectToPage("/Privacy");
        }

        var existing = _db.InventoryItems.FirstOrDefault(i => i.Id == input.Id);
        if (existing == null)
        {
            return RedirectToPage(new { Category });
        }
        existing.SKU = input.SKU;
        existing.Name = input.Name;
        existing.Category = input.Category;
        existing.Location = input.Location;
        existing.QuantityOnHand = input.QuantityOnHand;
        existing.ReorderLevel = input.ReorderLevel;
        existing.UnitCost = input.UnitCost;
        existing.UnitPrice = input.UnitPrice;
        existing.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return RedirectToPage(new { Category });
    }

    public IActionResult OnPostDelete(int id)
    {
        if (!IsInventoryAllowed())
        {
            TempData["ErrorMessage"] = "You do not have permission to delete inventory items.";
            return RedirectToPage("/Privacy");
        }

        var existing = _db.InventoryItems.FirstOrDefault(i => i.Id == id);
        if (existing != null)
        {
            _db.InventoryItems.Remove(existing);
            _db.SaveChanges();
        }
        return RedirectToPage(new { Category });
    }
}
