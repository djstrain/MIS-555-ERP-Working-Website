-- Complete Database Setup for MIS-555 ERP System
-- Run this script to create all tables and sample data

-- Create the rxerp database if it doesn't exist
CREATE DATABASE IF NOT EXISTS rxerp;
USE rxerp;

-- ========================================
-- 1. UserCredentials Table
-- ========================================
CREATE TABLE IF NOT EXISTS UserCredentials (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Insert sample users
INSERT IGNORE INTO UserCredentials (Email, Password, Role, CreatedAt) VALUES
('admin@ctrlfreak.com', 'AdminPassword123!', 'Admin', NOW()),
('jane.doe@ctrlfreak.com', 'JanePassword123!', 'User', NOW()),
('john.smith@ctrlfreak.com', 'JohnPassword123!', 'User', NOW());

-- ========================================
-- 2. Employees Table
-- ========================================
CREATE TABLE IF NOT EXISTS Employees (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Department VARCHAR(100) NOT NULL,
    Role VARCHAR(100) NOT NULL,
    Address VARCHAR(255),
    Phone VARCHAR(20),
    Salary DECIMAL(10, 2) NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Insert sample employees
INSERT IGNORE INTO Employees (Name, Department, Role, Address, Phone, Salary) VALUES
('Jane Doe', 'Human Resources', 'HR Manager', '123 Main St, New York, NY', '555-0101', 75000.00),
('John Smith', 'IT', 'Software Developer', '456 Oak Ave, Boston, MA', '555-0102', 85000.00),
('Alex Lee', 'Finance', 'Financial Analyst', '789 Pine Rd, Chicago, IL', '555-0103', 70000.00),
('Sarah Chen', 'Marketing', 'Marketing Specialist', '321 Elm St, Seattle, WA', '555-0104', 65000.00),
('David Brown', 'Operations', 'Operations Manager', '654 Maple Dr, Austin, TX', '555-0105', 80000.00),
('Maria Garcia', 'Sales', 'Sales Representative', '987 Cedar Ln, Miami, FL', '555-0106', 60000.00),
('Michael Clark', 'IT', 'System Administrator', '147 Birch Ct, Denver, CO', '555-0107', 72000.00);

-- ========================================
-- 3. Vendors Table
-- ========================================
CREATE TABLE IF NOT EXISTS Vendors (
    VendorID INT AUTO_INCREMENT PRIMARY KEY,
    VendorName VARCHAR(150) NOT NULL,
    ContactPerson VARCHAR(100),
    Email VARCHAR(255),
    VendorType VARCHAR(50),
    Status VARCHAR(50) DEFAULT 'Active',
    Rating DECIMAL(3, 2),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Insert sample vendors
INSERT IGNORE INTO Vendors (VendorName, ContactPerson, Email, VendorType, Status, Rating) VALUES
('Tech Solutions Inc', 'John Anderson', 'john@techsolutions.com', 'Technology', 'Active', 4.50),
('Office Supplies Co', 'Sarah Miller', 'sarah@officesupplies.com', 'Supplies', 'Active', 4.20),
('Global Logistics LLC', 'Michael Chen', 'mchen@globallogistics.com', 'Logistics', 'Active', 4.75),
('Cloud Services Group', 'Emily Rodriguez', 'emily@cloudservices.com', 'Technology', 'Active', 4.80),
('Maintenance Masters', 'David Wilson', 'dwilson@maintenancemasters.com', 'Services', 'Active', 3.90),
('Marketing Agency Pro', 'Jennifer Lee', 'jlee@marketingpro.com', 'Marketing', 'Active', 4.60),
('Legal Advisors Inc', 'Robert Taylor', 'rtaylor@legaladvisors.com', 'Legal', 'Inactive', 4.30),
('IT Consulting Experts', 'Amanda White', 'awhite@itconsulting.com', 'Technology', 'Active', 4.95);

-- ========================================
-- 4. VendorFiles Table (attachments per vendor)
-- ========================================
CREATE TABLE IF NOT EXISTS VendorFiles (
    FileID INT AUTO_INCREMENT PRIMARY KEY,
    VendorID INT NOT NULL,
    OriginalFileName VARCHAR(260) NOT NULL,
    StoredFileName VARCHAR(255) NOT NULL,
    ContentType VARCHAR(255),
    SizeBytes BIGINT UNSIGNED NOT NULL,
    Description VARCHAR(500),
    SHA256 CHAR(64),
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UploadedBy VARCHAR(255),
    IsPublic TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT fk_VendorFiles_Vendors
        FOREIGN KEY (VendorID) REFERENCES Vendors(VendorID)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT ux_VendorFiles_StoredFileName UNIQUE (StoredFileName)
) ENGINE=InnoDB;

-- Helpful indexes
CREATE INDEX IF NOT EXISTS ix_VendorFiles_VendorID_UploadedAt ON VendorFiles (VendorID, UploadedAt DESC);
CREATE INDEX IF NOT EXISTS ix_VendorFiles_SHA256 ON VendorFiles (SHA256);

-- ========================================
-- Display Results
-- ========================================
SELECT 'Database setup complete!' AS Status;
SELECT COUNT(*) AS UserCount FROM UserCredentials;
SELECT COUNT(*) AS EmployeeCount FROM Employees;
SELECT COUNT(*) AS VendorCount FROM Vendors;
SELECT COUNT(*) AS VendorFileCount FROM VendorFiles;
