using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages
{
    public class FinancialManagementModel : PageModel
    {
        private readonly AppDbContents _context;
        private readonly ILogger<FinancialManagementModel> _logger;

        public FinancialManagementModel(AppDbContents context, ILogger<FinancialManagementModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Data Collections
        public List<Account> Accounts { get; set; } = new();
        public List<Partner> Partners { get; set; } = new();
        public List<Invoice> Invoices { get; set; } = new();
        public List<OpenBalance> OpenBalances { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public List<JournalEntry> JournalEntries { get; set; } = new();
        public List<TaxRate> TaxRates { get; set; } = new();
    public List<InvoiceLine> InvoiceLines { get; set; } = new();
    public List<JournalLine> JournalLines { get; set; } = new();

        // Aggregate Metrics
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetBalance { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }

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

    // Active category selector for dashboard view switching
    [BindProperty(SupportsGet = true)]
    public string? ActiveCategory { get; set; } = "Accounts";

        // Edit form properties (Accounts)
        [BindProperty] public int EditAccountId { get; set; }
        [BindProperty] public string EditAccountNumber { get; set; } = string.Empty;
        [BindProperty] public string EditAccountName { get; set; } = string.Empty;
        [BindProperty] public string EditAccountType { get; set; } = string.Empty;
        [BindProperty] public decimal EditAccountBalance { get; set; }

        // Edit form properties (Partners)
        [BindProperty] public int EditPartnerId { get; set; }
        [BindProperty] public string EditPartnerName { get; set; } = string.Empty;
        [BindProperty] public string EditPartnerType { get; set; } = string.Empty;
        [BindProperty] public string EditPartnerEmail { get; set; } = string.Empty;
        [BindProperty] public string EditPartnerPhone { get; set; } = string.Empty;

    // Edit form properties (Invoices)
    [BindProperty] public int EditInvoiceId { get; set; }
    [BindProperty] public int EditInvoicePartnerId { get; set; }
    [BindProperty] public string EditInvoiceNumber { get; set; } = string.Empty;
    [BindProperty] public decimal EditInvoiceAmount { get; set; }
    [BindProperty] public string EditInvoiceStatus { get; set; } = string.Empty;
    [BindProperty] public DateTime EditInvoiceDate { get; set; } = DateTime.Today;
    [BindProperty] public DateTime EditInvoiceDueDate { get; set; } = DateTime.Today.AddDays(30);

    // Edit form properties (Open Balances)
    [BindProperty] public int EditOpenBalanceId { get; set; }
    [BindProperty] public int EditOpenBalanceAccountId { get; set; }
    [BindProperty] public decimal EditOpenBalanceAmount { get; set; }
    [BindProperty] public DateTime EditOpenBalanceDate { get; set; } = DateTime.Today;
    [BindProperty] public string EditOpenBalanceDescription { get; set; } = string.Empty;

    // Edit form properties (Payments)
    [BindProperty] public int EditPaymentId { get; set; }
    [BindProperty] public int EditPaymentInvoiceId { get; set; }
    [BindProperty] public string EditPaymentNumber { get; set; } = string.Empty;
    [BindProperty] public decimal EditPaymentAmount { get; set; }
    [BindProperty] public DateTime EditPaymentDate { get; set; } = DateTime.Today;
    [BindProperty] public string EditPaymentMethod { get; set; } = string.Empty;

    // Edit form properties (Journal Entries)
    [BindProperty] public int EditJournalEntryId { get; set; }
    [BindProperty] public string EditJournalNumber { get; set; } = string.Empty;
    [BindProperty] public int EditJournalDebitAccountId { get; set; }
    [BindProperty] public int EditJournalCreditAccountId { get; set; }
    [BindProperty] public decimal EditJournalAmount { get; set; }
    [BindProperty] public DateTime EditJournalDate { get; set; } = DateTime.Today;
    [BindProperty] public string EditJournalDescription { get; set; } = string.Empty;

    // Edit form properties (Invoice Lines)
    [BindProperty] public int EditInvoiceLineId { get; set; }
    [BindProperty] public int EditInvoiceLineInvoiceId { get; set; }
    [BindProperty] public string EditInvoiceLineDescription { get; set; } = string.Empty;
    [BindProperty] public decimal EditInvoiceLineQuantity { get; set; }
    [BindProperty] public decimal EditInvoiceLineUnitPrice { get; set; }

    // Edit form properties (Journal Lines)
    [BindProperty] public int EditJournalLineId { get; set; }
    [BindProperty] public int EditJournalLineEntryId { get; set; }
    [BindProperty] public int EditJournalLineAccountId { get; set; }
    [BindProperty] public decimal EditJournalLineDebit { get; set; }
    [BindProperty] public decimal EditJournalLineCredit { get; set; }
    [BindProperty] public string EditJournalLineDescription { get; set; } = string.Empty;

    // Edit form properties (Tax Rates)
    [BindProperty] public int EditTaxRateId { get; set; }
    [BindProperty] public string EditTaxCode { get; set; } = string.Empty;
    [BindProperty] public string EditTaxDescription { get; set; } = string.Empty;
    [BindProperty] public decimal EditTaxPercentage { get; set; }
    [BindProperty] public string EditTaxType { get; set; } = string.Empty;
    [BindProperty] public DateTime EditTaxEffectiveDate { get; set; } = DateTime.Today;

        // Form Properties for Add Account
        [BindProperty]
        public string NewAccountNumber { get; set; } = string.Empty;

        [BindProperty]
        public string NewAccountName { get; set; } = string.Empty;

        [BindProperty]
        public string NewAccountType { get; set; } = string.Empty;

        [BindProperty]
        public decimal NewAccountBalance { get; set; }

        // Form Properties for Add Partner
        [BindProperty]
        public string NewPartnerName { get; set; } = string.Empty;

        [BindProperty]
        public string NewPartnerType { get; set; } = string.Empty;

    // Separate fields to capture all Partner columns
    [BindProperty]
    public string NewPartnerEmail { get; set; } = string.Empty;

    [BindProperty]
    public string NewPartnerPhone { get; set; } = string.Empty;

        [BindProperty]
        public string NewPartnerContact { get; set; } = string.Empty;

        // Form Properties for Add Invoice
        [BindProperty]
        public int NewInvoicePartnerId { get; set; }

        [BindProperty]
        public string NewInvoiceNumber { get; set; } = string.Empty;

        [BindProperty]
        public decimal NewInvoiceAmount { get; set; }

        [BindProperty]
        public DateTime NewInvoiceDate { get; set; } = DateTime.Today;

    [BindProperty]
    public DateTime NewInvoiceDueDate { get; set; } = DateTime.Today.AddDays(30);

        [BindProperty]
        public string NewInvoiceStatus { get; set; } = "Pending";

        // Form Properties for Add Open Balance
        [BindProperty]
        public int NewOpenBalanceAccountId { get; set; }

        [BindProperty]
        public decimal NewOpenBalanceAmount { get; set; }

        [BindProperty]
        public DateTime NewOpenBalanceDate { get; set; } = DateTime.Today;

    [BindProperty]
    public string NewOpenBalanceDescription { get; set; } = string.Empty;

        // Form Properties for Add Payment
        [BindProperty]
        public int NewPaymentInvoiceId { get; set; }

    [BindProperty]
    public string NewPaymentNumber { get; set; } = string.Empty;

        [BindProperty]
        public decimal NewPaymentAmount { get; set; }

        [BindProperty]
        public DateTime NewPaymentDate { get; set; } = DateTime.Today;

        [BindProperty]
        public string NewPaymentMethod { get; set; } = "Bank Transfer";

        // Form Properties for Add Journal Entry
        [BindProperty]
    public string NewJournalNumber { get; set; } = string.Empty;

    [BindProperty]
        public int NewJournalDebitAccountId { get; set; }

        [BindProperty]
        public int NewJournalCreditAccountId { get; set; }

        [BindProperty]
        public decimal NewJournalAmount { get; set; }

        [BindProperty]
        public string NewJournalDescription { get; set; } = string.Empty;

        [BindProperty]
        public DateTime NewJournalDate { get; set; } = DateTime.Today;

        // Form Properties for Add Tax Rate
        [BindProperty]
        public string NewTaxCode { get; set; } = string.Empty;

        [BindProperty]
        public decimal NewTaxPercentage { get; set; }

        [BindProperty]
        public string NewTaxDescription { get; set; } = string.Empty;

    [BindProperty]
    public string NewTaxType { get; set; } = string.Empty;

    [BindProperty]
    public DateTime NewTaxEffectiveDate { get; set; } = DateTime.Today;

    // Form Properties for Add Invoice Line
    [BindProperty]
    public int NewInvoiceLineInvoiceId { get; set; }

    [BindProperty]
    public string NewInvoiceLineDescription { get; set; } = string.Empty;

    [BindProperty]
    public decimal NewInvoiceLineQuantity { get; set; }

    [BindProperty]
    public decimal NewInvoiceLineUnitPrice { get; set; }

    // Form Properties for Add Journal Line
    [BindProperty]
    public int NewJournalLineEntryId { get; set; }

    [BindProperty]
    public int NewJournalLineAccountId { get; set; }

    [BindProperty]
    public decimal NewJournalLineDebit { get; set; }

    [BindProperty]
    public decimal NewJournalLineCredit { get; set; }

    [BindProperty]
    public string NewJournalLineDescription { get; set; } = string.Empty;

        [BindProperty]
        public string FormType { get; set; } = string.Empty;

        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        // Helper method to check if user is allowed to access Financial Management
        private bool IsFinancialAllowed()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole)) return false;
            
            return userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || userRole.Equals("Guest", StringComparison.OrdinalIgnoreCase)
                || userRole.Equals("Accountant", StringComparison.OrdinalIgnoreCase);
        }

        public async Task OnGetAsync()
        {
            if (!IsFinancialAllowed())
            {
                TempData["ErrorMessage"] = "You do not have permission to access the Financial Management page.";
                RedirectToPage("/Privacy");
                return;
            }

            try
            {
                // Load distinct values for filter dropdowns
                AllAccountTypes = await _context.Accounts
                    .Select(a => a.AccountType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();
                AllAccountTypes.Insert(0, "All");

                AllPartnerTypes = await _context.Partners
                    .Select(p => p.PartnerType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();
                AllPartnerTypes.Insert(0, "All");

                AllInvoiceStatuses = await _context.Invoices
                    .Select(i => i.Status)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToListAsync();
                AllInvoiceStatuses.Insert(0, "All");

                AllPaymentMethods = await _context.Payments
                    .Select(p => p.PaymentMethod)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();
                AllPaymentMethods.Insert(0, "All");

                AllTaxTypes = await _context.TaxRates
                    .Select(t => t.TaxType)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();
                AllTaxTypes.Insert(0, "All");

                // Apply filters to Accounts
                var accountsQuery = _context.Accounts.AsQueryable();
                if (!string.IsNullOrWhiteSpace(AccountTypeFilter) && AccountTypeFilter != "All")
                {
                    accountsQuery = accountsQuery.Where(a => a.AccountType == AccountTypeFilter);
                }
                Accounts = await accountsQuery.OrderBy(a => a.AccountNumber).ToListAsync();

                // Apply filters to Partners
                var partnersQuery = _context.Partners.AsQueryable();
                if (!string.IsNullOrWhiteSpace(PartnerTypeFilter) && PartnerTypeFilter != "All")
                {
                    partnersQuery = partnersQuery.Where(p => p.PartnerType == PartnerTypeFilter);
                }
                Partners = await partnersQuery.OrderBy(p => p.PartnerName).ToListAsync();

                // Apply filters to Invoices
                var invoicesQuery = _context.Invoices.Include(i => i.Partner).AsQueryable();
                if (!string.IsNullOrWhiteSpace(InvoiceStatusFilter) && InvoiceStatusFilter != "All")
                {
                    invoicesQuery = invoicesQuery.Where(i => i.Status == InvoiceStatusFilter);
                }
                Invoices = await invoicesQuery.OrderByDescending(i => i.InvoiceDate).ToListAsync();

                // Apply filters to Payments
                var paymentsQuery = _context.Payments.Include(p => p.Invoice).AsQueryable();
                if (!string.IsNullOrWhiteSpace(PaymentMethodFilter) && PaymentMethodFilter != "All")
                {
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentMethod == PaymentMethodFilter);
                }
                Payments = await paymentsQuery.OrderByDescending(p => p.PaymentDate).ToListAsync();

                // Apply filters to Tax Rates
                var taxRatesQuery = _context.TaxRates.AsQueryable();
                if (!string.IsNullOrWhiteSpace(TaxTypeFilter) && TaxTypeFilter != "All")
                {
                    taxRatesQuery = taxRatesQuery.Where(t => t.TaxType == TaxTypeFilter);
                }
                TaxRates = await taxRatesQuery.OrderBy(t => t.TaxCode).ToListAsync();

                // Load remaining entities without filters
                OpenBalances = await _context.OpenBalances
                    .Include(ob => ob.Account)
                    .OrderByDescending(ob => ob.BalanceDate)
                    .ToListAsync();
                JournalEntries = await _context.JournalEntries
                    .Include(je => je.DebitAccount)
                    .Include(je => je.CreditAccount)
                    .OrderByDescending(je => je.EntryDate)
                    .ToListAsync();
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

        private IActionResult RedirectWithCategory()
        {
            // Preserve which section should be open and keep any active filters
            var cat = NormalizeActiveCategory();
            return RedirectToPage(new
            {
                ActiveCategory = cat,
                AccountTypeFilter = AccountTypeFilter,
                PartnerTypeFilter = PartnerTypeFilter,
                InvoiceStatusFilter = InvoiceStatusFilter,
                PaymentMethodFilter = PaymentMethodFilter,
                TaxTypeFilter = TaxTypeFilter
            });
        }

        private string NormalizeActiveCategory()
        {
            // Use posted ActiveCategory when available; otherwise infer from FormType
            var cat = (ActiveCategory ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(cat))
            {
                cat = FormType?.ToLowerInvariant() switch
                {
                    "account" => "accounts",
                    "partner" => "partners",
                    "invoice" => "invoices",
                    "openbalance" => "openbalances",
                    "payment" => "payments",
                    "journalentry" => "journalentries",
                    "taxrate" => "taxrates",
                    "invoiceline" => "invoicelines",
                    "journalline" => "journallines",
                    _ => string.Empty
                };
            }

            // Normalize to the CSS class key e.g., "accounts"
            return cat.ToLowerInvariant();
        }

        private void CalculateMetrics()
        {
            // Calculate total revenue (invoices with Paid or Pending status)
            TotalRevenue = Invoices
                .Where(i => i.Status != "Overdue")
                .Sum(i => i.Amount);

            // Calculate total expenses (from accounts - expense balances are typically negative or need absolute value)
            // Since Expense account balances may be stored as negative, take absolute value
            TotalExpenses = Accounts
                .Where(a => a.AccountType == "Expense")
                .Sum(a => Math.Abs(a.Balance));

            // Calculate net balance (Revenue minus Expenses)
            NetBalance = TotalRevenue - TotalExpenses;

            // Calculate invoice statistics
            TotalInvoices = Invoices.Count;
            PaidInvoices = Invoices.Count(i => i.Status == "Paid");
            PendingInvoices = Invoices.Count(i => i.Status == "Pending");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Financial POST received FormType='{FormType}' ActiveCategory='{ActiveCategory}'", FormType, ActiveCategory);
                // Determine which form was submitted
                if (FormType == "account")
                {
                    _logger.LogDebug("Attempting to add Account: Number={Number} Name={Name} Type={Type}", NewAccountNumber, NewAccountName, NewAccountType);
                    return await AddAccount();
                }
                else if (FormType == "partner")
                {
                    _logger.LogDebug("Attempting to add Partner: Name={Name} Type={Type}", NewPartnerName, NewPartnerType);
                    return await AddPartner();
                }
                else if (FormType == "invoice")
                {
                    _logger.LogDebug("Attempting to add Invoice: Number={Number} PartnerId={PartnerId} Amount={Amount}", NewInvoiceNumber, NewInvoicePartnerId, NewInvoiceAmount);
                    return await AddInvoice();
                }
                else if (FormType == "openbalance")
                {
                    _logger.LogDebug("Attempting to add OpenBalance: AccountId={AccountId} Amount={Amount}", NewOpenBalanceAccountId, NewOpenBalanceAmount);
                    return await AddOpenBalance();
                }
                else if (FormType == "payment")
                {
                    _logger.LogDebug("Attempting to add Payment: Number={Number} InvoiceId={InvoiceId} Amount={Amount}", NewPaymentNumber, NewPaymentInvoiceId, NewPaymentAmount);
                    return await AddPayment();
                }
                else if (FormType == "journalentry")
                {
                    _logger.LogDebug("Attempting to add JournalEntry: Number={Number} Debit={Debit} Credit={Credit} Amount={Amount}", NewJournalNumber, NewJournalDebitAccountId, NewJournalCreditAccountId, NewJournalAmount);
                    return await AddJournalEntry();
                }
                else if (FormType == "taxrate")
                {
                    _logger.LogDebug("Attempting to add TaxRate: Code={Code} Percentage={Percentage}", NewTaxCode, NewTaxPercentage);
                    return await AddTaxRate();
                }
                else if (FormType == "invoiceline")
                {
                    _logger.LogDebug("Attempting to add InvoiceLine: InvoiceId={InvoiceId} Desc={Desc} Qty={Qty} UnitPrice={UnitPrice}", NewInvoiceLineInvoiceId, NewInvoiceLineDescription, NewInvoiceLineQuantity, NewInvoiceLineUnitPrice);
                    return await AddInvoiceLine();
                }
                else if (FormType == "journalline")
                {
                    _logger.LogDebug("Attempting to add JournalLine: EntryId={EntryId} AccountId={AccountId} Debit={Debit} Credit={Credit}", NewJournalLineEntryId, NewJournalLineAccountId, NewJournalLineDebit, NewJournalLineCredit);
                    return await AddJournalLine();
                }

                ErrorMessage = "Invalid form submission.";
                _logger.LogWarning("Invalid FormType '{FormType}' posted", FormType);
                await OnGetAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in FinancialManagement POST for FormType={FormType}", FormType);
                ErrorMessage = $"Error processing form: {ex.Message}";
                await OnGetAsync();
                return Page();
            }
        }

        private async Task<IActionResult> AddAccount()
        {
            if (string.IsNullOrWhiteSpace(NewAccountNumber) || 
                string.IsNullOrWhiteSpace(NewAccountName) || 
                string.IsNullOrWhiteSpace(NewAccountType))
            {
                ErrorMessage = "All account fields are required.";
                await OnGetAsync();
                return Page();
            }

            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == NewAccountNumber);

            if (existingAccount != null)
            {
                ErrorMessage = $"Account number '{NewAccountNumber}' already exists.";
                await OnGetAsync();
                return Page();
            }

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
            _logger.LogInformation("Added Account Id={Id} Number={Number}", newAccount.Id, newAccount.AccountNumber);

            Message = $"Account '{NewAccountName}' ({NewAccountNumber}) has been successfully added!";
            ResetAllForms();
            return RedirectWithCategory();
        }

        // Edit Account
        public async Task<IActionResult> OnPostEditAccountAsync()
        {
            ActiveCategory = "accounts"; // ensure accounts section remains open
            if (EditAccountId <= 0 || string.IsNullOrWhiteSpace(EditAccountNumber) || string.IsNullOrWhiteSpace(EditAccountName) || string.IsNullOrWhiteSpace(EditAccountType))
            {
                ErrorMessage = "All account fields are required for edit.";
                await OnGetAsync();
                return Page();
            }
            var acct = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == EditAccountId);
            if (acct == null)
            {
                ErrorMessage = "Account not found.";
                await OnGetAsync();
                return Page();
            }
            // Prevent duplicate account number if changed
            if (acct.AccountNumber != EditAccountNumber)
            {
                var dup = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == EditAccountNumber);
                if (dup != null)
                {
                    ErrorMessage = $"Account number '{EditAccountNumber}' already exists.";
                    await OnGetAsync();
                    return Page();
                }
            }
            acct.AccountNumber = EditAccountNumber.Trim();
            acct.AccountName = EditAccountName.Trim();
            acct.AccountType = EditAccountType.Trim();
            acct.Balance = EditAccountBalance;
            await _context.SaveChangesAsync();
            Message = "Account updated.";
            _logger.LogInformation("Edited Account Id={Id} Number={Number}", acct.Id, acct.AccountNumber);
            return RedirectWithCategory();
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync(int id)
        {
            ActiveCategory = "accounts";
            var acct = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id);
            if (acct != null)
            {
                _context.Accounts.Remove(acct);
                await _context.SaveChangesAsync();
                Message = "Account deleted.";
                _logger.LogInformation("Deleted Account Id={Id}", id);
            }
            else
            {
                ErrorMessage = "Account not found.";
            }
            return RedirectWithCategory();
        }

        private async Task<IActionResult> AddPartner()
        {
            if (string.IsNullOrWhiteSpace(NewPartnerName) || 
                string.IsNullOrWhiteSpace(NewPartnerType))
            {
                ErrorMessage = "Partner Name and Type are required.";
                await OnGetAsync();
                return Page();
            }

            var newPartner = new Partner
            {
                PartnerName = NewPartnerName,
                PartnerType = NewPartnerType,
                Email = string.IsNullOrWhiteSpace(NewPartnerEmail) ? string.Empty : NewPartnerEmail,
                Phone = string.IsNullOrWhiteSpace(NewPartnerPhone) ? string.Empty : NewPartnerPhone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Partners.Add(newPartner);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added Partner Id={Id} Name={Name}", newPartner.Id, newPartner.PartnerName);

            Message = $"Partner '{NewPartnerName}' has been successfully added!";
            ResetAllForms();
            return RedirectWithCategory();
        }

        // Edit Partner
        public async Task<IActionResult> OnPostEditPartnerAsync()
        {
            ActiveCategory = "partners";
            if (EditPartnerId <= 0 || string.IsNullOrWhiteSpace(EditPartnerName) || string.IsNullOrWhiteSpace(EditPartnerType))
            {
                ErrorMessage = "Partner Name and Type are required for edit.";
                await OnGetAsync();
                return Page();
            }
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == EditPartnerId);
            if (partner == null)
            {
                ErrorMessage = "Partner not found.";
                await OnGetAsync();
                return Page();
            }
            partner.PartnerName = EditPartnerName.Trim();
            partner.PartnerType = EditPartnerType.Trim();
            partner.Email = EditPartnerEmail?.Trim() ?? string.Empty;
            partner.Phone = EditPartnerPhone?.Trim() ?? string.Empty;
            await _context.SaveChangesAsync();
            Message = "Partner updated.";
            _logger.LogInformation("Edited Partner Id={Id} Name={Name}", partner.Id, partner.PartnerName);
            return RedirectWithCategory();
        }

        public async Task<IActionResult> OnPostDeletePartnerAsync(int id)
        {
            ActiveCategory = "partners";
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == id);
            if (partner != null)
            {
                _context.Partners.Remove(partner);
                await _context.SaveChangesAsync();
                Message = "Partner deleted.";
                _logger.LogInformation("Deleted Partner Id={Id}", id);
            }
            else
            {
                ErrorMessage = "Partner not found.";
            }
            return RedirectWithCategory();
        }

        // Edit Invoice
        public async Task<IActionResult> OnPostEditInvoiceAsync()
        {
            ActiveCategory = "invoices";
            if (EditInvoiceId <= 0 || EditInvoicePartnerId <= 0 || string.IsNullOrWhiteSpace(EditInvoiceNumber) || EditInvoiceAmount <= 0)
            {
                ErrorMessage = "Invoice Partner, Number, and Amount are required.";
                await OnGetAsync();
                return Page();
            }
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == EditInvoiceId);
            if (invoice == null)
            {
                ErrorMessage = "Invoice not found.";
                await OnGetAsync();
                return Page();
            }
            invoice.PartnerId = EditInvoicePartnerId;
            invoice.InvoiceNumber = EditInvoiceNumber.Trim();
            invoice.Amount = EditInvoiceAmount;
            invoice.Status = EditInvoiceStatus?.Trim() ?? invoice.Status;
            invoice.InvoiceDate = EditInvoiceDate;
            invoice.DueDate = EditInvoiceDueDate;
            await _context.SaveChangesAsync();
            Message = "Invoice updated.";
            _logger.LogInformation("Edited Invoice Id={Id} Number={Number}", invoice.Id, invoice.InvoiceNumber);
            return RedirectWithCategory();
        }
        public async Task<IActionResult> OnPostDeleteInvoiceAsync(int id)
        {
            ActiveCategory = "invoices";
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                Message = "Invoice deleted.";
                _logger.LogInformation("Deleted Invoice Id={Id}", id);
            }
            else
            {
                ErrorMessage = "Invoice not found.";
            }
            return RedirectWithCategory();
        }

        // Edit Open Balance
        public async Task<IActionResult> OnPostEditOpenBalanceAsync()
        {
            ActiveCategory = "openbalances";
            if (EditOpenBalanceId <= 0 || EditOpenBalanceAccountId <= 0)
            {
                ErrorMessage = "Account is required.";
                await OnGetAsync();
                return Page();
            }
            var ob = await _context.OpenBalances.FirstOrDefaultAsync(o => o.Id == EditOpenBalanceId);
            if (ob == null)
            {
                ErrorMessage = "Open Balance not found.";
                await OnGetAsync();
                return Page();
            }
            ob.AccountId = EditOpenBalanceAccountId;
            ob.OpeningBalance = EditOpenBalanceAmount;
            ob.BalanceDate = EditOpenBalanceDate;
            ob.Description = EditOpenBalanceDescription ?? string.Empty;
            await _context.SaveChangesAsync();
            Message = "Open Balance updated.";
            _logger.LogInformation("Edited OpenBalance Id={Id}", ob.Id);
            return RedirectWithCategory();
        }
        public async Task<IActionResult> OnPostDeleteOpenBalanceAsync(int id)
        {
            ActiveCategory = "openbalances";
            var ob = await _context.OpenBalances.FirstOrDefaultAsync(o => o.Id == id);
            if (ob != null)
            {
                _context.OpenBalances.Remove(ob);
                await _context.SaveChangesAsync();
                Message = "Open Balance deleted.";
                _logger.LogInformation("Deleted OpenBalance Id={Id}", id);
            }
            else
            {
                ErrorMessage = "Open Balance not found.";
            }
            return RedirectWithCategory();
        }

        // Edit Payment
        public async Task<IActionResult> OnPostEditPaymentAsync()
        {
            ActiveCategory = "payments";
            if (EditPaymentId <= 0 || EditPaymentInvoiceId <= 0 || string.IsNullOrWhiteSpace(EditPaymentNumber))
            {
                ErrorMessage = "Invoice and Payment Number are required.";
                await OnGetAsync();
                return Page();
            }
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == EditPaymentId);
            if (payment == null)
            {
                ErrorMessage = "Payment not found.";
                await OnGetAsync();
                return Page();
            }
            payment.InvoiceId = EditPaymentInvoiceId;
            payment.PaymentNumber = EditPaymentNumber.Trim();
            payment.PaymentAmount = EditPaymentAmount;
            payment.PaymentDate = EditPaymentDate;
            payment.PaymentMethod = EditPaymentMethod?.Trim() ?? payment.PaymentMethod;
            await _context.SaveChangesAsync();
            Message = "Payment updated.";
            _logger.LogInformation("Edited Payment Id={Id} Number={Number}", payment.Id, payment.PaymentNumber);
            return RedirectWithCategory();
        }
        public async Task<IActionResult> OnPostDeletePaymentAsync(int id)
        {
            ActiveCategory = "payments";
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
                Message = "Payment deleted.";
                _logger.LogInformation("Deleted Payment Id={Id}", id);
            }
            else
            {
                ErrorMessage = "Payment not found.";
            }
            return RedirectWithCategory();
        }

        // Edit Journal Entry
        public async Task<IActionResult> OnPostEditJournalEntryAsync()
        {
            ActiveCategory = "journalentries";
            if (EditJournalEntryId <= 0 || string.IsNullOrWhiteSpace(EditJournalNumber) || EditJournalDebitAccountId <= 0 || EditJournalCreditAccountId <= 0)
            {
                ErrorMessage = "Journal Number and accounts are required.";
                await OnGetAsync();
                return Page();
            }
            if (EditJournalDebitAccountId == EditJournalCreditAccountId)
            {
                ErrorMessage = "Debit and Credit accounts must differ.";
                await OnGetAsync();
                return Page();
            }
            var je = await _context.JournalEntries.FirstOrDefaultAsync(j => j.Id == EditJournalEntryId);
            if (je == null)
            {
                ErrorMessage = "Journal Entry not found.";
                await OnGetAsync();
                return Page();
            }
            je.JournalNumber = EditJournalNumber.Trim();
            je.DebitAccountId = EditJournalDebitAccountId;
            je.CreditAccountId = EditJournalCreditAccountId;
            je.Amount = EditJournalAmount;
            je.EntryDate = EditJournalDate;
            je.Description = EditJournalDescription ?? string.Empty;
            await _context.SaveChangesAsync();
            Message = "Journal Entry updated.";
            _logger.LogInformation("Edited JournalEntry Id={Id} Number={Number}", je.Id, je.JournalNumber);
            return RedirectWithCategory();
        }
        public async Task<IActionResult> OnPostDeleteJournalEntryAsync(int id)
        {
            ActiveCategory = "journalentries";
            var je = await _context.JournalEntries.FirstOrDefaultAsync(j => j.Id == id);
            if (je != null)
            {
                _context.JournalEntries.Remove(je);
                await _context.SaveChangesAsync();
                Message = "Journal Entry deleted.";
                _logger.LogInformation("Deleted JournalEntry Id={Id}", id);
            }
            else
            {
                ErrorMessage = "Journal Entry not found.";
            }
            return RedirectWithCategory();
        }

        // Edit Invoice Line
        public async Task<IActionResult> OnPostEditInvoiceLineAsync()
        {
            ActiveCategory = "invoicelines";
            if (EditInvoiceLineId <= 0 || EditInvoiceLineInvoiceId <= 0 || string.IsNullOrWhiteSpace(EditInvoiceLineDescription) || EditInvoiceLineQuantity <= 0 || EditInvoiceLineUnitPrice < 0)
            {
                ErrorMessage = "Invoice, Description, Quantity and Unit Price are required.";
                await OnGetAsync();
                return Page();
            }
            var line = await _context.InvoiceLines.FirstOrDefaultAsync(il => il.Id == EditInvoiceLineId);
            if (line == null)
            {
                ErrorMessage = "Invoice Line not found.";
                await OnGetAsync();
                return Page();
            }
            line.InvoiceId = EditInvoiceLineInvoiceId;
            line.Description = EditInvoiceLineDescription.Trim();
            line.Quantity = EditInvoiceLineQuantity;
            line.UnitPrice = EditInvoiceLineUnitPrice;
            line.LineTotal = EditInvoiceLineQuantity * EditInvoiceLineUnitPrice;
            await _context.SaveChangesAsync();
            Message = "Invoice Line updated.";
            _logger.LogInformation("Edited InvoiceLine Id={Id}", line.Id);
            return RedirectWithCategory();
        }
        public async Task<IActionResult> OnPostDeleteInvoiceLineAsync(int id)
        {
            ActiveCategory = "invoicelines";
            var line = await _context.InvoiceLines.FirstOrDefaultAsync(il => il.Id == id);
            if (line != null)
            {
                _context.InvoiceLines.Remove(line);
                await _context.SaveChangesAsync();
                Message = "Invoice Line deleted.";
                _logger.LogInformation("Deleted InvoiceLine Id={Id}", id);
            }
            else
            {
                ErrorMessage = "Invoice Line not found.";
            }
            return RedirectWithCategory();
        }

        // Edit Journal Line
        public async Task<IActionResult> OnPostEditJournalLineAsync()
        {
            ActiveCategory = "journallines";
            if (EditJournalLineId <= 0 || EditJournalLineEntryId <= 0 || EditJournalLineAccountId <= 0)
            {
                ErrorMessage = "Journal Entry and Account are required.";
                await OnGetAsync();
                return Page();
            }
            var jl = await _context.JournalLines.FirstOrDefaultAsync(j => j.Id == EditJournalLineId);
            if (jl == null)
            {
                ErrorMessage = "Journal Line not found.";
                await OnGetAsync();
                return Page();
            }
            jl.JournalEntryId = EditJournalLineEntryId;
            jl.AccountId = EditJournalLineAccountId;
            jl.Debit = EditJournalLineDebit;
            jl.Credit = EditJournalLineCredit;
            jl.Description = EditJournalLineDescription ?? string.Empty;
            await _context.SaveChangesAsync();
            Message = "Journal Line updated.";
            _logger.LogInformation("Edited JournalLine Id={Id}", jl.Id);
            return RedirectWithCategory();
        }
        public async Task<IActionResult> OnPostDeleteJournalLineAsync(int id)
        {
            ActiveCategory = "journallines";
            var jl = await _context.JournalLines.FirstOrDefaultAsync(j => j.Id == id);
            if (jl != null)
            {
                _context.JournalLines.Remove(jl);
                await _context.SaveChangesAsync();
                Message = "Journal Line deleted.";
                _logger.LogInformation("Deleted JournalLine Id={Id}", id);
            }
            else
            {
                ErrorMessage = "Journal Line not found.";
            }
            return RedirectWithCategory();
        }

        // Edit Tax Rate
        public async Task<IActionResult> OnPostEditTaxRateAsync()
        {
            ActiveCategory = "taxrates";
            if (EditTaxRateId <= 0 || string.IsNullOrWhiteSpace(EditTaxCode))
            {
                ErrorMessage = "Tax Code is required.";
                await OnGetAsync();
                return Page();
            }
            var tax = await _context.TaxRates.FirstOrDefaultAsync(t => t.Id == EditTaxRateId);
            if (tax == null)
            {
                ErrorMessage = "Tax Rate not found.";
                await OnGetAsync();
                return Page();
            }
            tax.TaxCode = EditTaxCode.Trim();
            tax.TaxDescription = EditTaxDescription?.Trim() ?? string.Empty;
            tax.Rate = EditTaxPercentage / 100m;
            tax.TaxType = EditTaxType?.Trim() ?? string.Empty;
            tax.EffectiveDate = EditTaxEffectiveDate;
            await _context.SaveChangesAsync();
            Message = "Tax Rate updated.";
            _logger.LogInformation("Edited TaxRate Id={Id} Code={Code}", tax.Id, tax.TaxCode);
            return RedirectWithCategory();
        }
        public async Task<IActionResult> OnPostDeleteTaxRateAsync(int id)
        {
            ActiveCategory = "taxrates";
            var tax = await _context.TaxRates.FirstOrDefaultAsync(t => t.Id == id);
            if (tax != null)
            {
                _context.TaxRates.Remove(tax);
                await _context.SaveChangesAsync();
                Message = "Tax Rate deleted.";
                _logger.LogInformation("Deleted TaxRate Id={Id}", id);
            }
            else
            {
                ErrorMessage = "Tax Rate not found.";
            }
            return RedirectWithCategory();
        }

        private async Task<IActionResult> AddInvoice()
        {
            if (NewInvoicePartnerId <= 0 || string.IsNullOrWhiteSpace(NewInvoiceNumber) || NewInvoiceAmount <= 0)
            {
                ErrorMessage = "Partner, Invoice Number, and Amount are required.";
                await OnGetAsync();
                return Page();
            }

            var partner = await _context.Partners.FindAsync(NewInvoicePartnerId);
            if (partner == null)
            {
                ErrorMessage = "Selected partner not found.";
                await OnGetAsync();
                return Page();
            }

            var newInvoice = new Invoice
            {
                PartnerId = NewInvoicePartnerId,
                InvoiceNumber = NewInvoiceNumber,
                Amount = NewInvoiceAmount,
                InvoiceDate = NewInvoiceDate,
                DueDate = NewInvoiceDueDate,
                Status = NewInvoiceStatus,
                CreatedAt = DateTime.UtcNow
            };

            _context.Invoices.Add(newInvoice);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added Invoice Id={Id} Number={Number}", newInvoice.Id, newInvoice.InvoiceNumber);

            Message = $"Invoice '{NewInvoiceNumber}' ({NewInvoiceAmount:C}) has been successfully added!";
            ResetAllForms();
            return RedirectWithCategory();
        }

        private async Task<IActionResult> AddOpenBalance()
        {
            if (NewOpenBalanceAccountId <= 0 || NewOpenBalanceAmount == 0)
            {
                ErrorMessage = "Account and Balance amount are required.";
                await OnGetAsync();
                return Page();
            }

            var account = await _context.Accounts.FindAsync(NewOpenBalanceAccountId);
            if (account == null)
            {
                ErrorMessage = "Selected account not found.";
                await OnGetAsync();
                return Page();
            }

            var newOpenBalance = new OpenBalance
            {
                AccountId = NewOpenBalanceAccountId,
                OpeningBalance = NewOpenBalanceAmount,
                BalanceDate = NewOpenBalanceDate,
                Description = NewOpenBalanceDescription ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.OpenBalances.Add(newOpenBalance);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added OpenBalance Id={Id} AccountId={AccountId}", newOpenBalance.Id, newOpenBalance.AccountId);

            Message = $"Open Balance ({NewOpenBalanceAmount:C}) for '{account.AccountName}' has been successfully added!";
            ResetAllForms();
            // Fallback: if ActiveCategory wasn't posted, ensure Open Balances stays open
            if (string.IsNullOrWhiteSpace(ActiveCategory))
            {
                ActiveCategory = "openbalances";
            }
            return RedirectWithCategory();
        }

        private async Task<IActionResult> AddPayment()
        {
            if (NewPaymentInvoiceId <= 0 || string.IsNullOrWhiteSpace(NewPaymentNumber) || NewPaymentAmount <= 0)
            {
                ErrorMessage = "Invoice, Payment Number, and Payment amount are required.";
                await OnGetAsync();
                return Page();
            }

            var invoice = await _context.Invoices.FindAsync(NewPaymentInvoiceId);
            if (invoice == null)
            {
                ErrorMessage = "Selected invoice not found.";
                await OnGetAsync();
                return Page();
            }

            var dupPayment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentNumber == NewPaymentNumber);
            if (dupPayment != null)
            {
                ErrorMessage = $"Payment number '{NewPaymentNumber}' already exists.";
                await OnGetAsync();
                return Page();
            }

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
            _logger.LogInformation("Added Payment Id={Id} Number={Number}", newPayment.Id, newPayment.PaymentNumber);

            Message = $"Payment ({NewPaymentAmount:C}) has been successfully added!";
            ResetAllForms();
            return RedirectWithCategory();
        }

        private async Task<IActionResult> AddJournalEntry()
        {
            if (string.IsNullOrWhiteSpace(NewJournalNumber) || NewJournalDebitAccountId <= 0 || NewJournalCreditAccountId <= 0 || NewJournalAmount <= 0)
            {
                ErrorMessage = "Journal Number, Debit Account, Credit Account, and Amount are required.";
                await OnGetAsync();
                return Page();
            }

            if (NewJournalDebitAccountId == NewJournalCreditAccountId)
            {
                ErrorMessage = "Debit and Credit accounts must be different.";
                await OnGetAsync();
                return Page();
            }

            var debitAccount = await _context.Accounts.FindAsync(NewJournalDebitAccountId);
            var creditAccount = await _context.Accounts.FindAsync(NewJournalCreditAccountId);

            if (debitAccount == null || creditAccount == null)
            {
                ErrorMessage = "One or both selected accounts not found.";
                await OnGetAsync();
                return Page();
            }

            var jeExists = await _context.JournalEntries.FirstOrDefaultAsync(j => j.JournalNumber == NewJournalNumber);
            if (jeExists != null)
            {
                ErrorMessage = $"Journal number '{NewJournalNumber}' already exists.";
                await OnGetAsync();
                return Page();
            }

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
            _logger.LogInformation("Added JournalEntry Id={Id} Number={Number}", newJournalEntry.Id, newJournalEntry.JournalNumber);

            Message = $"Journal Entry ({NewJournalAmount:C}) has been successfully added!";
            ResetAllForms();
            return RedirectWithCategory();
        }

        private async Task<IActionResult> AddTaxRate()
        {
            if (string.IsNullOrWhiteSpace(NewTaxCode) || NewTaxPercentage < 0 || NewTaxPercentage > 100)
            {
                ErrorMessage = "Tax Code is required and percentage must be between 0 and 100.";
                await OnGetAsync();
                return Page();
            }

            var existingTax = await _context.TaxRates
                .FirstOrDefaultAsync(t => t.TaxCode == NewTaxCode);

            if (existingTax != null)
            {
                ErrorMessage = $"Tax code '{NewTaxCode}' already exists.";
                await OnGetAsync();
                return Page();
            }

            var newTaxRate = new TaxRate
            {
                TaxCode = NewTaxCode,
                Rate = NewTaxPercentage / 100m,
                TaxDescription = NewTaxDescription ?? string.Empty,
                TaxType = NewTaxType ?? string.Empty,
                EffectiveDate = NewTaxEffectiveDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaxRates.Add(newTaxRate);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added TaxRate Id={Id} Code={Code}", newTaxRate.Id, newTaxRate.TaxCode);

            Message = $"Tax Rate '{NewTaxCode}' ({NewTaxPercentage}%) has been successfully added!";
            ResetAllForms();
            return RedirectWithCategory();
        }

        private async Task<IActionResult> AddInvoiceLine()
        {
            if (NewInvoiceLineInvoiceId <= 0 || string.IsNullOrWhiteSpace(NewInvoiceLineDescription) || NewInvoiceLineQuantity <= 0 || NewInvoiceLineUnitPrice < 0)
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
            _logger.LogInformation("Added InvoiceLine Id={Id} InvoiceId={InvoiceId}", line.Id, line.InvoiceId);

            Message = "Invoice line added.";
            ResetAllForms();
            return RedirectWithCategory();
        }

        private async Task<IActionResult> AddJournalLine()
        {
            if (NewJournalLineEntryId <= 0 || NewJournalLineAccountId <= 0)
            {
                ErrorMessage = "Journal Entry and Account are required.";
                await OnGetAsync();
                return Page();
            }

            if (NewJournalLineDebit < 0 || NewJournalLineCredit < 0)
            {
                ErrorMessage = "Debit and Credit must be non-negative.";
                await OnGetAsync();
                return Page();
            }

            var entry = await _context.JournalEntries.FindAsync(NewJournalLineEntryId);
            var account = await _context.Accounts.FindAsync(NewJournalLineAccountId);
            if (entry == null || account == null)
            {
                ErrorMessage = "Selected entry or account not found.";
                await OnGetAsync();
                return Page();
            }

            var jl = new JournalLine
            {
                JournalEntryId = NewJournalLineEntryId,
                AccountId = NewJournalLineAccountId,
                Debit = NewJournalLineDebit,
                Credit = NewJournalLineCredit,
                Description = NewJournalLineDescription ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.JournalLines.Add(jl);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added JournalLine Id={Id} EntryId={EntryId}", jl.Id, jl.JournalEntryId);

            Message = "Journal line added.";
            ResetAllForms();
            return RedirectWithCategory();
        }

        private void ResetAllForms()
        {
            NewAccountNumber = string.Empty;
            NewAccountName = string.Empty;
            NewAccountType = string.Empty;
            NewAccountBalance = 0;

            NewPartnerName = string.Empty;
            NewPartnerType = string.Empty;
            NewPartnerEmail = string.Empty;
            NewPartnerPhone = string.Empty;
            NewPartnerContact = string.Empty; // legacy field, unused

            NewInvoicePartnerId = 0;
            NewInvoiceNumber = string.Empty;
            NewInvoiceAmount = 0;
            NewInvoiceDate = DateTime.Today;
            NewInvoiceDueDate = DateTime.Today.AddDays(30);
            NewInvoiceStatus = "Pending";

            NewOpenBalanceAccountId = 0;
            NewOpenBalanceAmount = 0;
            NewOpenBalanceDate = DateTime.Today;
            NewOpenBalanceDescription = string.Empty;

            NewPaymentInvoiceId = 0;
            NewPaymentNumber = string.Empty;
            NewPaymentAmount = 0;
            NewPaymentDate = DateTime.Today;
            NewPaymentMethod = "Bank Transfer";

            NewJournalNumber = string.Empty;
            NewJournalDebitAccountId = 0;
            NewJournalCreditAccountId = 0;
            NewJournalAmount = 0;
            NewJournalDescription = string.Empty;
            NewJournalDate = DateTime.Today;

            NewTaxCode = string.Empty;
            NewTaxPercentage = 0;
            NewTaxDescription = string.Empty;
            NewTaxType = string.Empty;
            NewTaxEffectiveDate = DateTime.Today;

            NewInvoiceLineInvoiceId = 0;
            NewInvoiceLineDescription = string.Empty;
            NewInvoiceLineQuantity = 0;
            NewInvoiceLineUnitPrice = 0;

            NewJournalLineEntryId = 0;
            NewJournalLineAccountId = 0;
            NewJournalLineDebit = 0;
            NewJournalLineCredit = 0;
            NewJournalLineDescription = string.Empty;

            FormType = string.Empty;
        }
    }
}