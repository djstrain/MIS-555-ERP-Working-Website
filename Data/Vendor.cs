using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data
{
    public class Vendor
    {
        [Key]
        public int VendorID { get; set; }

        [Required]
        [StringLength(150)]
        public string VendorName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? VendorType { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active";

        [Column(TypeName = "decimal(3, 2)")]
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public decimal? Rating { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
