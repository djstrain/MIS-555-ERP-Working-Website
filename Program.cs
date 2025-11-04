using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);
// Allow per-developer overrides without committing secrets
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
// connect AppDbContents to database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Always use MySQL with the newconnection101 user
builder.Services.AddDbContext<AppDbContents>(options =>
{
    var connString = "Server=localhost;Database=rxerp;Uid=newconnection101;Pwd=YourPasswordHere;";
    options.UseMySql(connString, new MySqlServerVersion(new Version(8, 0, 21)));
});

// HttpContext accessor and Session support
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



var app = builder.Build();

// Ensure database exists and seed initial data
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContents>();
        
        // Create database and tables
        db.Database.EnsureCreated();
        
        // Seed admin user if none exists
        if (!db.UserCredentials.Any())
        {
            var adminUser = new UserCredentials
            {
                Email = "admin@ctrlfreak.com",
                Password = "AdminPassword123!", // This is the exact password to use
                Role = "admin", // Changed to lowercase to match comparison
                CreatedAt = DateTime.UtcNow
            };
            db.UserCredentials.Add(adminUser);
            db.SaveChanges();
            
            logger.LogInformation("Seeded admin user: {Email}", adminUser.Email);
        }
    }
    catch (Exception ex)
    {
        // Log and continue so the site can run for debugging even if the DB is unavailable.
        logger.LogError(ex, "Database initialization failed. Continuing without database for local debug.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Enable session before endpoints
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();
