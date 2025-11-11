using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Data
{
    public class AppDbContents : DbContext
    {
        public AppDbContents(DbContextOptions<AppDbContents> options) : base(options)
        {
        }

        public DbSet<UserCredentials> UserCredentials { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Vendor> Vendors { get; set; } = null!;
        public DbSet<VendorFile> VendorFiles { get; set; } = null!;
        
        // Financial Management
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Partner> Partners { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<OpenBalance> OpenBalances { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
        public DbSet<TaxRate> TaxRates { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Accounts
            modelBuilder.Entity<Account>().HasData(
                new Account { Id = 1, AccountNumber = "1000", AccountName = "Cash", AccountType = "Asset", Balance = 50000m, CreatedAt = DateTime.UtcNow },
                new Account { Id = 2, AccountNumber = "1100", AccountName = "Accounts Receivable", AccountType = "Asset", Balance = 25000m, CreatedAt = DateTime.UtcNow },
                new Account { Id = 3, AccountNumber = "1200", AccountName = "Inventory", AccountType = "Asset", Balance = 75000m, CreatedAt = DateTime.UtcNow },
                new Account { Id = 4, AccountNumber = "2000", AccountName = "Accounts Payable", AccountType = "Liability", Balance = -30000m, CreatedAt = DateTime.UtcNow },
                new Account { Id = 5, AccountNumber = "3000", AccountName = "Common Stock", AccountType = "Equity", Balance = 100000m, CreatedAt = DateTime.UtcNow },
                new Account { Id = 6, AccountNumber = "4000", AccountName = "Sales Revenue", AccountType = "Revenue", Balance = 150000m, CreatedAt = DateTime.UtcNow },
                new Account { Id = 7, AccountNumber = "5000", AccountName = "Cost of Goods Sold", AccountType = "Expense", Balance = -45000m, CreatedAt = DateTime.UtcNow }
            );

            // Seed Partners
            modelBuilder.Entity<Partner>().HasData(
                new Partner { Id = 1, PartnerName = "Tech Solutions Inc", PartnerType = "Vendor", Email = "info@techsolutions.com", Phone = "(555) 123-4567", CreatedAt = DateTime.UtcNow },
                new Partner { Id = 2, PartnerName = "Global Manufacturing Co", PartnerType = "Customer", Email = "sales@globalmfg.com", Phone = "(555) 234-5678", CreatedAt = DateTime.UtcNow },
                new Partner { Id = 3, PartnerName = "Premium Supplies Ltd", PartnerType = "Vendor", Email = "order@premium-supplies.com", Phone = "(555) 345-6789", CreatedAt = DateTime.UtcNow },
                new Partner { Id = 4, PartnerName = "Enterprise Solutions", PartnerType = "Customer", Email = "contact@enterprise.com", Phone = "(555) 456-7890", CreatedAt = DateTime.UtcNow },
                new Partner { Id = 5, PartnerName = "Innovation Partners", PartnerType = "Associate", Email = "hello@innovationpartners.com", Phone = "(555) 567-8901", CreatedAt = DateTime.UtcNow }
            );

            // Seed Invoices
            modelBuilder.Entity<Invoice>().HasData(
                new Invoice { Id = 1, InvoiceNumber = "INV-001", PartnerId = 2, Amount = 5000m, Status = "Paid", InvoiceDate = DateTime.UtcNow.AddDays(-30), DueDate = DateTime.UtcNow.AddDays(-5), CreatedAt = DateTime.UtcNow },
                new Invoice { Id = 2, InvoiceNumber = "INV-002", PartnerId = 4, Amount = 7500m, Status = "Pending", InvoiceDate = DateTime.UtcNow.AddDays(-15), DueDate = DateTime.UtcNow.AddDays(15), CreatedAt = DateTime.UtcNow },
                new Invoice { Id = 3, InvoiceNumber = "INV-003", PartnerId = 2, Amount = 3200m, Status = "Pending", InvoiceDate = DateTime.UtcNow.AddDays(-10), DueDate = DateTime.UtcNow.AddDays(20), CreatedAt = DateTime.UtcNow },
                new Invoice { Id = 4, InvoiceNumber = "INV-004", PartnerId = 4, Amount = 8900m, Status = "Paid", InvoiceDate = DateTime.UtcNow.AddDays(-45), DueDate = DateTime.UtcNow.AddDays(-20), CreatedAt = DateTime.UtcNow },
                new Invoice { Id = 5, InvoiceNumber = "INV-005", PartnerId = 2, Amount = 4500m, Status = "Overdue", InvoiceDate = DateTime.UtcNow.AddDays(-60), DueDate = DateTime.UtcNow.AddDays(-15), CreatedAt = DateTime.UtcNow }
            );

            // Seed Open Balances
            modelBuilder.Entity<OpenBalance>().HasData(
                new OpenBalance { Id = 1, AccountId = 1, OpeningBalance = 25000m, BalanceDate = DateTime.UtcNow.AddDays(-90), Description = "Opening Balance - Cash Account", CreatedAt = DateTime.UtcNow },
                new OpenBalance { Id = 2, AccountId = 2, OpeningBalance = 15000m, BalanceDate = DateTime.UtcNow.AddDays(-90), Description = "Opening Balance - Receivables", CreatedAt = DateTime.UtcNow },
                new OpenBalance { Id = 3, AccountId = 3, OpeningBalance = 60000m, BalanceDate = DateTime.UtcNow.AddDays(-90), Description = "Opening Balance - Inventory", CreatedAt = DateTime.UtcNow },
                new OpenBalance { Id = 4, AccountId = 4, OpeningBalance = -20000m, BalanceDate = DateTime.UtcNow.AddDays(-90), Description = "Opening Balance - Payables", CreatedAt = DateTime.UtcNow },
                new OpenBalance { Id = 5, AccountId = 5, OpeningBalance = 100000m, BalanceDate = DateTime.UtcNow.AddDays(-90), Description = "Opening Balance - Equity", CreatedAt = DateTime.UtcNow }
            );

            // Seed Payments
            modelBuilder.Entity<Payment>().HasData(
                new Payment { Id = 1, PaymentNumber = "PAY-001", InvoiceId = 1, PaymentAmount = 5000m, PaymentDate = DateTime.UtcNow.AddDays(-20), PaymentMethod = "Wire", CreatedAt = DateTime.UtcNow },
                new Payment { Id = 2, PaymentNumber = "PAY-002", InvoiceId = 4, PaymentAmount = 8900m, PaymentDate = DateTime.UtcNow.AddDays(-30), PaymentMethod = "Check", CreatedAt = DateTime.UtcNow },
                new Payment { Id = 3, PaymentNumber = "PAY-003", InvoiceId = 1, PaymentAmount = 5000m, PaymentDate = DateTime.UtcNow.AddDays(-5), PaymentMethod = "Credit Card", CreatedAt = DateTime.UtcNow },
                new Payment { Id = 4, PaymentNumber = "PAY-004", InvoiceId = 2, PaymentAmount = 3750m, PaymentDate = DateTime.UtcNow.AddDays(-2), PaymentMethod = "Bank Transfer", CreatedAt = DateTime.UtcNow },
                new Payment { Id = 5, PaymentNumber = "PAY-005", InvoiceId = 4, PaymentAmount = 8900m, PaymentDate = DateTime.UtcNow.AddDays(-10), PaymentMethod = "Cash", CreatedAt = DateTime.UtcNow }
            );

            // Seed Journal Entries
            modelBuilder.Entity<JournalEntry>().HasData(
                new JournalEntry { Id = 1, JournalNumber = "JE-001", DebitAccountId = 1, CreditAccountId = 4, Amount = 10000m, Description = "Payment for supplier invoice", EntryDate = DateTime.UtcNow.AddDays(-5), CreatedAt = DateTime.UtcNow },
                new JournalEntry { Id = 2, JournalNumber = "JE-002", DebitAccountId = 7, CreditAccountId = 3, Amount = 5000m, Description = "COGS adjustment", EntryDate = DateTime.UtcNow.AddDays(-3), CreatedAt = DateTime.UtcNow },
                new JournalEntry { Id = 3, JournalNumber = "JE-003", DebitAccountId = 2, CreditAccountId = 6, Amount = 7500m, Description = "Invoice recognition", EntryDate = DateTime.UtcNow.AddDays(-2), CreatedAt = DateTime.UtcNow },
                new JournalEntry { Id = 4, JournalNumber = "JE-004", DebitAccountId = 1, CreditAccountId = 7, Amount = 3200m, Description = "Expense payment", EntryDate = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow },
                new JournalEntry { Id = 5, JournalNumber = "JE-005", DebitAccountId = 3, CreditAccountId = 2, Amount = 4500m, Description = "Inventory adjustment", EntryDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow }
            );

            // Seed Tax Rates
            modelBuilder.Entity<TaxRate>().HasData(
                new TaxRate { Id = 1, TaxCode = "SALES", TaxDescription = "Sales Tax Rate", Rate = 0.08m, TaxType = "Sales Tax", EffectiveDate = DateTime.UtcNow.AddDays(-365), CreatedAt = DateTime.UtcNow },
                new TaxRate { Id = 2, TaxCode = "FED", TaxDescription = "Federal Income Tax", Rate = 0.21m, TaxType = "Federal", EffectiveDate = DateTime.UtcNow.AddDays(-365), CreatedAt = DateTime.UtcNow },
                new TaxRate { Id = 3, TaxCode = "STATE", TaxDescription = "State Income Tax", Rate = 0.065m, TaxType = "State", EffectiveDate = DateTime.UtcNow.AddDays(-365), CreatedAt = DateTime.UtcNow },
                new TaxRate { Id = 4, TaxCode = "LOCAL", TaxDescription = "Local Sales Tax", Rate = 0.025m, TaxType = "Local", EffectiveDate = DateTime.UtcNow.AddDays(-365), CreatedAt = DateTime.UtcNow },
                new TaxRate { Id = 5, TaxCode = "PAYROLL", TaxDescription = "Payroll Tax Rate", Rate = 0.15m, TaxType = "Payroll", EffectiveDate = DateTime.UtcNow.AddDays(-365), CreatedAt = DateTime.UtcNow }
            );
        }
    }
}
