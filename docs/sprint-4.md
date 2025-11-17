# Sprint 4 Verification – Financial Management

This document proves the following for the Financial Management module:

1) Database connection string is read from configuration (not hardcoded)
2) The app reads records from the database and displays them on the Financial Management page
3) Aggregate metrics (Total Revenue, Total Expenses, Net Balance) are calculated and displayed on page load via section cards

---

## 1) Connection string is loaded from appsettings.json (not hardcoded)

- Location: `Program.cs`
- The connection string is retrieved from configuration via `GetConnectionString("DefaultConnection")` and passed into EF Core.

```csharp
// Program.cs
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);
// Allow per-developer overrides without committing secrets
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Read from configuration (not hardcoded)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContents>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)))
);
```

- Backing configuration: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=rxerp;User=root;Password=<redacted>;AllowPublicKeyRetrieval=True;SslMode=None;"
  }
}
```

This confirms the database connection info comes from configuration files, not literals in source code.

---

## 2)Records are read from the database and rendered in the Financial page

- Location: `Pages/FinancialManagement.cshtml.cs` → `OnGetAsync()`
- The PageModel queries EF Core for entities and includes related data where appropriate.

```csharp
// Pages/FinancialManagement.cshtml.cs (excerpt)
public async Task OnGetAsync()
{ 
    Accounts = await _context.Accounts.OrderBy(a => a.AccountNumber).ToListAsync();
    Partners = await _context.Partners.OrderBy(p => p.PartnerName).ToListAsync();
    Invoices = await _context.Invoices
        .Include(i => i.Partner)
        .OrderByDescending(i => i.InvoiceDate)
        .ToListAsync();
    OpenBalances = await _context.OpenBalances
        .Include(ob => ob.Account)
        .OrderByDescending(ob => ob.BalanceDate)
        .ToListAsync();
    Payments = await _context.Payments
        .Include(p => p.Invoice)
        .OrderByDescending(p => p.PaymentDate)
        .ToListAsync();
    JournalEntries = await _context.JournalEntries
        .Include(je => je.DebitAccount)
        .Include(je => je.CreditAccount)
        .OrderByDescending(je => je.EntryDate)
        .ToListAsync();
    TaxRates = await _context.TaxRates.OrderBy(t => t.TaxCode).ToListAsync();

    // Line items
    InvoiceLines = await _context.InvoiceLines.Include(il => il.Invoice).ToListAsync();
    JournalLines = await _context.JournalLines
        .Include(jl => jl.JournalEntry)
        .Include(jl => jl.Account)
        .ToListAsync();

    CalculateMetrics();
}
```

- Rendering in Razor: `Pages/FinancialManagement.cshtml` (example: Invoices table)

```html
<table class="table table-striped table-hover">
  <thead class="table-light">
    <tr>
      <th>Invoice Number</th>
      <th>Partner</th>
      <th>Amount</th>
      <th>Status</th>
      <th>Invoice Date</th>
      <th>Due Date</th>
    </tr>
  </thead>
  <tbody>
    @foreach (var invoice in Model.Invoices)
    {
      <tr>
        <td>@invoice.InvoiceNumber</td>
        <td>@invoice.Partner?.PartnerName</td>
        <td>@invoice.Amount.ToString("C")</td>
        <td><span class="badge">@invoice.Status</span></td>
        <td>@invoice.InvoiceDate.ToString("MM/dd/yyyy")</td>
        <td>@invoice.DueDate.ToString("MM/dd/yyyy")</td>
      </tr>
    }
  </tbody>
