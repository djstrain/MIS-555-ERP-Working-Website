-- Sample data inserts for Financial Management (8 tables)
-- Target database: rxerp

USE `rxerp`;

-- 1) Accounts (at least 5; here 7)
INSERT INTO `Accounts` (`AccountNumber`, `AccountName`, `AccountType`, `Balance`, `CreatedAt`) VALUES
('1000', 'Cash', 'Asset', 50000.00, UTC_TIMESTAMP()),
('1100', 'Accounts Receivable', 'Asset', 25000.00, UTC_TIMESTAMP()),
('1200', 'Inventory', 'Asset', 75000.00, UTC_TIMESTAMP()),
('2000', 'Accounts Payable', 'Liability', -30000.00, UTC_TIMESTAMP()),
('3000', 'Common Stock', 'Equity', 100000.00, UTC_TIMESTAMP()),
('4000', 'Sales Revenue', 'Revenue', 150000.00, UTC_TIMESTAMP()),
('5000', 'Cost of Goods Sold', 'Expense', -45000.00, UTC_TIMESTAMP());

-- 2) Partners (5)
INSERT INTO `Partners` (`PartnerName`, `PartnerType`, `Email`, `Phone`, `CreatedAt`) VALUES
('Tech Solutions Inc', 'Vendor', 'info@techsolutions.com', '(555) 123-4567', UTC_TIMESTAMP()),
('Global Manufacturing Co', 'Customer', 'sales@globalmfg.com', '(555) 234-5678', UTC_TIMESTAMP()),
('Premium Supplies Ltd', 'Vendor', 'order@premium-supplies.com', '(555) 345-6789', UTC_TIMESTAMP()),
('Enterprise Solutions', 'Customer', 'contact@enterprise.com', '(555) 456-7890', UTC_TIMESTAMP()),
('Innovation Partners', 'Associate', 'hello@innovationpartners.com', '(555) 567-8901', UTC_TIMESTAMP());

-- 3) TaxRates (5)
INSERT INTO `TaxRates` (`TaxCode`, `TaxDescription`, `Rate`, `TaxType`, `EffectiveDate`, `CreatedAt`) VALUES
('SALES', 'Sales Tax Rate', 0.0800, 'Sales Tax', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP()),
('FED', 'Federal Income Tax', 0.2100, 'Federal', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP()),
('STATE', 'State Income Tax', 0.0650, 'State', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP()),
('LOCAL', 'Local Sales Tax', 0.0250, 'Local', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP()),
('PAYROLL', 'Payroll Tax Rate', 0.1500, 'Payroll', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 365 DAY), UTC_TIMESTAMP());

-- 4) Invoices (5) - reference Partners by name to resolve PartnerId
INSERT INTO `Invoices` (`InvoiceNumber`, `PartnerId`, `Amount`, `Status`, `InvoiceDate`, `DueDate`, `CreatedAt`)
VALUES
('INV-001', (SELECT Id FROM `Partners` WHERE PartnerName = 'Global Manufacturing Co'), 5000.00, 'Paid', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 30 DAY), DATE_SUB(UTC_TIMESTAMP(), INTERVAL 5 DAY), UTC_TIMESTAMP()),
('INV-002', (SELECT Id FROM `Partners` WHERE PartnerName = 'Enterprise Solutions'), 7500.00, 'Pending', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 15 DAY), DATE_ADD(UTC_TIMESTAMP(), INTERVAL 15 DAY), UTC_TIMESTAMP()),
('INV-003', (SELECT Id FROM `Partners` WHERE PartnerName = 'Global Manufacturing Co'), 3200.00, 'Pending', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 10 DAY), DATE_ADD(UTC_TIMESTAMP(), INTERVAL 20 DAY), UTC_TIMESTAMP()),
('INV-004', (SELECT Id FROM `Partners` WHERE PartnerName = 'Enterprise Solutions'), 8900.00, 'Paid', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 45 DAY), DATE_SUB(UTC_TIMESTAMP(), INTERVAL 20 DAY), UTC_TIMESTAMP()),
('INV-005', (SELECT Id FROM `Partners` WHERE PartnerName = 'Global Manufacturing Co'), 4500.00, 'Overdue', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 60 DAY), DATE_SUB(UTC_TIMESTAMP(), INTERVAL 15 DAY), UTC_TIMESTAMP());

