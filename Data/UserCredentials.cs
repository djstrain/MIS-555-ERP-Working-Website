using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public class UserCredentials
    {
        [Key]
        public int Id { get; set; }
 
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
 
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
 
        [Column("Role")]
        public string Role { get; set; } = "User";
 
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 
        [NotMapped]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
 
 