</table>
```

This verifies that the page queries the DB and renders the results to the UI.

---

## 3) Aggregate metrics are calculated and displayed in section cards

- Location (calculation): `Pages/FinancialManagement.cshtml.cs` → `CalculateMetrics()`

```csharp
private void CalculateMetrics()
{
    // Totals
    TotalRevenue = Invoices.Where(i => i.Status != "Overdue").Sum(i => i.Amount);
    TotalExpenses = Accounts.Where(a => a.AccountType == "Expense").Sum(a => a.Balance);
    NetBalance = TotalRevenue - TotalExpenses;

    // Counts
    TotalInvoices = Invoices.Count;
    PaidInvoices = Invoices.Count(i => i.Status == "Paid");
    PendingInvoices = Invoices.Count(i => i.Status == "Pending");
}
```

- Location (display): `Pages/FinancialManagement.cshtml` → Bootstrap cards at the top of the page.

```html
<div class="row mb-5">
  <div class="col-md-4">
    <div class="card bg-light border-primary">
      <div class="card-body">
        <h5 class="card-title">Total Revenue</h5>
        <h3 class="text-primary">@Model.TotalRevenue.ToString("C")</h3>
        <small class="text-muted">@Model.Invoices.Count invoices</small>
      </div>
    </div>
  </div>
  <div class="col-md-4">
    <div class="card bg-light border-danger">
      <div class="card-body">
        <h5 class="card-title">Total Expenses</h5>
        <h3 class="text-danger">@Model.TotalExpenses.ToString("C")</h3>
        <small class="text-muted">@Model.Accounts.Count(a => a.AccountType == "Expense") expense accounts</small>
      </div>
    </div>
  </div>
  <div class="col-md-4">
    <div class="card bg-light border-success">
      <div class="card-body">
        <h5 class="card-title">Net Balance</h5>
        <h3 class="text-success">@Model.NetBalance.ToString("C")</h3>
        <small class="text-muted">@Model.PaidInvoices paid invoices</small>
      </div>
    </div>
  </div>
</div>
```

These cards are visible immediately on page load, confirming that calculations run server-side and are presented consistently.

---

## How to verify quickly
1. Ensure the DB exists and the connection string is set in `appsettings.json` (or `appsettings.Local.json`).
2. Run the site and navigate to `/FinancialManagement`.
3. Confirm data tables render (Accounts, Partners, Invoices, etc.).
4. Confirm the three metric cards show values for Total Revenue, Total Expenses, and Net Balance.

---

## Backend Lead (Member 3) – Dominic Strain

### Bullet Point 1: Implement the PageModel class (FinancialModel) with a list of transactions, filter properties, and a bound transaction object for form data

**Summary:**  
The `FinancialManagementModel` class defines multiple lists representing financial entities (Accounts, Partners, Invoices, Payments, Journal Entries, Tax Rates, Invoice Lines, Journal Lines) and uses `[BindProperty]` attributes to bind form inputs for creating new records. This design supports multi-entity management on a single page.

**Code Proof:**
```csharp
// Location: Pages/FinancialManagement.cshtml.cs
public class FinancialManagementModel : PageModel
{
    private readonly AppDbContents _context;

