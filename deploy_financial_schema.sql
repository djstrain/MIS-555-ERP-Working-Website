-- ============================================================================
-- Financial Management Database Deployment Script
-- For: CTRL+Freak ERP System - Financial Management Module
-- Version: 1.0
-- Created: 2025
-- Database: rxerp
-- Usage: Run this script in MySQL to deploy financial module
-- ============================================================================

-- ============================================================================
-- ACCOUNTS TABLE - Chart of Accounts
-- ============================================================================
CREATE TABLE IF NOT EXISTS Accounts (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    AccountNumber VARCHAR(50) NOT NULL UNIQUE,
    AccountName VARCHAR(255) NOT NULL,
    AccountType VARCHAR(50) NOT NULL COMMENT 'Asset, Liability, Equity, Revenue, Expense',
    Balance DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_account_number (AccountNumber),
    INDEX idx_account_type (AccountType),
    INDEX idx_created_at (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- PARTNERS TABLE - Vendors, Customers, Associates
-- ============================================================================
CREATE TABLE IF NOT EXISTS Partners (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    PartnerName VARCHAR(255) NOT NULL,
    PartnerType VARCHAR(50) NOT NULL COMMENT 'Vendor, Customer, Associate',
    Email VARCHAR(255),
    Phone VARCHAR(20),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create index for partner lookups
CREATE INDEX idx_partner_type ON Partners(PartnerType);
CREATE INDEX idx_partner_name ON Partners(PartnerName);

-- ============================================================================
-- INVOICES TABLE - Sales and Purchase Invoices
-- ============================================================================
CREATE TABLE IF NOT EXISTS Invoices (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    InvoiceNumber VARCHAR(50) NOT NULL UNIQUE,
    PartnerId INT NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending' COMMENT 'Pending, Paid, Overdue',
    InvoiceDate DATETIME NOT NULL,
    DueDate DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PartnerId) REFERENCES Partners(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create indexes for invoice queries
CREATE INDEX idx_invoice_number ON Invoices(InvoiceNumber);
CREATE INDEX idx_invoice_status ON Invoices(Status);
CREATE INDEX idx_invoice_partner ON Invoices(PartnerId);
CREATE INDEX idx_invoice_date ON Invoices(InvoiceDate);

-- ============================================================================
-- OPEN_BALANCES TABLE - Beginning Balances
-- ============================================================================
CREATE TABLE IF NOT EXISTS OpenBalances (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    AccountId INT NOT NULL,
    OpeningBalance DECIMAL(18, 2) NOT NULL,
    BalanceDate DATETIME NOT NULL,
    Description VARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (AccountId) REFERENCES Accounts(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create index for balance lookups
CREATE INDEX idx_open_balance_account ON OpenBalances(AccountId);
CREATE INDEX idx_open_balance_date ON OpenBalances(BalanceDate);

-- ============================================================================
-- PAYMENTS TABLE - Payment Records
-- ============================================================================
CREATE TABLE IF NOT EXISTS Payments (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    PaymentNumber VARCHAR(50) NOT NULL UNIQUE,
    InvoiceId INT NOT NULL,
    PaymentAmount DECIMAL(18, 2) NOT NULL,
    PaymentDate DATETIME NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL COMMENT 'Cash, Check, Wire, Credit Card, Bank Transfer',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create indexes for payment queries
CREATE INDEX idx_payment_number ON Payments(PaymentNumber);
CREATE INDEX idx_payment_invoice ON Payments(InvoiceId);
CREATE INDEX idx_payment_date ON Payments(PaymentDate);
CREATE INDEX idx_payment_method ON Payments(PaymentMethod);

-- ============================================================================
-- JOURNAL_ENTRIES TABLE - General Ledger Entries
-- ============================================================================
CREATE TABLE IF NOT EXISTS JournalEntries (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    JournalNumber VARCHAR(50) NOT NULL UNIQUE,
    DebitAccountId INT NOT NULL,
    CreditAccountId INT NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    Description VARCHAR(500),
    EntryDate DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (DebitAccountId) REFERENCES Accounts(Id) ON DELETE RESTRICT,
    FOREIGN KEY (CreditAccountId) REFERENCES Accounts(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create indexes for journal entry queries
CREATE INDEX idx_journal_number ON JournalEntries(JournalNumber);
CREATE INDEX idx_journal_debit ON JournalEntries(DebitAccountId);
CREATE INDEX idx_journal_credit ON JournalEntries(CreditAccountId);
CREATE INDEX idx_journal_date ON JournalEntries(EntryDate);

-- ============================================================================
-- TAX_RATES TABLE - Tax Rate Configuration
-- ============================================================================
CREATE TABLE IF NOT EXISTS TaxRates (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    TaxCode VARCHAR(50) NOT NULL UNIQUE,
    TaxDescription VARCHAR(255) NOT NULL,
    Rate DECIMAL(5, 4) NOT NULL COMMENT 'Stored as decimal (0.08 = 8%)',
    TaxType VARCHAR(50) NOT NULL COMMENT 'Federal, State, Local, Sales Tax, Payroll',
    EffectiveDate DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create index for tax lookups
CREATE INDEX idx_tax_code ON TaxRates(TaxCode);
CREATE INDEX idx_tax_type ON TaxRates(TaxType);

-- ============================================================================
-- SAMPLE DATA INSERTION
-- ============================================================================

-- Insert Sample Accounts
INSERT INTO Accounts (AccountNumber, AccountName, AccountType, Balance, CreatedAt) VALUES
('1000', 'Cash', 'Asset', 50000.00, UTC_TIMESTAMP()),
('1100', 'Accounts Receivable', 'Asset', 25000.00, UTC_TIMESTAMP()),
('1200', 'Inventory', 'Asset', 75000.00, UTC_TIMESTAMP()),
('2000', 'Accounts Payable', 'Liability', -30000.00, UTC_TIMESTAMP()),
('3000', 'Common Stock', 'Equity', 100000.00, UTC_TIMESTAMP()),
('4000', 'Sales Revenue', 'Revenue', 150000.00, UTC_TIMESTAMP()),
('5000', 'Cost of Goods Sold', 'Expense', -45000.00, UTC_TIMESTAMP());

-- Insert Sample Partners
INSERT INTO Partners (PartnerName, PartnerType, Email, Phone, CreatedAt) VALUES
('Tech Solutions Inc', 'Vendor', 'info@techsolutions.com', '(555) 123-4567', UTC_TIMESTAMP()),
('Global Manufacturing Co', 'Customer', 'sales@globalmfg.com', '(555) 234-5678', UTC_TIMESTAMP()),
('Premium Supplies Ltd', 'Vendor', 'order@premium-supplies.com', '(555) 345-6789', UTC_TIMESTAMP()),
('Enterprise Solutions', 'Customer', 'contact@enterprise.com', '(555) 456-7890', UTC_TIMESTAMP()),
('Innovation Partners', 'Associate', 'hello@innovationpartners.com', '(555) 567-8901', UTC_TIMESTAMP());

-- Insert Sample Invoices
INSERT INTO Invoices (InvoiceNumber, PartnerId, Amount, Status, InvoiceDate, DueDate, CreatedAt) VALUES
('INV-001', 2, 5000.00, 'Paid', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 30 DAY), DATE_SUB(UTC_TIMESTAMP(), INTERVAL 5 DAY), UTC_TIMESTAMP()),
('INV-002', 4, 7500.00, 'Pending', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 15 DAY), DATE_ADD(UTC_TIMESTAMP(), INTERVAL 15 DAY), UTC_TIMESTAMP()),
('INV-003', 2, 3200.00, 'Pending', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 10 DAY), DATE_ADD(UTC_TIMESTAMP(), INTERVAL 20 DAY), UTC_TIMESTAMP()),
('INV-004', 4, 8900.00, 'Paid', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 45 DAY), DATE_SUB(UTC_TIMESTAMP(), INTERVAL 20 DAY), UTC_TIMESTAMP()),
('INV-005', 2, 4500.00, 'Overdue', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 60 DAY), DATE_SUB(UTC_TIMESTAMP(), INTERVAL 15 DAY), UTC_TIMESTAMP());

-- Insert Sample Open Balances
INSERT INTO OpenBalances (AccountId, OpeningBalance, BalanceDate, Description, CreatedAt) VALUES
(1, 25000.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 90 DAY), 'Opening Balance - Cash Account', UTC_TIMESTAMP()),
(2, 15000.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 90 DAY), 'Opening Balance - Receivables', UTC_TIMESTAMP()),
(3, 60000.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 90 DAY), 'Opening Balance - Inventory', UTC_TIMESTAMP()),
(4, -20000.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 90 DAY), 'Opening Balance - Payables', UTC_TIMESTAMP()),
(5, 100000.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 90 DAY), 'Opening Balance - Equity', UTC_TIMESTAMP());

