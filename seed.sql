-- Create schema if missing (safe to run multiple times)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductId INT IDENTITY(1,1) PRIMARY KEY,
        Sku NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        AvailableStock INT NOT NULL
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ExternalOrderId NVARCHAR(100) NULL,
        PlacedAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETDATE()),
        Status INT NOT NULL DEFAULT(1)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItems')
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Sku NVARCHAR(50) NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
    );
END

-- Create or replace view vDailySalesByProduct (per-product per-day totals)
IF OBJECT_ID('dbo.vDailySalesByProduct', 'V') IS NOT NULL
    DROP VIEW dbo.vDailySalesByProduct;
GO
CREATE VIEW dbo.vDailySalesByProduct
AS
SELECT
    CAST(o.PlacedAt AS date) AS OrderDate,
    p.Sku,
    SUM(oi.Quantity) AS DayTotalQuantity,
    SUM(oi.Quantity * oi.UnitPrice) AS DayTotalGrossAmount,
    COUNT(DISTINCT o.Id) AS DayOrderCount
FROM dbo.Orders o
INNER JOIN dbo.OrderItems oi ON oi.OrderId = o.Id
INNER JOIN dbo.Products p ON p.ProductId = oi.ProductId
GROUP BY CAST(o.PlacedAt AS date), p.Sku;
GO

-- Seed sample products (skip duplicates by SKU)
IF NOT EXISTS(SELECT 1 FROM dbo.Products WHERE Sku = 'SKU-A')
    INSERT INTO dbo.Products (Sku, Name, Price, AvailableStock) VALUES ('SKU-A', 'Alpha Widget', 49.99, 100);
IF NOT EXISTS(SELECT 1 FROM dbo.Products WHERE Sku = 'SKU-B')
    INSERT INTO dbo.Products (Sku, Name, Price, AvailableStock) VALUES ('SKU-B', 'Beta Gadget', 24.99, 200);

-- Insert sample orders (dates inside Sept 2026)
-- Order 1: 2026-09-03
INSERT INTO dbo.Orders (ExternalOrderId, PlacedAt, CreatedAt, Status)
VALUES ('EXT-1001','2026-09-03T10:15:00','2026-09-03T10:15:00', 1);
DECLARE @o1 INT = SCOPE_IDENTITY();
INSERT INTO dbo.OrderItems (OrderId, ProductId, Sku, Quantity, UnitPrice)
SELECT @o1, p.ProductId, p.Sku, 2, p.Price FROM dbo.Products p WHERE p.Sku = 'SKU-A';
INSERT INTO dbo.OrderItems (OrderId, ProductId, Sku, Quantity, UnitPrice)
SELECT @o1, p.ProductId, p.Sku, 1, p.Price FROM dbo.Products p WHERE p.Sku = 'SKU-B';

-- Order 2: 2026-09-10
INSERT INTO dbo.Orders (ExternalOrderId, PlacedAt, CreatedAt, Status)
VALUES ('EXT-1002','2026-09-10T14:20:00','2026-09-10T14:20:00', 1);
DECLARE @o2 INT = SCOPE_IDENTITY();
INSERT INTO dbo.OrderItems (OrderId, ProductId, Sku, Quantity, UnitPrice)
SELECT @o2, p.ProductId, p.Sku, 1, p.Price FROM dbo.Products p WHERE p.Sku = 'SKU-A';