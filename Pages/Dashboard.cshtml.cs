using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;

namespace WebApplication1.Pages;

public class DashboardModel : PageModel
{
    private readonly AppDbContents _context;

    public DashboardModel(AppDbContents context)
    {
        _context = context;
    }

    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal TotalPayroll { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if user is logged in
        var userEmail = HttpContext.Session.GetString("UserEmail");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(userEmail))
        {
            // Redirect to login if not authenticated
            return RedirectToPage("/Index");
        }

        UserEmail = userEmail;
        UserRole = userRole;

        // Load dashboard metrics from database
        await LoadDashboardMetricsAsync();

        return Page();
    }

    private async Task LoadDashboardMetricsAsync()
    {
        // Get total number of employees
        TotalEmployees = await Task.Run(() => _context.Employees.Count());

        // Get total number of unique departments
        TotalDepartments = await Task.Run(() => 
            _context.Employees
                .Where(e => !string.IsNullOrWhiteSpace(e.Department))
                .Select(e => e.Department)
                .Distinct()
                .Count()
        );

        // Calculate average salary
        var employees = _context.Employees.ToList();
        AverageSalary = employees.Any() ? Math.Round(employees.Average(e => e.Salary), 2) : 0m;

        // Calculate total payroll (annual)
        TotalPayroll = employees.Any() ? Math.Round(employees.Sum(e => e.Salary), 2) : 0m;
    }
}
