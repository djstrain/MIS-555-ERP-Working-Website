-- =====================================================================
-- All SQL Code
-- Database: rxerp (adjust USE statement if different)
-- Purpose: Full DDL for all tables used by the application.
-- Notes:
--   1. Schemas for Tags, ContactTags, Documents were inferred (no C# model definitions present).
--   2. Run in order; parents before children to satisfy foreign keys.
--   3. Uses utf8mb4 + InnoDB. Adjust engine/charset as needed.
--   4. Timestamps use DATETIME (UTC expected from app). Consider TIMESTAMP if preferred.
--   5. Decimal scales chosen based on typical usage; adjust if business rules differ.
-- =====================================================================

USE `rxerp`;

SET FOREIGN_KEY_CHECKS = 0;

-- =========================
-- CORE / SECURITY TABLES
-- =========================
DROP TABLE IF EXISTS `UserCredentials`;
CREATE TABLE `UserCredentials` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `Email` VARCHAR(255) NOT NULL UNIQUE,
  `Password` VARCHAR(255) NOT NULL,
  `Role` VARCHAR(50) NOT NULL DEFAULT 'User',
  `CreatedAt` DATETIME NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =========================
-- EMPLOYEES / HR
-- =========================
DROP TABLE IF EXISTS `Employees`;
CREATE TABLE `Employees` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `Name` VARCHAR(100) NOT NULL,
  `Department` VARCHAR(100) NOT NULL,
  `Role` VARCHAR(100) NOT NULL,
  `Address` VARCHAR(255) NULL,
  `Phone` VARCHAR(20) NULL,
  `Salary` DECIMAL(10,2) NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_Employees_Department` (`Department`),
  KEY `IX_Employees_Role` (`Role`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =========================
-- VENDORS & FILES
-- =========================
DROP TABLE IF EXISTS `Vendors`;
CREATE TABLE `Vendors` (
  `VendorID` INT AUTO_INCREMENT PRIMARY KEY,
  `VendorName` VARCHAR(150) NOT NULL,
  `ContactPerson` VARCHAR(100) NULL,
  `Email` VARCHAR(255) NULL,
  `VendorType` VARCHAR(50) NULL,
  `Status` VARCHAR(50) NOT NULL DEFAULT 'Active',
  `Rating` DECIMAL(3,2) NULL,
  `CreatedAt` DATETIME NOT NULL,
  `UpdatedAt` DATETIME NOT NULL,
  KEY `IX_Vendors_VendorType` (`VendorType`),
  KEY `IX_Vendors_Status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `VendorFiles`;
CREATE TABLE `VendorFiles` (
  `FileID` INT AUTO_INCREMENT PRIMARY KEY,
  `VendorID` INT NOT NULL,
  `OriginalFileName` VARCHAR(260) NOT NULL,
  `StoredFileName` VARCHAR(255) NOT NULL,
  `ContentType` VARCHAR(255) NULL,
  `SizeBytes` BIGINT UNSIGNED NOT NULL,
  `Description` VARCHAR(500) NULL,
  `SHA256` VARCHAR(64) NULL,
  `UploadedAt` DATETIME NOT NULL,
  `UploadedBy` VARCHAR(255) NULL,
  `IsPublic` TINYINT(1) NOT NULL DEFAULT 0,
  CONSTRAINT `FK_VendorFiles_Vendors` FOREIGN KEY (`VendorID`) REFERENCES `Vendors`(`VendorID`) ON DELETE CASCADE ON UPDATE CASCADE,
  KEY `IX_VendorFiles_VendorID` (`VendorID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =========================
-- FINANCIAL MANAGEMENT TABLES
-- =========================
DROP TABLE IF EXISTS `Accounts`;
CREATE TABLE `Accounts` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `AccountNumber` VARCHAR(50) NOT NULL,
  `AccountName` VARCHAR(150) NOT NULL,
  `AccountType` VARCHAR(50) NOT NULL, -- Asset, Liability, Equity, Revenue, Expense
  `Balance` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME NOT NULL,
  UNIQUE KEY `UQ_Accounts_AccountNumber` (`AccountNumber`),
  KEY `IX_Accounts_AccountType` (`AccountType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `Partners`;
CREATE TABLE `Partners` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `PartnerName` VARCHAR(150) NOT NULL,
  `PartnerType` VARCHAR(50) NOT NULL, -- Vendor, Customer, Associate
  `Email` VARCHAR(255) NOT NULL,
  `Phone` VARCHAR(50) NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_Partners_Type` (`PartnerType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `Invoices`;
CREATE TABLE `Invoices` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `InvoiceNumber` VARCHAR(100) NOT NULL,
  `PartnerId` INT NOT NULL,
  `Amount` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `Status` VARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Paid, Overdue
  `InvoiceDate` DATETIME NOT NULL,
  `DueDate` DATETIME NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  UNIQUE KEY `UQ_Invoices_InvoiceNumber` (`InvoiceNumber`),
  KEY `IX_Invoices_PartnerId` (`PartnerId`),
  KEY `IX_Invoices_Status` (`Status`),
  CONSTRAINT `FK_Invoices_Partners` FOREIGN KEY (`PartnerId`) REFERENCES `Partners`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `InvoiceLines`;
CREATE TABLE `InvoiceLines` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `InvoiceId` INT NOT NULL,
  `Description` VARCHAR(200) NOT NULL,
  `Quantity` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `UnitPrice` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `LineTotal` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_InvoiceLines_InvoiceId` (`InvoiceId`),
  CONSTRAINT `FK_InvoiceLines_Invoices` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `OpenBalances`;
CREATE TABLE `OpenBalances` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `AccountId` INT NOT NULL,
  `OpeningBalance` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `BalanceDate` DATETIME NOT NULL,
  `Description` VARCHAR(255) NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_OpenBalances_AccountId` (`AccountId`),
  CONSTRAINT `FK_OpenBalances_Accounts` FOREIGN KEY (`AccountId`) REFERENCES `Accounts`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `Payments`;
CREATE TABLE `Payments` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `PaymentNumber` VARCHAR(100) NOT NULL,
  `InvoiceId` INT NOT NULL,
  `PaymentAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `PaymentDate` DATETIME NOT NULL,
  `PaymentMethod` VARCHAR(50) NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  UNIQUE KEY `UQ_Payments_PaymentNumber` (`PaymentNumber`),
  KEY `IX_Payments_InvoiceId` (`InvoiceId`),
  KEY `IX_Payments_PaymentMethod` (`PaymentMethod`),
  CONSTRAINT `FK_Payments_Invoices` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `JournalEntries`;
CREATE TABLE `JournalEntries` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `JournalNumber` VARCHAR(100) NOT NULL,
  `DebitAccountId` INT NOT NULL,
  `CreditAccountId` INT NOT NULL,
  `Amount` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `Description` VARCHAR(255) NOT NULL,
  `EntryDate` DATETIME NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  UNIQUE KEY `UQ_JournalEntries_Number` (`JournalNumber`),
  KEY `IX_JournalEntries_DebitAccount` (`DebitAccountId`),
  KEY `IX_JournalEntries_CreditAccount` (`CreditAccountId`),
  CONSTRAINT `FK_JournalEntries_DebitAccount` FOREIGN KEY (`DebitAccountId`) REFERENCES `Accounts`(`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `FK_JournalEntries_CreditAccount` FOREIGN KEY (`CreditAccountId`) REFERENCES `Accounts`(`Id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `JournalLines`;
CREATE TABLE `JournalLines` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `JournalEntryId` INT NOT NULL,
  `AccountId` INT NOT NULL,
  `Debit` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `Credit` DECIMAL(18,2) NOT NULL DEFAULT 0,
  `Description` VARCHAR(200) NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_JournalLines_JournalEntryId` (`JournalEntryId`),
  KEY `IX_JournalLines_AccountId` (`AccountId`),
  CONSTRAINT `FK_JournalLines_JournalEntries` FOREIGN KEY (`JournalEntryId`) REFERENCES `JournalEntries`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_JournalLines_Accounts` FOREIGN KEY (`AccountId`) REFERENCES `Accounts`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `TaxRates`;
CREATE TABLE `TaxRates` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `TaxCode` VARCHAR(50) NOT NULL,
  `TaxDescription` VARCHAR(255) NOT NULL,
  `Rate` DECIMAL(6,4) NOT NULL DEFAULT 0, -- e.g. 0.0800 for 8%
  `TaxType` VARCHAR(60) NOT NULL,
  `EffectiveDate` DATETIME NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  UNIQUE KEY `UQ_TaxRates_TaxCode` (`TaxCode`),
  KEY `IX_TaxRates_TaxType` (`TaxType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =========================
-- CRM TABLES
-- =========================
DROP TABLE IF EXISTS `Companies`;
CREATE TABLE `Companies` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `Name` VARCHAR(150) NOT NULL,
  `Industry` VARCHAR(100) NULL,
  `Website` VARCHAR(200) NULL,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_Companies_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `Contacts`;
CREATE TABLE `Contacts` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `CompanyId` INT NULL,
  `FirstName` VARCHAR(80) NOT NULL,
  `LastName` VARCHAR(80) NOT NULL,
  `Email` VARCHAR(150) NULL,
  `Phone` VARCHAR(50) NULL,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_Contacts_CompanyId` (`CompanyId`),
  KEY `IX_Contacts_LastName` (`LastName`),
  KEY `IX_Contacts_Email` (`Email`),
  CONSTRAINT `FK_Contacts_Companies` FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `Opportunities`;
CREATE TABLE `Opportunities` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `CompanyId` INT NOT NULL,
  `Name` VARCHAR(160) NOT NULL,
  `Stage` VARCHAR(40) NULL,
  `Value` DECIMAL(18,2) NULL,
  `CloseDate` DATETIME NULL,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_Opportunities_CompanyId` (`CompanyId`),
  KEY `IX_Opportunities_Stage` (`Stage`),
  CONSTRAINT `FK_Opportunities_Companies` FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `Activities`;
CREATE TABLE `Activities` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `CompanyId` INT NULL,
  `ContactId` INT NULL,
  `ActivityType` VARCHAR(40) NULL,
  `Subject` VARCHAR(160) NULL,
  `ActivityDate` DATETIME NOT NULL,
  `Notes` VARCHAR(500) NULL,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_Activities_CompanyId` (`CompanyId`),
  KEY `IX_Activities_ContactId` (`ContactId`),
  KEY `IX_Activities_ActivityDate` (`ActivityDate`),
  CONSTRAINT `FK_Activities_Companies` FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_Activities_Contacts` FOREIGN KEY (`ContactId`) REFERENCES `Contacts`(`Id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `Notes`;
CREATE TABLE `Notes` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `CompanyId` INT NULL,
  `ContactId` INT NULL,
  `Content` VARCHAR(1000) NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  KEY `IX_Notes_CompanyId` (`CompanyId`),
  KEY `IX_Notes_ContactId` (`ContactId`),
  CONSTRAINT `FK_Notes_Companies` FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_Notes_Contacts` FOREIGN KEY (`ContactId`) REFERENCES `Contacts`(`Id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =========================
-- TAGGING (ASSUMED SCHEMA)
-- =========================
DROP TABLE IF EXISTS `Tags`;
CREATE TABLE `Tags` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `Name` VARCHAR(100) NOT NULL,
  UNIQUE KEY `UQ_Tags_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `ContactTags`;
CREATE TABLE `ContactTags` (
  `ContactId` INT NOT NULL,
  `TagId` INT NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  PRIMARY KEY (`ContactId`,`TagId`),
  CONSTRAINT `FK_ContactTags_Contacts` FOREIGN KEY (`ContactId`) REFERENCES `Contacts`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ContactTags_Tags` FOREIGN KEY (`TagId`) REFERENCES `Tags`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =========================
-- DOCUMENTS (ASSUMED SCHEMA)
-- =========================
DROP TABLE IF EXISTS `Documents`;
CREATE TABLE `Documents` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `OriginalFileName` VARCHAR(260) NOT NULL,
  `StoredFileName` VARCHAR(255) NOT NULL,
  `ContentType` VARCHAR(255) NULL,
  `SizeBytes` BIGINT UNSIGNED NOT NULL,
  `Description` VARCHAR(500) NULL,
  `SHA256` VARCHAR(64) NULL,
  `UploadedAt` DATETIME NOT NULL,
  `UploadedBy` VARCHAR(255) NULL,
  `CompanyId` INT NULL,
  `VendorID` INT NULL,
  KEY `IX_Documents_CompanyId` (`CompanyId`),
  KEY `IX_Documents_VendorID` (`VendorID`),
  CONSTRAINT `FK_Documents_Companies` FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_Documents_Vendors` FOREIGN KEY (`VendorID`) REFERENCES `Vendors`(`VendorID`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

SET FOREIGN_KEY_CHECKS = 1;

-- =====================================================================
-- Optional: Seed data (comment/uncomment as needed). Minimal examples.
-- =====================================================================
-- INSERT INTO `Accounts` (AccountNumber, AccountName, AccountType, Balance, CreatedAt) VALUES
-- ('1000','Cash','Asset',50000,NOW()),
-- ('1100','Accounts Receivable','Asset',25000,NOW());

-- Etc... replicate seeds from EF if desired.
-- =====================================================================
