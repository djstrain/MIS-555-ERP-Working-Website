# Financial Management Module - Deployment Guide

## Overview
The Financial Management module provides comprehensive financial tracking and reporting capabilities for the CTRL+Freak ERP system. It includes accounts management, partner tracking, invoicing, payments, journal entries, and tax rate configuration.

## Features

### 1. **Accounts Management** (Chart of Accounts)
- Track all financial accounts (Assets, Liabilities, Equity, Revenue, Expenses)
- Maintain account balances
- Support for standard accounting account types

### 2. **Partners** (Vendors & Customers)
- Manage vendor and customer information
- Track partner contact details (email, phone)
- Support for vendor, customer, and associate classifications

### 3. **Invoices**
- Create and track sales/purchase invoices
- Monitor invoice status (Pending, Paid, Overdue)
- Link invoices to partners
- Track invoice and due dates

### 4. **Open Balances**
- Record opening balances for accounts
- Maintain historical balance records
- Track balance dates for period reconciliation

### 5. **Payments**
- Record payment transactions
- Link payments to invoices
- Support multiple payment methods (Wire, Check, Credit Card, Bank Transfer, Cash)
- Track payment dates

### 6. **Journal Entries**
- Record double-entry accounting transactions
- Link debit and credit accounts
- Maintain audit trail with descriptions

### 7. **Tax Rates**
- Configure tax rates by type (Federal, State, Local, Sales, Payroll)
- Maintain effective dates for tax rate changes
- Support multiple tax codes

## Dashboard Metrics

The Financial Management page displays three key aggregate metrics:

- **Total Revenue**: Sum of all invoices (excluding overdue)
- **Total Expenses**: Sum of expense accounts
- **Net Balance**: Revenue minus Expenses
- **Invoice Statistics**: Total, Paid, and Pending invoice counts

## Database Schema

### Tables Created:
1. `Accounts` - Chart of accounts
2. `Partners` - Vendors, customers, associates
3. `Invoices` - Invoice records
4. `OpenBalances` - Beginning balances
5. `Payments` - Payment transactions
6. `JournalEntries` - General ledger entries
7. `TaxRates` - Tax rate configuration

All tables include:
- Primary key (Id)
- CreatedAt timestamp for audit trail
- Appropriate indexes for query performance
- Foreign key constraints for referential integrity

## Deployment Instructions

### Prerequisites:
- MySQL Server (5.7 or higher)
- Database: `rxerp`
- Database User: `rxerp_user` with full privileges

### Step 1: Create the Database (if not exists)
```sql
CREATE DATABASE IF NOT EXISTS rxerp;
CREATE USER IF NOT EXISTS 'rxerp_user'@'localhost' IDENTIFIED BY 'your_password';
GRANT ALL PRIVILEGES ON rxerp.* TO 'rxerp_user'@'localhost';
FLUSH PRIVILEGES;
```

### Step 2: Run the SQL Deployment Script
Execute the `deploy_financial_schema.sql` file in your MySQL client:

```bash
mysql -u rxerp_user -p rxerp < deploy_financial_schema.sql
```

Or copy and paste the contents into MySQL Workbench or phpMyAdmin.

### Step 3: Start the Application
```bash
dotnet run
```

The application will automatically:
1. Apply Entity Framework migrations (if needed)
2. Seed sample data via `OnModelCreating()` in `AppDbContents.cs`
3. Create tables if they don't exist
4. Start the web server on `http://localhost:5176`

### Step 4: Access Financial Management
1. Navigate to `http://localhost:5176`
2. Login with Admin credentials
3. Click "Financial" in the navigation menu

## Sample Data

The deployment includes 5+ sample records for each table:

### Accounts (7 accounts):
- Cash, Accounts Receivable, Inventory (Assets)
- Accounts Payable (Liability)
- Common Stock (Equity)
- Sales Revenue (Revenue)
- Cost of Goods Sold (Expense)

### Partners (5 partners):
- Tech Solutions Inc (Vendor)
- Global Manufacturing Co (Customer)
- Premium Supplies Ltd (Vendor)
- Enterprise Solutions (Customer)
- Innovation Partners (Associate)

### Invoices (5 invoices):
- Various statuses: Paid, Pending, Overdue
- Date range: -60 days to current
- Amounts: $3,200 - $8,900

### Payments, Journals & Tax Rates (5 each):
- Complete transaction records
- Multiple payment methods
- Standard federal/state/local tax rates

## Access Control

The Financial Management page is restricted to **Admin role** users only.

To grant access to other users:
1. Update the role in `_Layout.cshtml` navigation check
2. Or modify `FinancialManagement.cshtml.cs` to add role-based authorization

## API Endpoints

The module is accessed via:
- **Page**: `/FinancialManagement`
- **Handler**: HTTP GET (OnGetAsync in PageModel)

## Performance Considerations

- Indexes created on all foreign key and frequently-queried columns
- Table structure optimized for reporting queries
- Efficient relationship loading with `.Include()`

## Backup and Maintenance

### Regular Backups:
```bash
mysqldump -u rxerp_user -p rxerp > rxerp_backup.sql
```

### Restore:
```bash
mysql -u rxerp_user -p rxerp < rxerp_backup.sql
```

## Troubleshooting

### Issue: "Tables don't exist"
**Solution**: Run the deployment script and ensure database user has CREATE privileges.

### Issue: "Foreign key constraint fails"
**Solution**: Ensure data is inserted in dependency order (Partners before Invoices, Accounts before JournalEntries).

### Issue: "Connection string error"
**Solution**: Verify `appsettings.json` contains correct MySQL connection string for your environment.

## Team Deployment Checklist

- [ ] Database created and user configured
- [ ] `deploy_financial_schema.sql` executed successfully
- [ ] `appsettings.json` connection string verified
- [ ] Application started without errors
- [ ] Financial Management page accessible
- [ ] Sample data visible in all 7 tables
- [ ] Aggregate metrics displaying correctly

## Related Files

- `Pages/FinancialManagement.cshtml` - UI page with 7 tables and metrics
- `Pages/FinancialManagement.cshtml.cs` - PageModel with data loading and calculations
- `Data/FinancialModels.cs` - Entity classes
- `Data/AppDbContents.cs` - DbContext with seeding logic
- `deploy_financial_schema.sql` - Complete database schema and sample data

## Version

- **Module Version**: 1.0
- **Created**: 2025
- **Last Updated**: 2025

## Support

For issues or questions, contact the development team.

---

**Note**: This module provides the foundation for financial reporting. Additional features (reports, analytics, budget tracking) can be added in future releases.
