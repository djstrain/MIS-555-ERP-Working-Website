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
    }
}
 