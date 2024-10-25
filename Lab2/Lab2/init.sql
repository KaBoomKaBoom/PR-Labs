-- Check if the Lab2 database exists and create it if it doesn't
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Lab2')
BEGIN
    CREATE DATABASE Lab2;
END
GO

-- Use the Lab2 database
USE Lab2;
GO

-- Check if the Lab2 schema exists and create it if it doesn't
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Lab2')
BEGIN
    CREATE SCHEMA Lab2;
END
GO

-- Check if the Products table exists and create it if it doesn't
IF NOT EXISTS (SELECT * FROM information_schema.tables WHERE table_name = 'Products' AND table_schema = 'Lab2')
BEGIN
    CREATE TABLE Lab2.Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NULL,
        Price DECIMAL(18, 2) NOT NULL,
        Link NVARCHAR(255) NULL,
        Resolution NVARCHAR(255) NULL
    );
END
GO

-- Check if the ProductsFiltered table exists and create it if it doesn't
IF NOT EXISTS (SELECT * FROM information_schema.tables WHERE table_name = 'ProductsFiltered' AND table_schema = 'Lab2')
BEGIN
    CREATE TABLE Lab2.ProductsFiltered (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TotalPrice DECIMAL(18, 2) NOT NULL,
        DateTime DATETIME NOT NULL,
        ProductId INT NOT NULL,
        FOREIGN KEY (ProductId) REFERENCES Lab2.Products(Id) -- Assuming a relationship with Products
    );
END
GO
