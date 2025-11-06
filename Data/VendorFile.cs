using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data
{
    public class VendorFile
    {
        [Key]
        public int FileID { get; set; }

        [Required]
        [ForeignKey("Vendor")] 
        public int VendorID { get; set; }

        [Required]
        [StringLength(260)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ContentType { get; set; }

        [Required]
        public ulong SizeBytes { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(64)]
        public string? SHA256 { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? UploadedBy { get; set; }

        public bool IsPublic { get; set; } = false;

        public Vendor? Vendor { get; set; }
    }
}
