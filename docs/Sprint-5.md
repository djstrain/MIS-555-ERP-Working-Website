# Sprint 5

Concise definitions and example code covering CRM page logic, CRUD operations, and role-based access implemented in this app.

## 1) CRM Page Logic
- **Definition:** The CRM page code-behind loads recent entities, computes metrics, and uses a query parameter (`ActiveModule`) to switch tabs.
- **Sample (C#, `Pages/CustomerRelationship.cshtml.cs`, core of `OnGetAsync`):**

```csharp
public async Task<IActionResult> OnGetAsync()
{
    if (!IsCrmAllowed()) return RedirectToPage("/Index");

    ActiveModule = NormalizeModule(ActiveModule);

    Companies = await _db.Companies.OrderByDescending(c => c.Id).Take(25).ToListAsync();
    Contacts = await _db.Contacts.OrderByDescending(c => c.Id).Take(25).Include(c => c.Company).ToListAsync();
    Opportunities = await _db.Opportunities.OrderByDescending(o => o.Id).Take(25).Include(o => o.Company).ToListAsync();
    Activities = await _db.Activities.OrderByDescending(a => a.Id).Take(20).Include(a => a.Company).Include(a => a.Contact).ToListAsync();
    Notes = await _db.Notes.OrderByDescending(n => n.Id).Take(20).Include(n => n.Company).Include(n => n.Contact).ToListAsync();

    TotalCompanies = await _db.Companies.CountAsync();
    TotalContacts  = await _db.Contacts.CountAsync();
    TopOpportunity = await _db.Opportunities
        .OrderByDescending(o => o.Value ?? 0)
        .Include(o => o.Company)
        .FirstOrDefaultAsync();

    return Page();
}
```

## 2) CRUD Operations
- **Definition:** Each entity (Company, Contact, Opportunity, Activity, Note) has `OnPostAdd*`, `OnPostEdit*`, and `OnPostDelete*` handlers using EF Core. Validation is scoped per-form (`TryValidateModel` on the specific bound object) to avoid cross-form errors.
- **Sample (C#, Company handlers in `Pages/CustomerRelationship.cshtml.cs`):**

```csharp
[BindProperty] public Company NewCompany { get; set; } = new();
[BindProperty] public Company EditCompany { get; set; } = new();

public async Task<IActionResult> OnPostAddCompanyAsync()
{
    if (!IsCrmAllowed()) return RedirectToPage("/Index");
    ValidationErrors.Clear();
    ModelState.Clear();
    var isValid = TryValidateModel(NewCompany, nameof(NewCompany));
    if (string.IsNullOrWhiteSpace(NewCompany.Name)) isValid = false;

    if (!isValid)
    {
        ActiveModule = "companies"; await OnGetAsync(); return Page();
    }

    NewCompany.CreatedAt = DateTime.UtcNow;
    _db.Companies.Add(NewCompany);
    await _db.SaveChangesAsync();
    return RedirectToPage("/CustomerRelationship", new { ActiveModule = "companies" });
}

public async Task<IActionResult> OnPostEditCompanyAsync()
{
    if (!IsCrmAllowed()) return RedirectToPage("/Index");
    if (EditCompany.Id <= 0 || string.IsNullOrWhiteSpace(EditCompany.Name))
    { ActiveModule = "companies"; await OnGetAsync(); return Page(); }

    var existing = await _db.Companies.FirstOrDefaultAsync(c => c.Id == EditCompany.Id);
    if (existing != null)
    {
        existing.Name = EditCompany.Name.Trim();
        existing.Industry = EditCompany.Industry?.Trim();
        existing.Website = EditCompany.Website?.Trim();
        await _db.SaveChangesAsync();
    }
    return RedirectToPage("/CustomerRelationship", new { ActiveModule = "companies" });
}

public async Task<IActionResult> OnPostDeleteCompanyAsync(int id)
{
    if (!IsCrmAllowed()) return RedirectToPage("/Index");
    var existing = await _db.Companies.FirstOrDefaultAsync(c => c.Id == id);
    if (existing != null)
    { _db.Companies.Remove(existing); await _db.SaveChangesAsync(); }
    return RedirectToPage("/CustomerRelationship", new { ActiveModule = "companies" });
}
```

## 3) Role-Based Access
- **Definition:** Enforced at UI and server layers. UI hides tabs/buttons based on role; server guards block unauthorized access and CRUD. Customers can access CRM; Managers/Admins can access everything.
- **Sample (C#, server guard in `Pages/CustomerRelationship.cshtml.cs`):**

```csharp
private bool IsCrmAllowed()
{
    var role = HttpContext?.Session?.GetString("UserRole");
    if (string.IsNullOrWhiteSpace(role)) return false;
    return role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
        || role.Equals("Manager", StringComparison.OrdinalIgnoreCase)
        || role.Equals("Customer", StringComparison.OrdinalIgnoreCase);
}

public async Task<IActionResult> OnPostAddContactAsync()
{
    if (!IsCrmAllowed()) return RedirectToPage("/Index");
    // validate + save ...
    return RedirectToPage("/CustomerRelationship", new { ActiveModule = "contacts" });
}
```

- **Sample (Razor, navbar visibility in `Pages/Shared/_Layout.cshtml`):**

```razor
@{
    var role = Context?.Session?.GetString("UserRole");
    var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
    var isManager = string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);
    var isVendor = string.Equals(role, "Vendor", StringComparison.OrdinalIgnoreCase);
    var isCustomer = string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase);
}
@if (isAdmin || isManager)
{
    <li class="nav-item"><a class="nav-link text-dark" asp-page="/HRM">HRM</a></li>
    <li class="nav-item"><a class="nav-link text-dark" asp-page="/Inventory">Inventory</a></li>
    <li class="nav-item"><a class="nav-link text-dark" asp-page="/FinancialManagement">Financial</a></li>
}
@if (isVendor || isAdmin || isManager)
{ <li class="nav-item"><a class="nav-link text-dark" asp-page="/VendorManagement">Vendors</a></li> }
@if (isCustomer || isAdmin || isManager)
{ <li class="nav-item"><a class="nav-link text-dark" asp-page="/CustomerRelationship">CRM</a></li> }
```

---

This document captures the exact approach implemented for Sprint 5.
