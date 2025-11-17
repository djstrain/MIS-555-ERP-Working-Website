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

// Configure detailed logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Connect AppDbContents to database with detailed logging
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
<<<<<<< Updated upstream

// Always use MySQL with the newconnection101 user
builder.Services.AddDbContext<AppDbContents>(options =>
{
<<<<<<< Updated upstream
    var connString = "Server=localhost;Database=rxerp;Uid=newconnection101;Pwd=YourPasswordHere;";
    options.UseMySql(connString, new MySqlServerVersion(new Version(8, 0, 21)));
=======
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
    options.UseMySql(connectionString, serverVersion, 
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)
    );
>>>>>>> Stashed changes
=======
builder.Services.AddDbContext<AppDbContents>(options =>
{
    options
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .UseMySql(connectionString, 
            new MySqlServerVersion(new Version(8, 0, 21)),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure(3));
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
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
=======
    var db = scope.ServiceProvider.GetRequiredService<AppDbContents>();
    db.Database.EnsureCreated();
    // Seed demo employees if none exist
    try
    {
        if (!db.Employees.Any())
        {
            db.Employees.AddRange(
                new Employee { Name = "Aisha Mahmoud", Department = "Engineering", Role = "Senior Software Engineer", Address = "1200 Elm St, Apt 4B, Springfield", Phone = "555-0101", Salary = 115000m, CreatedAt = DateTime.Parse("2023-08-01") },
                new Employee { Name = "Daniel Kim", Department = "Engineering", Role = "DevOps Engineer", Address = "22 River Rd, Suite 3, Springfield", Phone = "555-0102", Salary = 98000m, CreatedAt = DateTime.Parse("2024-01-18") },
                new Employee { Name = "Priya Patel", Department = "Engineering", Role = "Software Engineer", Address = "45 Market St, Springfield", Phone = "555-0103", Salary = 87000m, CreatedAt = DateTime.Parse("2024-03-05") },

                new Employee { Name = "Maria González", Department = "HR", Role = "HR Manager", Address = "300 Oak Ave, Springfield", Phone = "555-0201", Salary = 78000m, CreatedAt = DateTime.Parse("2022-11-12") },
                new Employee { Name = "Liam O'Connor", Department = "HR", Role = "Recruiter", Address = "78 Pine St, Springfield", Phone = "555-0202", Salary = 56000m, CreatedAt = DateTime.Parse("2024-06-20") },
                new Employee { Name = "Chen Wei", Department = "HR", Role = "HR Coordinator", Address = "9 Willow Ln, Springfield", Phone = "555-0203", Salary = 52000m, CreatedAt = DateTime.Parse("2023-05-02") },

                new Employee { Name = "Olivia Park", Department = "Sales", Role = "Sales Director", Address = "150 Commerce Blvd, Springfield", Phone = "555-0301", Salary = 129000m, CreatedAt = DateTime.Parse("2021-09-03") },
                new Employee { Name = "Noah Johnson", Department = "Sales", Role = "Account Executive", Address = "4 Capital Way, Springfield", Phone = "555-0302", Salary = 72000m, CreatedAt = DateTime.Parse("2023-12-01") },
                new Employee { Name = "Sofia Russo", Department = "Sales", Role = "Account Manager", Address = "51 Lakeview Dr, Springfield", Phone = "555-0303", Salary = 68000m, CreatedAt = DateTime.Parse("2024-02-27") },

                new Employee { Name = "Ethan Brown", Department = "Finance", Role = "Finance Manager", Address = "88 Bank St, Springfield", Phone = "555-0401", Salary = 98000m, CreatedAt = DateTime.Parse("2022-02-14") },
                new Employee { Name = "Isabella Rossi", Department = "Finance", Role = "Financial Analyst", Address = "16 Hill Rd, Springfield", Phone = "555-0402", Salary = 72000m, CreatedAt = DateTime.Parse("2023-07-09") },
                new Employee { Name = "Mateo Alvarez", Department = "Finance", Role = "Payroll Specialist", Address = "2 Sunset Blvd, Springfield", Phone = "555-0403", Salary = 59000m, CreatedAt = DateTime.Parse("2024-04-11") }
            );
            db.SaveChanges();
            var seedLogger = scope.ServiceProvider.GetService<ILogger<Program>>();
            seedLogger?.LogInformation("Seeded demo employees into Employees table.");
>>>>>>> Stashed changes
        }
    }
    catch (Exception ex)
    {
<<<<<<< Updated upstream
        // Log and continue so the site can run for debugging even if the DB is unavailable.
        logger.LogError(ex, "Database initialization failed. Continuing without database for local debug.");
=======
        var seedLogger = scope.ServiceProvider.GetService<ILogger<Program>>();
        seedLogger?.LogError(ex, "Error while seeding demo employees");
>>>>>>> Stashed changes
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
