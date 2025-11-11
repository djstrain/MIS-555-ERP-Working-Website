using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data
{
    public class Account
    {
        public int Id { get; set; }
        [Required]
        public string AccountNumber { get; set; } = string.Empty;
        [Required]
        public string AccountName { get; set; } = string.Empty;
        [Required]
        public string AccountType { get; set; } = string.Empty; // Asset, Liability, Equity, Revenue, Expense
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Partner
    {
        public int Id { get; set; }
        [Required]
        public string PartnerName { get; set; } = string.Empty;
        [Required]
        public string PartnerType { get; set; } = string.Empty; // Vendor, Customer, Associate
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Invoice
    {
        public int Id { get; set; }
        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;
        [Required]
        public int PartnerId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Partner? Partner { get; set; }
    }

    public class OpenBalance
    {
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        public decimal OpeningBalance { get; set; }
        public DateTime BalanceDate { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Account? Account { get; set; }
    }

    public class Payment
    {
        public int Id { get; set; }
        [Required]
        public string PaymentNumber { get; set; } = string.Empty;
        [Required]
        public int InvoiceId { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Check, Wire, Credit Card
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Invoice? Invoice { get; set; }
    }

    public class JournalEntry
    {
        public int Id { get; set; }
        [Required]
        public string JournalNumber { get; set; } = string.Empty;
        [Required]
        public int DebitAccountId { get; set; }
        [Required]
        public int CreditAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Account? DebitAccount { get; set; }
        public Account? CreditAccount { get; set; }
    }

    public class TaxRate
    {
        public int Id { get; set; }
        [Required]
        public string TaxCode { get; set; } = string.Empty;
        [Required]
        public string TaxDescription { get; set; } = string.Empty;
        public decimal Rate { get; set; } // e.g., 0.08 for 8%
        public string TaxType { get; set; } = string.Empty; // Federal, State, Local, Sales Tax, etc.
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