    // Lists of transactions/entities
    public List<Account> Accounts { get; set; } = new();
    public List<Partner> Partners { get; set; } = new();
    public List<Invoice> Invoices { get; set; } = new();
    public List<OpenBalance> OpenBalances { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public List<JournalEntry> JournalEntries { get; set; } = new();
    public List<TaxRate> TaxRates { get; set; } = new();
    public List<InvoiceLine> InvoiceLines { get; set; } = new();
    public List<JournalLine> JournalLines { get; set; } = new();

    // Bound form properties for creating new records
    [BindProperty] public string NewAccountNumber { get; set; } = string.Empty;
    [BindProperty] public string NewAccountName { get; set; } = string.Empty;
    [BindProperty] public string NewAccountType { get; set; } = string.Empty;
    [BindProperty] public decimal NewAccountBalance { get; set; }

    [BindProperty] public string NewPartnerName { get; set; } = string.Empty;
    [BindProperty] public string NewPartnerType { get; set; } = string.Empty;
    [BindProperty] public string NewPartnerEmail { get; set; } = string.Empty;
    [BindProperty] public string NewPartnerPhone { get; set; } = string.Empty;

    [BindProperty] public int NewInvoicePartnerId { get; set; }
    [BindProperty] public string NewInvoiceNumber { get; set; } = string.Empty;
    [BindProperty] public decimal NewInvoiceAmount { get; set; }
    [BindProperty] public DateTime NewInvoiceDate { get; set; } = DateTime.Today;
    [BindProperty] public DateTime NewInvoiceDueDate { get; set; } = DateTime.Today.AddDays(30);
    [BindProperty] public string NewInvoiceStatus { get; set; } = "Pending";

    [BindProperty] public string NewPaymentNumber { get; set; } = string.Empty;
    [BindProperty] public int NewPaymentInvoiceId { get; set; }
    [BindProperty] public decimal NewPaymentAmount { get; set; }
    [BindProperty] public DateTime NewPaymentDate { get; set; } = DateTime.Today;
    [BindProperty] public string NewPaymentMethod { get; set; } = "Bank Transfer";

    [BindProperty] public string NewJournalNumber { get; set; } = string.Empty;
    [BindProperty] public int NewJournalDebitAccountId { get; set; }
    [BindProperty] public int NewJournalCreditAccountId { get; set; }
    [BindProperty] public decimal NewJournalAmount { get; set; }
    [BindProperty] public string NewJournalDescription { get; set; } = string.Empty;
    [BindProperty] public DateTime NewJournalDate { get; set; } = DateTime.Today;

    [BindProperty] public string NewTaxCode { get; set; } = string.Empty;
    [BindProperty] public decimal NewTaxPercentage { get; set; }
    [BindProperty] public string NewTaxDescription { get; set; } = string.Empty;
    [BindProperty] public string NewTaxType { get; set; } = string.Empty;
    [BindProperty] public DateTime NewTaxEffectiveDate { get; set; } = DateTime.Today;

    [BindProperty] public int NewInvoiceLineInvoiceId { get; set; }
    [BindProperty] public string NewInvoiceLineDescription { get; set; } = string.Empty;
    [BindProperty] public decimal NewInvoiceLineQuantity { get; set; }
    [BindProperty] public decimal NewInvoiceLineUnitPrice { get; set; }

    [BindProperty] public int NewJournalLineEntryId { get; set; }
    [BindProperty] public int NewJournalLineAccountId { get; set; }
    [BindProperty] public decimal NewJournalLineDebit { get; set; }
    [BindProperty] public decimal NewJournalLineCredit { get; set; }
    [BindProperty] public string NewJournalLineDescription { get; set; } = string.Empty;

    [BindProperty] public string FormType { get; set; } = string.Empty;
}
```

---

### Bullet Point 2: Use appropriate Razor handlers, such as OnGet() and OnPost() (or OnPostAsync()) to support retrieving, adding, and recalculating transactions

**Summary:**  
The PageModel uses `OnGetAsync()` to load all financial data from the database and `OnPostAsync()` to route form submissions to specific add methods based on `FormType`. After each add operation, metrics are recalculated automatically.

**Code Proof:**
```csharp
// Location: Pages/FinancialManagement.cshtml.cs

public async Task OnGetAsync()
{
    try
    {
        // Retrieve all data from database
        Accounts = await _context.Accounts.OrderBy(a => a.AccountNumber).ToListAsync();
        Partners = await _context.Partners.OrderBy(p => p.PartnerName).ToListAsync();
        Invoices = await _context.Invoices
            .Include(i => i.Partner)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
        OpenBalances = await _context.OpenBalances
            .Include(ob => ob.Account)
            .OrderByDescending(ob => ob.BalanceDate)
            .ToListAsync();
        Payments = await _context.Payments
            .Include(p => p.Invoice)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
        JournalEntries = await _context.JournalEntries
            .Include(je => je.DebitAccount)
            .Include(je => je.CreditAccount)
            .OrderByDescending(je => je.EntryDate)
            .ToListAsync();
        TaxRates = await _context.TaxRates.OrderBy(t => t.TaxCode).ToListAsync();
        InvoiceLines = await _context.InvoiceLines
            .Include(il => il.Invoice)
            .OrderByDescending(il => il.CreatedAt)
            .ToListAsync();
        JournalLines = await _context.JournalLines
            .Include(jl => jl.JournalEntry)
            .Include(jl => jl.Account)
            .OrderByDescending(jl => jl.CreatedAt)
            .ToListAsync();

        // Calculate aggregate metrics
        CalculateMetrics();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading financial data: {ex.Message}");
    }
}

public async Task<IActionResult> OnPostAsync()
{
    try
    {
        // Route to appropriate handler based on form type
        if (FormType == "account")
            return await AddAccount();
        else if (FormType == "partner")
            return await AddPartner();
        else if (FormType == "invoice")
            return await AddInvoice();
        else if (FormType == "openbalance")
            return await AddOpenBalance();
        else if (FormType == "payment")
            return await AddPayment();
        else if (FormType == "journalentry")
            return await AddJournalEntry();
        else if (FormType == "taxrate")
            return await AddTaxRate();
        else if (FormType == "invoiceline")
            return await AddInvoiceLine();
        else if (FormType == "journalline")
            return await AddJournalLine();

        ErrorMessage = "Invalid form submission.";
        await OnGetAsync();
        return Page();
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Error processing form: {ex.Message}";
        await OnGetAsync();
        return Page();
    }
}

private void CalculateMetrics()
{
    TotalRevenue = Invoices.Where(i => i.Status != "Overdue").Sum(i => i.Amount);
    TotalExpenses = Accounts.Where(a => a.AccountType == "Expense").Sum(a => a.Balance);
    NetBalance = TotalRevenue - TotalExpenses;
    TotalInvoices = Invoices.Count;
    PaidInvoices = Invoices.Count(i => i.Status == "Paid");
    PendingInvoices = Invoices.Count(i => i.Status == "Pending");
}
```

---

### Bullet Point 3: Include basic validation for required fields and positive numeric amounts

**Summary:**  
Each add method performs validation before persisting to the database. This includes checking for required fields, ensuring positive numeric values, preventing duplicate keys, and enforcing business rules (e.g., debit and credit accounts must differ).

**Code Proof:**
```csharp
// Location: Pages/FinancialManagement.cshtml.cs

private async Task<IActionResult> AddPayment()
{
    // Validate required fields and positive amount
    if (NewPaymentInvoiceId <= 0 || string.IsNullOrWhiteSpace(NewPaymentNumber) || NewPaymentAmount <= 0)
    {
        ErrorMessage = "Invoice, Payment Number, and Payment amount are required.";
        await OnGetAsync();
        return Page();
    }

    // Validate invoice exists
    var invoice = await _context.Invoices.FindAsync(NewPaymentInvoiceId);
    if (invoice == null)
    {
        ErrorMessage = "Selected invoice not found.";
        await OnGetAsync();
        return Page();
    }

    // Check for duplicate payment number
    var dupPayment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentNumber == NewPaymentNumber);
    if (dupPayment != null)
    {
        ErrorMessage = $"Payment number '{NewPaymentNumber}' already exists.";
        await OnGetAsync();
        return Page();
    }

