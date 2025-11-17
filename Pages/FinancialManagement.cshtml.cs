using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages
{
    public class FinancialManagementModel : PageModel
    {
        private readonly AppDbContents _context;

        public FinancialManagementModel(AppDbContents context)
        {
            _context = context;
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

        private void CalculateMetrics()
        {
            // Calculate total revenue (invoices with Paid or Pending status)
            TotalRevenue = Invoices
                .Where(i => i.Status != "Overdue")
                .Sum(i => i.Amount);

            // Calculate total expenses (from journal entries on the expense side)
            TotalExpenses = Accounts
                .Where(a => a.AccountType == "Expense")
                .Sum(a => a.Balance);

            // Calculate net balance
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
                // Determine which form was submitted
                if (FormType == "account")
                {
                    return await AddAccount();
                }
                else if (FormType == "partner")
                {
                    return await AddPartner();
                }
                else if (FormType == "invoice")
                {
                    return await AddInvoice();
                }
                else if (FormType == "openbalance")
                {
                    return await AddOpenBalance();
                }
                else if (FormType == "payment")
                {
                    return await AddPayment();
                }
                else if (FormType == "journalentry")
                {
                    return await AddJournalEntry();
                }
                else if (FormType == "taxrate")
                {
                    return await AddTaxRate();
                }
                else if (FormType == "invoiceline")
                {
                    return await AddInvoiceLine();
                }
                else if (FormType == "journalline")
                {
                    return await AddJournalLine();
                }

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

            Message = $"Account '{NewAccountName}' ({NewAccountNumber}) has been successfully added!";
            ResetAllForms();
            await OnGetAsync();
            return Page();
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

            Message = $"Partner '{NewPartnerName}' has been successfully added!";
            ResetAllForms();
            await OnGetAsync();
            return Page();
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

            Message = $"Invoice '{NewInvoiceNumber}' ({NewInvoiceAmount:C}) has been successfully added!";
            ResetAllForms();
            await OnGetAsync();
            return Page();
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

            Message = $"Open Balance ({NewOpenBalanceAmount:C}) for '{account.AccountName}' has been successfully added!";
            ResetAllForms();
            await OnGetAsync();
            return Page();
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

            Message = $"Payment ({NewPaymentAmount:C}) has been successfully added!";
            ResetAllForms();
            await OnGetAsync();
            return Page();
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

            Message = $"Journal Entry ({NewJournalAmount:C}) has been successfully added!";
            ResetAllForms();
            await OnGetAsync();
            return Page();
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

            Message = $"Tax Rate '{NewTaxCode}' ({NewTaxPercentage}%) has been successfully added!";
            ResetAllForms();
            await OnGetAsync();
            return Page();
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

            Message = "Invoice line added.";
            ResetAllForms();
            await OnGetAsync();
            return Page();
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

            Message = "Journal line added.";
            ResetAllForms();
            await OnGetAsync();
            return Page();
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