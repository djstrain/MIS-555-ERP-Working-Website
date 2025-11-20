using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data
{
    [Table("Companies")]
    public class Company
    {
        public int Id { get; set; }
        [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
        [MaxLength(100)] public string? Industry { get; set; }
        [MaxLength(200)] public string? Website { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }

    [Table("Contacts")]
    public class Contact
    {
        public int Id { get; set; }
        [ForeignKey(nameof(Company))] public int? CompanyId { get; set; }
        public Company? Company { get; set; }
        [Required, MaxLength(80)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(80)] public string LastName { get; set; } = string.Empty;
        [MaxLength(150)] public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }

    [Table("Opportunities")]
    public class Opportunity
    {
        public int Id { get; set; }
        [ForeignKey(nameof(Company))] public int CompanyId { get; set; }
        public Company? Company { get; set; }
        [Required, MaxLength(160)] public string Name { get; set; } = string.Empty;
        [MaxLength(40)] public string? Stage { get; set; }
        public decimal? Value { get; set; }
        public DateTime? CloseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("Activities")]
    public class Activity
    {
        public int Id { get; set; }
        [ForeignKey(nameof(Company))] public int? CompanyId { get; set; }
        public Company? Company { get; set; }
        [ForeignKey(nameof(Contact))] public int? ContactId { get; set; }
        public Contact? Contact { get; set; }
        [MaxLength(40)] public string? ActivityType { get; set; }
        [MaxLength(160)] public string? Subject { get; set; }
        public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
        [MaxLength(500)] public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("Notes")]
    public class Note
    {
        public int Id { get; set; }
        [ForeignKey(nameof(Company))] public int? CompanyId { get; set; }
        public Company? Company { get; set; }
        [ForeignKey(nameof(Contact))] public int? ContactId { get; set; }
        public Contact? Contact { get; set; }
        [Required, MaxLength(1000)] public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}