-- Inventory DDL and seed data for rxerp
CREATE TABLE IF NOT EXISTS InventoryItems (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  SKU VARCHAR(64) NOT NULL,
  Name VARCHAR(255) NOT NULL,
  Category VARCHAR(128) NULL,
  QuantityOnHand INT NOT NULL DEFAULT 0,
  ReorderLevel INT NOT NULL DEFAULT 0,
  UnitCost DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  UnitPrice DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  Location VARCHAR(128) NULL,
  CreatedAt DATETIME NULL,
  UpdatedAt DATETIME NULL
);

INSERT INTO InventoryItems (SKU, Name, Category, QuantityOnHand, ReorderLevel, UnitCost, UnitPrice, Location, CreatedAt, UpdatedAt) VALUES
('SKU-1001','Laser Printer','Office Equipment',12,3,199.99,249.99,'Aisle 1',NOW(),NOW()),
('SKU-1002','Ink Cartridge XL','Supplies',50,20,24.50,39.99,'Aisle 2',NOW(),NOW()),
('SKU-1003','USB-C Cable 1m','Accessories',120,40,3.20,9.99,'Aisle 3',NOW(),NOW()),
('SKU-1004','Office Chair Ergonomic','Furniture',8,2,89.00,149.00,'Aisle 4',NOW(),NOW()),
('SKU-1005','Monitor 27" 4K','Electronics',15,5,229.00,329.00,'Aisle 5',NOW(),NOW());
