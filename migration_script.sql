-- 1. Create Tables
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sites]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Sites](
	[SiteId] [int] IDENTITY(1,1) NOT NULL,
	[SiteName] [nvarchar](100) NOT NULL,
	[Location] [nvarchar](250) NULL,
	[CreatedAt] [datetime] NOT NULL DEFAULT (getdate()),
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_Sites] PRIMARY KEY CLUSTERED ([SiteId] ASC)
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Suppliers]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Suppliers](
	[SupplierId] [int] IDENTITY(1,1) NOT NULL,
	[SupplierName] [nvarchar](150) NOT NULL,
	[ContactPerson] [nvarchar](100) NULL,
	[Phone] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[CreatedAt] [datetime] NOT NULL DEFAULT (getdate()),
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([SupplierId] ASC)
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Role] [nvarchar](20) NOT NULL,
	[SiteId] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL DEFAULT (getdate()),
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC)
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Items]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Items](
	[ItemId] [int] IDENTITY(1,1) NOT NULL,
	[ItemName] [nvarchar](100) NOT NULL,
	[Unit] [nvarchar](20) NULL,
	[CurrentQuantity] [int] NOT NULL DEFAULT ((0)),
	[MinimumQuantity] [int] NOT NULL DEFAULT ((0)),
	[SiteId] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL DEFAULT (getdate()),
	[IsActive] [bit] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_Items] PRIMARY KEY CLUSTERED ([ItemId] ASC)
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockTransactions]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[StockTransactions](
	[TransactionId] [int] IDENTITY(1,1) NOT NULL,
	[ItemId] [int] NOT NULL,
	[Type] [nvarchar](10) NOT NULL, -- IN/OUT
	[Quantity] [int] NOT NULL,
	[TransactionDate] [datetime] NOT NULL DEFAULT (getdate()),
	[RecordedByUserId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[SupplierId] [int] NULL,
	[Remarks] [nvarchar](max) NULL,
 CONSTRAINT [PK_StockTransactions] PRIMARY KEY CLUSTERED ([TransactionId] ASC)
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LowStockAlerts]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[LowStockAlerts](
	[AlertId] [int] IDENTITY(1,1) NOT NULL,
	[ItemId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[AlertDate] [datetime] NOT NULL DEFAULT (getdate()),
	[IsResolved] [bit] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_LowStockAlerts] PRIMARY KEY CLUSTERED ([AlertId] ASC)
)
END
GO

-- 2. Add Foreign Keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Sites')
    ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FK_Users_Sites] FOREIGN KEY([SiteId]) REFERENCES [dbo].[Sites] ([SiteId])
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Items_Sites')
    ALTER TABLE [dbo].[Items] ADD CONSTRAINT [FK_Items_Sites] FOREIGN KEY([SiteId]) REFERENCES [dbo].[Sites] ([SiteId])
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Stock_Items')
    ALTER TABLE [dbo].[StockTransactions] ADD CONSTRAINT [FK_Stock_Items] FOREIGN KEY([ItemId]) REFERENCES [dbo].[Items] ([ItemId])
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Stock_Users')
    ALTER TABLE [dbo].[StockTransactions] ADD CONSTRAINT [FK_Stock_Users] FOREIGN KEY([RecordedByUserId]) REFERENCES [dbo].[Users] ([UserId])
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Stock_Sites')
    ALTER TABLE [dbo].[StockTransactions] ADD CONSTRAINT [FK_Stock_Sites] FOREIGN KEY([SiteId]) REFERENCES [dbo].[Sites] ([SiteId])
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Stock_Supplier')
    ALTER TABLE [dbo].[StockTransactions] ADD CONSTRAINT [FK_Stock_Supplier] FOREIGN KEY([SupplierId]) REFERENCES [dbo].[Suppliers] ([SupplierId])
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Alert_Items')
    ALTER TABLE [dbo].[LowStockAlerts] ADD CONSTRAINT [FK_Alert_Items] FOREIGN KEY([ItemId]) REFERENCES [dbo].[Items] ([ItemId])
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Alert_Sites')
    ALTER TABLE [dbo].[LowStockAlerts] ADD CONSTRAINT [FK_Alert_Sites] FOREIGN KEY([SiteId]) REFERENCES [dbo].[Sites] ([SiteId])
GO

-- 3. Create Trigger for Automatic Stock & Alerts
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_AfterStockTransaction')
    DROP TRIGGER [dbo].[trg_AfterStockTransaction]
GO

CREATE TRIGGER [dbo].[trg_AfterStockTransaction]
ON [dbo].[StockTransactions]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Update Item Quantities
    UPDATE i
    SET i.CurrentQuantity = CASE 
        WHEN ins.Type = 'IN' THEN i.CurrentQuantity + ins.Quantity
        WHEN ins.Type = 'OUT' THEN i.CurrentQuantity - ins.Quantity
        ELSE i.CurrentQuantity
    END
    FROM Items i
    INNER JOIN inserted ins ON i.ItemId = ins.ItemId;

    -- Generate Alerts if below minimum
    INSERT INTO LowStockAlerts (ItemId, SiteId, AlertDate, IsResolved)
    SELECT i.ItemId, i.SiteId, GETDATE(), 0
    FROM Items i
    INNER JOIN inserted ins ON i.ItemId = ins.ItemId
    WHERE i.CurrentQuantity < i.MinimumQuantity
      AND NOT EXISTS (
          SELECT 1 FROM LowStockAlerts lsa 
          WHERE lsa.ItemId = i.ItemId AND lsa.IsResolved = 0
      );
END
GO

-- 4. Initial Seed (Wait to add your actual Admin here)
-- INSERT INTO Sites (SiteName, Location) VALUES ('Main Office', 'Kigali HQ');
-- INSERT INTO Users (Username, PasswordHash, FullName, Role, SiteId) VALUES ('admin', '...', 'System Admin', 'Admin', 1);
