using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data
{
    public class InventoryItem
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string SKU { get; set; } = string.Empty;
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMovementAt { get; set; }
    }
}
