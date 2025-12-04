using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;
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

    public async Task OnGet()
    {
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
    }

    public IActionResult OnPostAdd(InventoryItem input)
    {
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
        var existing = _db.InventoryItems.FirstOrDefault(i => i.Id == id);
        if (existing != null)
        {
            _db.InventoryItems.Remove(existing);
            _db.SaveChanges();
        }
        return RedirectToPage(new { Category });
    }
}
