# Feature-Oriented Change Summary for sprint 3

This document organizes changes by feature: Login (UserCredentials), HRM (Employees), Vendors (Vendors), followed by cross-cutting security/session and setup notes.

## 1) Login page + UserCredentials database

- Database table: `UserCredentials`
  - Columns: `Id` (PK), `Email` (unique), `Password` (plaintext for now), `Role` (Admin/User), `CreatedAt`.
  - Created via `docs/database-setup.sql`.
- EF Core
  - Entity: `Data/UserCredentials.cs`
  - DbContext: `Data/AppDbContents.cs` exposes `DbSet<UserCredentials>`
- Page flow
  - UI/Code: `Pages/Index.cshtml` + `Pages/Index.cshtml.cs`
  - Validates email/password; looks up user by email; compares stored password (plaintext) to input.
  - On success: stores session keys `UserRole` and `UserEmail` and redirects based on role:
    - Admin → `/HRM`
    - User → `/Privacy`
  - On failure: shows “Invalid email or password.” and logs details.
- Notes
  - Logging is added for login attempts and outcomes.
  - There is a legacy in-memory dictionary for demo users, but the logic first checks the database.

## 2) HRM page + Employees database

- Database table: `Employees`
  - Columns: `Id`, `Name`, `Department`, `Role`, `Address`, `Phone`, `Salary` (DECIMAL), `CreatedAt`.
  - Seed/script in `docs/database-setup.sql`.
- EF Core
  - Entity: `Data/Employee.cs`
  - DbContext: `DbSet<Employee>` in `Data/AppDbContents.cs`
- Page capabilities (Admin-only)
  - UI/Code: `Pages/HRM.cshtml` + `Pages/HRM.cshtml.cs`
  - Read from DB and render a searchable, filterable employee table.
  - CRUD with Bootstrap modals:
    - Add employee (validated server-side)
    - Edit employee in place (ID-tracked)
    - Delete with confirmation
  - Metrics shown at top (computed on server):
    - Distinct departments count
    - Average salary
    - Monthly payroll estimate (sum of salaries / 12)
- Validation & UX
  - Input models enforce required fields and ranges.
  - Alerts use TempData for consistent success/error messaging.

## 3) Vendor Management page + Vendors database

- Database table: `Vendors`
  - Columns: `VendorID` (PK), `VendorName`, `ContactPerson`, `Email`, `VendorType`, `Status` (Active/Inactive), `Rating` (0–5), `CreatedAt`, `UpdatedAt`.
  - Seed/script in `docs/database-setup.sql`.
- EF Core
  - Entity: `Data/Vendor.cs`
  - DbContext: `DbSet<Vendor>` in `Data/AppDbContents.cs`
- Page capabilities (Admin-only)
  - UI/Code: `Pages/VendorManagement.cshtml` + `Pages/VendorManagement.cshtml.cs`
  - Search by vendor name/contact/email; filter by Status and Type.
  - CRUD with Bootstrap modals: add, edit, delete vendors.
  - Metrics: total vendors, active vendors, average rating (where provided).
- UX alignment
  - Styling matches HRM (Bootstrap cards/tables, Font Awesome icons, TempData alerts).

## 4) Security, sessions, navigation, and logging

- Sessions
  - Keys: `UserRole`, `UserEmail` set at login.
  - Configured in `Program.cs` with `AddSession()` and `UseSession()`.
- Role-based access control (RBAC)
  - HRM and Vendor Management pages check for Admin on both GET and POST.
  - Unauthorized users are redirected to `/Privacy` with an error message.
- Navigation
  - `Pages/Shared/_Layout.cshtml` shows “HRM” and “Vendors” links only for Admins.
  - Logout button visible when signed in.
- Logout
  - `Pages/Logout.cshtml.cs` clears session and redirects back to Login with a confirmation message.
- Logging & alerts
  - ILogger used for key events (login attempts, role routing, access denials).
  - TempData-based success/error alerts provide user feedback.
- Known security gaps (planned improvements)
  - Passwords are plaintext; adopt hashing (e.g., ASP.NET Core Identity) next.
  - Using `EnsureCreated()` in dev; move to EF Core migrations.
  - Enforce HTTPS and secure cookie settings.

## 5) Setup and environment

- Database bootstrap
  - Run `docs/database-setup.sql` to create `rxerp` and tables: `UserCredentials`, `Employees`, `Vendors` with sample data.
  - Sample Admin for testing: `admin@ctrlfreak.com` / `AdminPassword123!`.
- Configuration
  - Connection string in `appsettings.json` (MySQL/Pomelo provider).
  - DbContext: `AppDbContents` registered in `Program.cs`.

## 6) Quick references

- Entities and DbContext
  - `Data/UserCredentials.cs`, `Data/Employee.cs`, `Data/Vendor.cs`, `Data/AppDbContents.cs`
- Pages
  - Login: `Pages/Index.cshtml` + `Pages/Index.cshtml.cs`
  - HRM: `Pages/HRM.cshtml` + `Pages/HRM.cshtml.cs`
  - Vendors: `Pages/VendorManagement.cshtml` + `Pages/VendorManagement.cshtml.cs`
- Shared
  - Layout/navbar: `Pages/Shared/_Layout.cshtml`
  - Logout: `Pages/Logout.cshtml.cs`