    // Persist to database
    var newPayment = new Payment
    {
        PaymentNumber = NewPaymentNumber,
        InvoiceId = NewPaymentInvoiceId,
        PaymentAmount = NewPaymentAmount,
        PaymentDate = NewPaymentDate,
        PaymentMethod = NewPaymentMethod,
        CreatedAt = DateTime.UtcNow
    };

    _context.Payments.Add(newPayment);
    await _context.SaveChangesAsync();

    Message = $"Payment ({NewPaymentAmount:C}) has been successfully added!";
    ResetAllForms();
    await OnGetAsync();
    return Page();
}

private async Task<IActionResult> AddJournalEntry()
{
    // Validate required fields and positive amount
    if (string.IsNullOrWhiteSpace(NewJournalNumber) || NewJournalDebitAccountId <= 0 || 
        NewJournalCreditAccountId <= 0 || NewJournalAmount <= 0)
    {
        ErrorMessage = "Journal Number, Debit Account, Credit Account, and Amount are required.";
        await OnGetAsync();
        return Page();
    }

    // Business rule: debit and credit accounts must be different
    if (NewJournalDebitAccountId == NewJournalCreditAccountId)
    {
        ErrorMessage = "Debit and Credit accounts must be different.";
        await OnGetAsync();
        return Page();
    }

    // Validate accounts exist
    var debitAccount = await _context.Accounts.FindAsync(NewJournalDebitAccountId);
    var creditAccount = await _context.Accounts.FindAsync(NewJournalCreditAccountId);
    if (debitAccount == null || creditAccount == null)
    {
        ErrorMessage = "One or both selected accounts not found.";
        await OnGetAsync();
        return Page();
    }

    // Check for duplicate journal number
    var jeExists = await _context.JournalEntries.FirstOrDefaultAsync(j => j.JournalNumber == NewJournalNumber);
    if (jeExists != null)
    {
        ErrorMessage = $"Journal number '{NewJournalNumber}' already exists.";
        await OnGetAsync();
        return Page();
    }

    // Persist to database
    var newJournalEntry = new JournalEntry
    {
        JournalNumber = NewJournalNumber,
        DebitAccountId = NewJournalDebitAccountId,
        CreditAccountId = NewJournalCreditAccountId,
        Amount = NewJournalAmount,
        Description = NewJournalDescription ?? string.Empty,
        EntryDate = NewJournalDate,
        CreatedAt = DateTime.UtcNow
    };

    _context.JournalEntries.Add(newJournalEntry);
    await _context.SaveChangesAsync();

    Message = $"Journal Entry ({NewJournalAmount:C}) has been successfully added!";
    ResetAllForms();
    await OnGetAsync();
    return Page();
}

