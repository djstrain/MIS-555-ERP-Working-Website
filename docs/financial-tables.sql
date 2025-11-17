-- MySQL DDL for Financial Management (8 tables)
-- Target database: rxerp (change with USE if needed)

-- Ensure database exists
CREATE DATABASE IF NOT EXISTS `rxerp` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `rxerp`;

-- 1) Accounts
CREATE TABLE IF NOT EXISTS `Accounts` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `AccountNumber` VARCHAR(50) NOT NULL,
  `AccountName` VARCHAR(100) NOT NULL,
  `AccountType` VARCHAR(50) NOT NULL,
  `Balance` DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_Accounts_AccountNumber` (`AccountNumber`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2) Partners
CREATE TABLE IF NOT EXISTS `Partners` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `PartnerName` VARCHAR(100) NOT NULL,
  `PartnerType` VARCHAR(50) NOT NULL,
  `Email` VARCHAR(255) NULL,
  `Phone` VARCHAR(50) NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3) TaxRates
CREATE TABLE IF NOT EXISTS `TaxRates` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `TaxCode` VARCHAR(50) NOT NULL,
  `TaxDescription` VARCHAR(255) NOT NULL,
  `Rate` DECIMAL(7,4) NOT NULL DEFAULT 0.0000,
  `TaxType` VARCHAR(50) NULL,
  `EffectiveDate` DATETIME NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_TaxRates_TaxCode` (`TaxCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4) Invoices
CREATE TABLE IF NOT EXISTS `Invoices` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `InvoiceNumber` VARCHAR(50) NOT NULL,
  `PartnerId` INT NOT NULL,
  `Amount` DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  `Status` VARCHAR(20) NOT NULL DEFAULT 'Pending',
  `InvoiceDate` DATETIME NULL,
  `DueDate` DATETIME NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_Invoices_InvoiceNumber` (`InvoiceNumber`),
  KEY `IX_Invoices_PartnerId` (`PartnerId`),
  CONSTRAINT `FK_Invoices_Partners` FOREIGN KEY (`PartnerId`) REFERENCES `Partners` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5) InvoiceLines
CREATE TABLE IF NOT EXISTS `InvoiceLines` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `InvoiceId` INT NOT NULL,
  `Description` VARCHAR(200) NOT NULL,
  `Quantity` DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  `UnitPrice` DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  `LineTotal` DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `IX_InvoiceLines_InvoiceId` (`InvoiceId`),
  CONSTRAINT `FK_InvoiceLines_Invoices` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6) Payments
CREATE TABLE IF NOT EXISTS `Payments` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `PaymentNumber` VARCHAR(50) NOT NULL,
  `InvoiceId` INT NOT NULL,
  `PaymentAmount` DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  `PaymentDate` DATETIME NULL,
  `PaymentMethod` VARCHAR(50) NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_Payments_PaymentNumber` (`PaymentNumber`),
  KEY `IX_Payments_InvoiceId` (`InvoiceId`),
  CONSTRAINT `FK_Payments_Invoices` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 7) JournalEntries
CREATE TABLE IF NOT EXISTS `JournalEntries` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `JournalNumber` VARCHAR(50) NOT NULL,
  `DebitAccountId` INT NOT NULL,
  `CreditAccountId` INT NOT NULL,
  `Amount` DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  `Description` VARCHAR(255) NULL,
  `EntryDate` DATETIME NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_JournalEntries_JournalNumber` (`JournalNumber`),
  KEY `IX_JournalEntries_DebitAccountId` (`DebitAccountId`),
  KEY `IX_JournalEntries_CreditAccountId` (`CreditAccountId`),
  CONSTRAINT `FK_JournalEntries_DebitAccount` FOREIGN KEY (`DebitAccountId`) REFERENCES `Accounts` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `FK_JournalEntries_CreditAccount` FOREIGN KEY (`CreditAccountId`) REFERENCES `Accounts` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8) JournalLines
CREATE TABLE IF NOT EXISTS `JournalLines` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `JournalEntryId` INT NOT NULL,
  `AccountId` INT NOT NULL,
  `Debit` DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  `Credit` DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  `Description` VARCHAR(200) NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `IX_JournalLines_JournalEntryId` (`JournalEntryId`),
  KEY `IX_JournalLines_AccountId` (`AccountId`),
  CONSTRAINT `FK_JournalLines_JournalEntries` FOREIGN KEY (`JournalEntryId`) REFERENCES `JournalEntries` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_JournalLines_Accounts` FOREIGN KEY (`AccountId`) REFERENCES `Accounts` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