-- Insert Sample Payments
INSERT INTO Payments (PaymentNumber, InvoiceId, PaymentAmount, PaymentDate, PaymentMethod, CreatedAt) VALUES
('PAY-001', 1, 5000.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 20 DAY), 'Wire', UTC_TIMESTAMP()),
('PAY-002', 4, 8900.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 30 DAY), 'Check', UTC_TIMESTAMP()),
('PAY-003', 1, 5000.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 5 DAY), 'Credit Card', UTC_TIMESTAMP()),
('PAY-004', 2, 3750.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 2 DAY), 'Bank Transfer', UTC_TIMESTAMP()),
('PAY-005', 4, 8900.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 10 DAY), 'Cash', UTC_TIMESTAMP());

-- Insert Sample Journal Entries
INSERT INTO JournalEntries (JournalNumber, DebitAccountId, CreditAccountId, Amount, Description, EntryDate, CreatedAt) VALUES
('JE-001', 1, 4, 10000.00, 'Payment for supplier invoice', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 5 DAY), UTC_TIMESTAMP()),
('JE-002', 7, 3, 5000.00, 'COGS adjustment', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 3 DAY), UTC_TIMESTAMP()),
('JE-003', 2, 6, 7500.00, 'Invoice recognition', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 2 DAY), UTC_TIMESTAMP()),
('JE-004', 1, 7, 3200.00, 'Expense payment', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 1 DAY), UTC_TIMESTAMP()),
('JE-005', 3, 2, 4500.00, 'Inventory adjustment', UTC_TIMESTAMP(), UTC_TIMESTAMP());

-- Insert Sample Tax Rates
INSERT INTO TaxRates (TaxCode, TaxDescription, Rate, TaxType, EffectiveDate, CreatedAt) VALUES
('SALES', 'Sales Tax Rate', 0.0800, 'Sales Tax', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP()),
('FED', 'Federal Income Tax', 0.2100, 'Federal', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP()),
('STATE', 'State Income Tax', 0.0650, 'State', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP()),
('LOCAL', 'Local Sales Tax', 0.0250, 'Local', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP()),
('PAYROLL', 'Payroll Tax Rate', 0.1500, 'Payroll', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP());

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================
-- Run these queries to verify the data was inserted correctly:
-- SELECT COUNT(*) AS total_accounts FROM Accounts;
-- SELECT COUNT(*) AS total_partners FROM Partners;
-- SELECT COUNT(*) AS total_invoices FROM Invoices;
-- SELECT COUNT(*) AS total_payments FROM Payments;
-- SELECT COUNT(*) AS total_journal_entries FROM JournalEntries;
-- SELECT COUNT(*) AS total_tax_rates FROM TaxRates;
-- SELECT COUNT(*) AS total_open_balances FROM OpenBalances;
-- ============================================================================