private async Task<IActionResult> AddInvoiceLine()
{
    // Validate required fields and positive numeric values
    if (NewInvoiceLineInvoiceId <= 0 || string.IsNullOrWhiteSpace(NewInvoiceLineDescription) || 
        NewInvoiceLineQuantity <= 0 || NewInvoiceLineUnitPrice < 0)
    {
        ErrorMessage = "Invoice, Description, Quantity and Unit Price are required.";
        await OnGetAsync();
        return Page();
    }

    var invoice = await _context.Invoices.FindAsync(NewInvoiceLineInvoiceId);
    if (invoice == null)
    {
        ErrorMessage = "Selected invoice not found.";
        await OnGetAsync();
        return Page();
    }

    var line = new InvoiceLine
    {
        InvoiceId = NewInvoiceLineInvoiceId,
        Description = NewInvoiceLineDescription,
        Quantity = NewInvoiceLineQuantity,
        UnitPrice = NewInvoiceLineUnitPrice,
        LineTotal = NewInvoiceLineQuantity * NewInvoiceLineUnitPrice,
        CreatedAt = DateTime.UtcNow
    };

    _context.InvoiceLines.Add(line);
    await _context.SaveChangesAsync();

    Message = "Invoice line added.";
    ResetAllForms();
    await OnGetAsync();
    return Page();
}
```

---

### Bullet Point 4: After submission, clear the form and refresh the list with the new entry immediately visible

**Summary:**  
After successful submission, each add method calls `ResetAllForms()` to clear all input fields and then invokes `OnGetAsync()` to reload all lists from the database. This ensures the newly added record appears immediately in the appropriate table without requiring a page refresh.

**Code Proof:**
```csharp
// Location: Pages/FinancialManagement.cshtml.cs

private async Task<IActionResult> AddAccount()
{
    // ... validation logic ...

    var newAccount = new Account
    {
        AccountNumber = NewAccountNumber,
        AccountName = NewAccountName,
        AccountType = NewAccountType,
        Balance = NewAccountBalance,
        CreatedAt = DateTime.UtcNow
    };

    _context.Accounts.Add(newAccount);
    await _context.SaveChangesAsync();

    Message = $"Account '{NewAccountName}' ({NewAccountNumber}) has been successfully added!";
    
    // Clear form fields
    ResetAllForms();
    
    // Reload all data so new account appears in table
    await OnGetAsync();
    
    return Page();
}

