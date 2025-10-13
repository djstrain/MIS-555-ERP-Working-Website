using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApplication1.Pages;

public class HRMmodel : PageModel
{
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
    public void OnGet()
    {
        // Load the hardcoded data to ensure the page shows content.
        LoadEmployeeData();
    }
    
    // OnPost handler to process form submissions (e.g., when a user clicks 'Search')
    public IActionResult OnPost()
    {
        // Check if validation rules (like [Required]) have failed
        if (!ModelState.IsValid)
        {
            LoadEmployeeData(); 
            return Page(); // Return to the page to show the error message
        }
        
        // If valid, proceed with search/filter logic
        LoadEmployeeData(); 
        
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

    // Helper method to load the pre-set data into memory.
    private void LoadEmployeeData()
    {
        Employees = new List<Employee>
        {
            // Existing Employees
            new Employee {
                Name = "Jane Doe",
                Department = "HR",
                Role = "Specialist",
                Address = "123 Main St",
                Phone = "555-1234",
                Salary = 75000.00m
            },
            new Employee {
                Name = "John Smith",
                Department = "Finance",
                Role = "Manager",
                Address = "456 Oak Ave",
                Phone = "555-5678",
                Salary = 120000.00m
            },
            new Employee {
                Name = "Alex Lee",
                Department = "IT",
                Role = "Engineer",
                Address = "789 Pine Ln",
                Phone = "555-9012",
                Salary = 110000.00m
            },
            
            // --- NEW EMPLOYEES ADDED BELOW ---
            new Employee {
                Name = "Sarah Chen",
                Department = "Sales",
                Role = "Account Executive",
                Address = "101 Market St",
                Phone = "555-1010",
                Salary = 95000.00m
            },
            new Employee {
                Name = "David Brown",
                Department = "IT",
                Role = "Helpdesk Technician",
                Address = "202 Tech Way",
                Phone = "555-2020",
                Salary = 65000.00m
            },
            new Employee {
                Name = "Maria Garcia",
                Department = "HR",
                Role = "Recruiter",
                Address = "303 River Rd",
                Phone = "555-3030",
                Salary = 80000.00m
            },
            new Employee {
                Name = "Michael Clark",
                Department = "Finance",
                Role = "Analyst",
                Address = "404 Corporate Dr",
                Phone = "555-4040",
                Salary = 105000.00m
            }
        };

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

// Simple employee model used by the HRM page
public class Employee
{
    public string? Name { get; set; }
    public string? Department { get; set; }
    public string? Role { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public decimal Salary { get; set; } 
}