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
        [Column("email")]  // Match your database column name
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Column("password")]  // Match your database column name
        public string Password { get; set; } = string.Empty;

        [Column("role")]  // Match your database column name
        public string Role { get; set; } = "user";  // Default to lowercase

        [Column("created_at")]  // Match your database column name
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
 
 
 