private void ResetAllForms()
{
    // Clear all account form fields
    NewAccountNumber = string.Empty;
    NewAccountName = string.Empty;
    NewAccountType = string.Empty;
    NewAccountBalance = 0;

    // Clear all partner form fields
    NewPartnerName = string.Empty;
    NewPartnerType = string.Empty;
    NewPartnerEmail = string.Empty;
    NewPartnerPhone = string.Empty;
    NewPartnerContact = string.Empty;

    // Clear all invoice form fields
    NewInvoicePartnerId = 0;
    NewInvoiceNumber = string.Empty;
    NewInvoiceAmount = 0;
    NewInvoiceDate = DateTime.Today;
    NewInvoiceDueDate = DateTime.Today.AddDays(30);
    NewInvoiceStatus = "Pending";

    // Clear all payment form fields
    NewPaymentInvoiceId = 0;
    NewPaymentNumber = string.Empty;
    NewPaymentAmount = 0;
    NewPaymentDate = DateTime.Today;
    NewPaymentMethod = "Bank Transfer";

    // Clear all journal entry form fields
    NewJournalNumber = string.Empty;
    NewJournalDebitAccountId = 0;
    NewJournalCreditAccountId = 0;
    NewJournalAmount = 0;
    NewJournalDescription = string.Empty;
    NewJournalDate = DateTime.Today;

    // Clear all tax rate form fields
    NewTaxCode = string.Empty;
    NewTaxPercentage = 0;
    NewTaxDescription = string.Empty;
    NewTaxType = string.Empty;
    NewTaxEffectiveDate = DateTime.Today;

    // Clear all invoice line form fields
    NewInvoiceLineInvoiceId = 0;
    NewInvoiceLineDescription = string.Empty;
    NewInvoiceLineQuantity = 0;
    NewInvoiceLineUnitPrice = 0;

    // Clear all journal line form fields
    NewJournalLineEntryId = 0;
    NewJournalLineAccountId = 0;
    NewJournalLineDebit = 0;
    NewJournalLineCredit = 0;
    NewJournalLineDescription = string.Empty;

    // Clear form type to prevent stale handler routing
    FormType = string.Empty;
}
```

Result: User submits form → validation passes → record saved → form cleared → page reloaded with new record visible in table.

---

### Bullet Point 5: Keep the selected filter values persistent after each form submission

**Summary:**  
Filter persistence is implemented using `[BindProperty(SupportsGet = true)]` on filter properties. This approach is demonstrated in the HRM and Vendor Management pages. When a filter is applied and a form is submitted, the filter value is retained as a query string parameter, keeping the filtered view consistent across submissions.

**Code Proof (from HRM page as reference implementation):**
```csharp
// Location: Pages/HRM.cshtml.cs (pattern applicable to Financial page)

public class HRMModel : PageModel
{
    // Filter property that persists via query string
    [BindProperty(SupportsGet = true)]
    public string? DepartmentFilter { get; set; }

    public List<string> AllDepartments { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Load all distinct departments for filter dropdown
        AllDepartments = await _context.Employees
            .Select(e => e.Department)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
        AllDepartments.Insert(0, "All");

        // Apply filter if selected
        var employeesQuery = _context.Employees.AsQueryable();
        if (!string.IsNullOrWhiteSpace(DepartmentFilter) && DepartmentFilter != "All")
        {
            employeesQuery = employeesQuery.Where(e => e.Department == DepartmentFilter);
        }

        Employees = await employeesQuery.OrderBy(e => e.Name).ToListAsync();
        CalculateMetrics();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        // ... add employee logic ...
        await _context.SaveChangesAsync();
        
        // Redirect to GET with filter preserved in query string
        return RedirectToPage(new { DepartmentFilter });
    }
}
```

In the Razor page:
```html
<!-- Location: Pages/HRM.cshtml -->
<form method="post" asp-page-handler="Search">
    <div class="row">
        <div class="col-md-6">
            <label class="form-label">Department</label>
            <select class="form-select" asp-for="DepartmentFilter" onchange="this.form.submit()">
                @foreach (var dept in Model.AllDepartments)
                {
                    <option value="@dept" selected="@(Model.DepartmentFilter == dept)">@dept</option>
                }
            </select>
        </div>
    </div>
