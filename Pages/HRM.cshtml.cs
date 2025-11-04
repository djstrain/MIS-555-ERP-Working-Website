using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using WebApplication1.Data;

namespace WebApplication1.Pages;

public class HRMmodel : PageModel
{
    private readonly AppDbContents _context;

    public HRMmodel(AppDbContents context)
    {
        _context = context;
    }

    // Model to bind search/form inputs and apply validation
    [BindProperty]
    public InputModel Input { get; set; } = new();

    // Property to hold the list of employees for the view (HRM.cshtml)
    public List<Employee> Employees { get; set; } = new();

    // Aggregated metrics for the UI
    public int DepartmentsCount { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal MonthlyPayroll { get; set; }

    // Nested class for form inputs with validation
    public class InputModel
    {
    [Required(ErrorMessage = "Employee Name or ID is required for a search.")]
    [StringLength(50, ErrorMessage = "Search term cannot exceed 50 characters.")]
    [Display(Name = "Search Employee")]
    public string? SearchTerm { get; set; }
        
        public string? DepartmentFilter { get; set; }
    }

    // OnGet handler to simulate fetching the data (for initial page load)
    public async Task<IActionResult> OnGetAsync()
    {
        //create a variable save UserRole values from session
        var userRole = HttpContext.Session.GetString("UserRole");
        //check if userrole is null or not admin (case-insensitive)
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to access the HRM page.";
            // redirect regular users to Privacy
            return RedirectToPage("/Privacy");
        }

        // Load employee data from database
        await LoadEmployeeDataAsync();

        return Page();
    }

    
    // OnPost handler to process form submissions (e.g., when a user clicks 'Search')
    public async Task<IActionResult> OnPostAsync()
    {
        // Enforce admin-only access on POST as well
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You do not have permission to access the HRM page.";
            return RedirectToPage("/Privacy");
        }

        // Check if validation rules (like [Required]) have failed
        if (!ModelState.IsValid)
        {
            await LoadEmployeeDataAsync(); 
            return Page(); // Return to the page to show the error message
        }
        
        // Load all employees from database first
        await LoadEmployeeDataAsync(); 
        
        // Optional: Simple filtering logic (if a search term is provided)
        if (!string.IsNullOrWhiteSpace(Input.SearchTerm))
        {
            Employees = Employees
                .Where(e => e.Name!.Contains(Input.SearchTerm, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Apply department filter (if selected)
        if (!string.IsNullOrWhiteSpace(Input.DepartmentFilter))
        {
            Employees = Employees
                .Where(e => string.Equals(e.Department, Input.DepartmentFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Recompute metrics after filtering
        ComputeMetrics();
        
        return Page(); // Stay on the HRM page
    }

    // Helper method to load employee data from the database
    private async Task LoadEmployeeDataAsync()
    {
        Employees = await _context.Employees.ToListAsync();
        
        // compute aggregated values once data is loaded
        ComputeMetrics();
    }

    // Helper to compute metrics from the current Employees list
    private void ComputeMetrics()
    {
        DepartmentsCount = Employees
            .Where(e => !string.IsNullOrWhiteSpace(e.Department))
            .Select(e => e.Department!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        AverageSalary = Employees.Any() ? Math.Round(Employees.Average(e => e.Salary), 2) : 0m;

        // Monthly payroll = sum of salaries / 12
        MonthlyPayroll = Employees.Any() ? Math.Round(Employees.Sum(e => e.Salary) / 12m, 2) : 0m;
    }
}