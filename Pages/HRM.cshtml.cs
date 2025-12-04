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

    // Model to bind search inputs and apply validation
    [BindProperty]
    public InputModel Input { get; set; } = new();

    // Model to bind Add/Edit employee inputs
    [BindProperty]
    public EmployeeInputModel EmployeeInput { get; set; } = new();

    // Track which employee is being edited
    [BindProperty]
    public int? EditingEmployeeId { get; set; }

    // Property to hold the list of employees for the view (HRM.cshtml)
    public List<Employee> Employees { get; set; } = new();

    // Property to hold all departments for dropdown (unaffected by filters)
    public List<string> AllDepartments { get; set; } = new();

    // Aggregated metrics for the UI
    public int DepartmentsCount { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal MonthlyPayroll { get; set; }

    // Nested class for search inputs with validation
    public class InputModel
    {
        [StringLength(50, ErrorMessage = "Search term cannot exceed 50 characters.")]
        [Display(Name = "Search Employee")]
        public string? SearchTerm { get; set; }
        public string? DepartmentFilter { get; set; }
    }

    // Nested class for Add/Edit inputs
    public class EmployeeInputModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [Range(0, 10000000)]
        public decimal Salary { get; set; }
    }

    // Helper method to check if user is allowed to access HRM
    private bool IsHRMAllowed()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole)) return false;
        
        return userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
            || userRole.Equals("Guest", StringComparison.OrdinalIgnoreCase)
            || userRole.Equals("HR", StringComparison.OrdinalIgnoreCase);
    }

    // OnGet handler to simulate fetching the data (for initial page load)
    public async Task<IActionResult> OnGetAsync()
    {
        if (!IsHRMAllowed())
        {
            TempData["ErrorMessage"] = "You do not have permission to access the HRM page.";
            return RedirectToPage("/Privacy");
        }

        // Load employee data from database
        await LoadEmployeeDataAsync();

        return Page();
    }

    
    // Search handler to process search/filter submissions
    public async Task<IActionResult> OnPostSearchAsync()
    {
        // Enforce HRM access control on POST as well
        if (!IsHRMAllowed())
        {
            TempData["ErrorMessage"] = "You do not have permission to access the HRM page.";
            return RedirectToPage("/Privacy");
        }

        // Log search parameters
        Console.WriteLine($"[HRM Search] SearchTerm: '{Input.SearchTerm}', DepartmentFilter: '{Input.DepartmentFilter}'");
        
        // For search, we don't need ModelState validation - search fields are optional
        // Just proceed with filtering
        Console.WriteLine("[HRM Search] Proceeding with filtering...");
        
        // Load all employees from database first
        await LoadEmployeeDataAsync();
        Console.WriteLine($"[HRM Search] Loaded {Employees.Count} employees from database");
        
        // Optional: Simple filtering logic (if a search term is provided)
        if (!string.IsNullOrWhiteSpace(Input.SearchTerm))
        {
            Employees = Employees
                .Where(e => e.Name!.Contains(Input.SearchTerm, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
            Console.WriteLine($"[HRM Search] After name filter: {Employees.Count} employees");
        }

        // Apply department filter (if selected)
        if (!string.IsNullOrWhiteSpace(Input.DepartmentFilter))
        {
            Employees = Employees
                .Where(e => string.Equals(e.Department, Input.DepartmentFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Console.WriteLine($"[HRM Search] After department filter: {Employees.Count} employees");
        }

        // Recompute metrics after filtering
        ComputeMetrics();
        
        Console.WriteLine($"[HRM Search] Final employee count: {Employees.Count}");
        return Page(); // Stay on the HRM page
    }

    // Add a new employee
    public async Task<IActionResult> OnPostAddAsync()
    {
        // Enforce HRM access control on POST as well
        if (!IsHRMAllowed())
        {
            TempData["ErrorMessage"] = "You do not have permission to add employees.";
            return RedirectToPage("/Privacy");
        }

        if (!ModelState.IsValid)
        {
            await LoadEmployeeDataAsync();
            return Page();
        }

        var emp = new Employee
        {
            Name = EmployeeInput.Name,
            Department = EmployeeInput.Department,
            Role = EmployeeInput.Role,
            Address = EmployeeInput.Address,
            Phone = EmployeeInput.Phone,
            Salary = EmployeeInput.Salary,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(emp);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Employee '{emp.Name}' added successfully!";
        return RedirectToPage();
    }

    // Edit an existing employee
    public async Task<IActionResult> OnPostEditAsync()
    {
        if (!IsHRMAllowed())
        {
            TempData["ErrorMessage"] = "You do not have permission to edit employees.";
            return RedirectToPage("/Privacy");
        }

        if (!EditingEmployeeId.HasValue)
        {
            TempData["ErrorMessage"] = "Invalid employee ID.";
            return RedirectToPage();
        }

        if (!ModelState.IsValid)
        {
            await LoadEmployeeDataAsync();
            return Page();
        }

        var emp = await _context.Employees.FindAsync(EditingEmployeeId.Value);
        if (emp == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToPage();
        }

        emp.Name = EmployeeInput.Name;
        emp.Department = EmployeeInput.Department;
        emp.Role = EmployeeInput.Role;
        emp.Address = EmployeeInput.Address;
        emp.Phone = EmployeeInput.Phone;
        emp.Salary = EmployeeInput.Salary;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Employee '{emp.Name}' updated successfully!";
        return RedirectToPage();
    }

    // Delete an employee
    public async Task<IActionResult> OnPostDeleteAsync(int employeeId)
    {
        if (!IsHRMAllowed())
        {
            TempData["ErrorMessage"] = "You do not have permission to delete employees.";
            return RedirectToPage("/Privacy");
        }

        var emp = await _context.Employees.FindAsync(employeeId);
        if (emp != null)
        {
            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Employee '{emp.Name}' deleted successfully!";
        }

        return RedirectToPage();
    }

    // Helper method to load employee data from the database
    private async Task LoadEmployeeDataAsync()
    {
        Employees = await _context.Employees.ToListAsync();
        
        // Load all departments for the dropdown (unfiltered)
        AllDepartments = await _context.Employees
            .Where(e => !string.IsNullOrWhiteSpace(e.Department))
            .Select(e => e.Department!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
        
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