</form>
```

Because `DepartmentFilter` is marked with `SupportsGet = true`, it automatically persists in the URL (e.g., `/HRM?DepartmentFilter=Engineering`). When a form is submitted, the `RedirectToPage(new { DepartmentFilter })` call preserves the filter value, maintaining the user's filtered view.

**Implementation in Financial Page:**
The Financial Management page now has filter properties for Account Type, Partner Type, Invoice Status, Payment Method, and Tax Type using `[BindProperty(SupportsGet = true)]` pattern with dropdown menus in each table's toolbar.

```csharp
// Location: Pages/FinancialManagement.cshtml.cs
// Filter Properties (persisted via query string)
[BindProperty(SupportsGet = true)]
public string? AccountTypeFilter { get; set; }

[BindProperty(SupportsGet = true)]
public string? PartnerTypeFilter { get; set; }

[BindProperty(SupportsGet = true)]
public string? InvoiceStatusFilter { get; set; }

[BindProperty(SupportsGet = true)]
public string? PaymentMethodFilter { get; set; }

[BindProperty(SupportsGet = true)]
public string? TaxTypeFilter { get; set; }

// Lists for filter dropdowns
public List<string> AllAccountTypes { get; set; } = new();
public List<string> AllPartnerTypes { get; set; } = new();
public List<string> AllInvoiceStatuses { get; set; } = new();
public List<string> AllPaymentMethods { get; set; } = new();
public List<string> AllTaxTypes { get; set; } = new();

public async Task OnGetAsync()
{
    // Load distinct values for filter dropdowns
    AllAccountTypes = await _context.Accounts
        .Select(a => a.AccountType)
        .Distinct()
        .OrderBy(t => t)
        .ToListAsync();
    AllAccountTypes.Insert(0, "All");

    // Apply filters to Accounts
    var accountsQuery = _context.Accounts.AsQueryable();
    if (!string.IsNullOrWhiteSpace(AccountTypeFilter) && AccountTypeFilter != "All")
    {
        accountsQuery = accountsQuery.Where(a => a.AccountType == AccountTypeFilter);
    }
    Accounts = await accountsQuery.OrderBy(a => a.AccountNumber).ToListAsync();
    
    // Similar pattern applied for Partners, Invoices, Payments, TaxRates...
}
```

In the Razor page, each table now has a filter toolbar:
```html
<!-- Location: Pages/FinancialManagement.cshtml -->
<div class="card-body">
    <!-- Filter Toolbar -->
    <form method="get" class="mb-3">
        <div class="row align-items-end">
            <div class="col-md-3">
                <label class="form-label">Filter by Account Type</label>
                <select class="form-select" asp-for="AccountTypeFilter" onchange="this.form.submit()">
                    @foreach (var type in Model.AllAccountTypes)
                    {
                        <option value="@type" selected="@(Model.AccountTypeFilter == type)">@type</option>
                    }
                </select>
            </div>
            @if (!string.IsNullOrWhiteSpace(Model.AccountTypeFilter) && Model.AccountTypeFilter != "All")
            {
                <div class="col-md-2">
                    <a href="/FinancialManagement" class="btn btn-secondary">Clear Filter</a>
                </div>
            }
        </div>
    </form>
    <div class="table-responsive">
        <table class="table table-striped table-hover">
            <!-- Table content -->
        </table>
    </div>
</div>
```

This implementation ensures that when a user selects a filter category from the dropdown, the page automatically submits and reloads with the filtered data, and the filter selection persists in the URL query string (e.g., `/FinancialManagement?AccountTypeFilter=Expense`). When form submissions occur, the filter values are preserved because they are part of the query string.