-- 5) InvoiceLines (>=5; here 7) - reference Invoices by InvoiceNumber
INSERT INTO `InvoiceLines` (`InvoiceId`, `Description`, `Quantity`, `UnitPrice`, `LineTotal`, `CreatedAt`) VALUES
((SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-001'), 'Consulting Services', 10.00, 250.00, 2500.00, UTC_TIMESTAMP()),
((SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-001'), 'Software Licenses', 5.00, 500.00, 2500.00, UTC_TIMESTAMP()),
((SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-002'), 'Integration Work', 15.00, 300.00, 4500.00, UTC_TIMESTAMP()),
((SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-002'), 'Support Retainer', 2.00, 1500.00, 3000.00, UTC_TIMESTAMP()),
((SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-003'), 'Implementation Hours', 8.00, 250.00, 2000.00, UTC_TIMESTAMP()),
((SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-004'), 'Annual Subscription', 1.00, 8900.00, 8900.00, UTC_TIMESTAMP()),
((SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-005'), 'Maintenance', 6.00, 250.00, 1500.00, UTC_TIMESTAMP());

-- 6) Payments (5) - reference Invoices by InvoiceNumber
INSERT INTO `Payments` (`PaymentNumber`, `InvoiceId`, `PaymentAmount`, `PaymentDate`, `PaymentMethod`, `CreatedAt`) VALUES
('PAY-001', (SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-001'), 5000.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 20 DAY), 'Wire', UTC_TIMESTAMP()),
('PAY-002', (SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-004'), 8900.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 30 DAY), 'Check', UTC_TIMESTAMP()),
('PAY-003', (SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-001'), 5000.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 5 DAY), 'Credit Card', UTC_TIMESTAMP()),
('PAY-004', (SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-002'), 3750.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 2 DAY), 'Bank Transfer', UTC_TIMESTAMP()),
('PAY-005', (SELECT Id FROM `Invoices` WHERE InvoiceNumber = 'INV-004'), 8900.00, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 10 DAY), 'Cash', UTC_TIMESTAMP());

-- 7) JournalEntries (5) - reference Accounts by AccountNumber
INSERT INTO `JournalEntries` (`JournalNumber`, `DebitAccountId`, `CreditAccountId`, `Amount`, `Description`, `EntryDate`, `CreatedAt`) VALUES
('JE-001', (SELECT Id FROM `Accounts` WHERE AccountNumber = '1000'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '2000'), 10000.00, 'Payment for supplier invoice', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 5 DAY), UTC_TIMESTAMP()),
('JE-002', (SELECT Id FROM `Accounts` WHERE AccountNumber = '5000'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '1200'), 5000.00, 'COGS adjustment', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 3 DAY), UTC_TIMESTAMP()),
('JE-003', (SELECT Id FROM `Accounts` WHERE AccountNumber = '1100'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '4000'), 7500.00, 'Invoice recognition', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 2 DAY), UTC_TIMESTAMP()),
('JE-004', (SELECT Id FROM `Accounts` WHERE AccountNumber = '1000'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '5000'), 3200.00, 'Expense payment', DATE_SUB(UTC_TIMESTAMP(), INTERVAL 1 DAY), UTC_TIMESTAMP()),
('JE-005', (SELECT Id FROM `Accounts` WHERE AccountNumber = '1200'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '1100'), 4500.00, 'Inventory adjustment', UTC_TIMESTAMP(), UTC_TIMESTAMP());

-- 8) JournalLines (>=5; here 10) - split entries into debit/credit rows
INSERT INTO `JournalLines` (`JournalEntryId`, `AccountId`, `Debit`, `Credit`, `Description`, `CreatedAt`) VALUES
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-001'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '1000'), 10000.00, 0.00, 'Cash Debited', UTC_TIMESTAMP()),
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-001'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '2000'), 0.00, 10000.00, 'AP Credited', UTC_TIMESTAMP()),
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-002'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '5000'), 5000.00, 0.00, 'COGS Debited', UTC_TIMESTAMP()),
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-002'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '1200'), 0.00, 5000.00, 'Inventory Credited', UTC_TIMESTAMP()),
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-003'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '1100'), 7500.00, 0.00, 'AR Debited', UTC_TIMESTAMP()),
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-003'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '4000'), 0.00, 7500.00, 'Revenue Credited', UTC_TIMESTAMP()),
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-004'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '1000'), 0.00, 3200.00, 'Cash Credited', UTC_TIMESTAMP()),
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-004'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '5000'), 3200.00, 0.00, 'Expense Debited', UTC_TIMESTAMP()),
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-005'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '1200'), 0.00, 4500.00, 'Inventory Credited', UTC_TIMESTAMP()),
((SELECT Id FROM `JournalEntries` WHERE JournalNumber = 'JE-005'), (SELECT Id FROM `Accounts` WHERE AccountNumber = '1100'), 4500.00, 0.00, 'AR Debited', UTC_TIMESTAMP());
