# Database Entities

Complete schema definition with detailed table structures, relationships, and SQL definitions.

## Schema Organization

Entities organized into 5 logical schemas:

| Schema     | Purpose                               | Entities                       |
| ---------- | ------------------------------------- | ------------------------------ |
| `identity` | User authentication and authorization | Users, Roles, RefreshTokens    |
| `tenant`   | Multi-tenancy                         | Merchants, Subscriptions       |
| `catalog`  | Menu and product management           | Categories, Products, Toppings |
| `sales`    | Orders and transactions               | Tables, Orders, OrderItems     |
| `billing`  | Billing and payment tracking          | SubscriptionPlans, Invoices    |

## Core Tables

### identity Schema

#### ApplicationUser

```sql
CREATE TABLE [identity].[AspNetUsers] (
    [Id]                    UNIQUEIDENTIFIER    PRIMARY KEY,
    [UserName]              NVARCHAR(256)       UNIQUE NOT NULL,
    [NormalizedUserName]    NVARCHAR(256)       UNIQUE,
    [Email]                 NVARCHAR(256)       UNIQUE,
    [NormalizedEmail]       NVARCHAR(256),
    [EmailConfirmed]        BIT                 DEFAULT 0,
    [PasswordHash]          NVARCHAR(MAX)       NOT NULL,  -- PBKDF2 hashed
    [SecurityStamp]         NVARCHAR(MAX),
    [PhoneNumber]           NVARCHAR(20),
    [PhoneNumberConfirmed]  BIT                 DEFAULT 0,
    [TwoFactorEnabled]      BIT                 DEFAULT 0,
    [LockoutEnd]            DATETIMEOFFSET,
    [LockoutEnabled]        BIT                 DEFAULT 1,
    [AccessFailedCount]     INT                 DEFAULT 0,
    [ConcurrencyStamp]      NVARCHAR(MAX),
    [MerchantId]            UNIQUEIDENTIFIER    FOREIGN KEY REFERENCES [tenant].[Merchants],
    [CreatedAt]             DATETIME2           DEFAULT GETUTCDATE(),
    [UpdatedAt]             DATETIME2,
    [IsDeleted]             BIT                 DEFAULT 0,
    [DeletedAt]             DATETIME2
) ON [PRIMARY];

CREATE INDEX [IX_AspNetUsers_MerchantId] ON [identity].[AspNetUsers] ([MerchantId]);
CREATE INDEX [IX_AspNetUsers_IsDeleted] ON [identity].[AspNetUsers] ([IsDeleted]);
```

**Key Columns:**

- `Id` — User UUID (from ASP.NET Identity)
- `MerchantId` — Links staff to merchant (NULL for SuperAdmin)
- `PasswordHash` — PBKDF2 salted hash (256-bit)
- `AccessFailedCount` — Incremented on failed login
- `LockoutEnd` — If set, user cannot login until this time

#### RefreshToken

```sql
CREATE TABLE [identity].[RefreshTokens] (
    [Id]            UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [UserId]        UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [identity].[AspNetUsers],
    [Token]         NVARCHAR(MAX)       NOT NULL,        -- Base64 encoded random bytes
    [ExpiryDate]    DATETIME2           NOT NULL,        -- 7 days from creation
    [IsRevoked]     BIT                 DEFAULT 0,       -- Set to 1 on refresh/logout
    [CreatedAt]     DATETIME2           DEFAULT GETUTCDATE()
) ON [PRIMARY];

CREATE INDEX [IX_RefreshTokens_UserId] ON [identity].[RefreshTokens] ([UserId]);
CREATE INDEX [IX_RefreshTokens_Token] ON [identity].[RefreshTokens] ([Token]);
```

### tenant Schema

#### Merchant

```sql
CREATE TABLE [tenant].[Merchants] (
    [Id]                        UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [TenantId]                  UNIQUEIDENTIFIER    UNIQUE NOT NULL,  -- Logical tenant ID
    [Name]                      NVARCHAR(255)       NOT NULL,
    [Email]                     NVARCHAR(256)       UNIQUE NOT NULL,
    [PhoneNumber]               NVARCHAR(20),
    [RegistrationDate]          DATETIME2           DEFAULT GETUTCDATE(),
    [IsActive]                  BIT                 DEFAULT 1,
    [SubscriptionId]            UNIQUEIDENTIFIER    FOREIGN KEY REFERENCES [billing].[Subscriptions],
    [RestaurantName]            NVARCHAR(255),
    [Address]                   NVARCHAR(500),
    [City]                      NVARCHAR(100),
    [PostalCode]                NVARCHAR(20),
    [Latitude]                  DECIMAL(10,8),     -- For map integration
    [Longitude]                 DECIMAL(11,8),
    [OpeningTime]               TIME,
    [ClosingTime]               TIME,
    [CreatedAt]                 DATETIME2           DEFAULT GETUTCDATE(),
    [UpdatedAt]                 DATETIME2,
    [IsDeleted]                 BIT                 DEFAULT 0,
    [DeletedAt]                 DATETIME2
) ON [PRIMARY];

CREATE UNIQUE INDEX [UX_Merchants_TenantId] ON [tenant].[Merchants] ([TenantId]);
CREATE INDEX [IX_Merchants_IsActive] ON [tenant].[Merchants] ([IsActive]);
```

**Key Insight:** Each merchant is a unique tenant with isolated data.

### catalog Schema

#### Category

```sql
CREATE TABLE [catalog].[Categories] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [MerchantId]        UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [tenant].[Merchants],
    [Name]              NVARCHAR(100)       NOT NULL,
    [DisplayOrder]      INT                 DEFAULT 0,
    [Description]       NVARCHAR(500),
    [CreatedAt]         DATETIME2           DEFAULT GETUTCDATE(),
    [UpdatedAt]         DATETIME2,
    [IsDeleted]         BIT                 DEFAULT 0,
    [DeletedAt]         DATETIME2,
    [RowVersion]        ROWVERSION          -- For concurrency control
) ON [PRIMARY];

CREATE UNIQUE INDEX [UX_Categories_MerchantId_Name]
    ON [catalog].[Categories] ([MerchantId], [Name])
    WHERE [IsDeleted] = 0;

CREATE INDEX [IX_Categories_MerchantId] ON [catalog].[Categories] ([MerchantId]);
```

#### Product

```sql
CREATE TABLE [catalog].[Products] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [MerchantId]        UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [tenant].[Merchants],
    [CategoryId]        UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [catalog].[Categories],
    [Name]              NVARCHAR(255)       NOT NULL,
    [Description]       NVARCHAR(1000),
    [Price]             DECIMAL(10,2)       NOT NULL,   -- In vendor currency (VND)
    [CostPrice]         DECIMAL(10,2),      -- For margin calculation
    [IsAvailable]       BIT                 DEFAULT 1,
    [DisplayOrder]      INT                 DEFAULT 0,
    [ImageUrl]          NVARCHAR(500),      -- Cloudinary URL
    [PreparationTime]   INT,                -- Minutes
    [Vegetarian]        BIT                 DEFAULT 0,
    [Spicy]             BIT                 DEFAULT 0,
    [Allergens]         NVARCHAR(500),      -- JSON: ["nuts", "gluten"]
    [CreatedAt]         DATETIME2           DEFAULT GETUTCDATE(),
    [UpdatedAt]         DATETIME2,
    [IsDeleted]         BIT                 DEFAULT 0,
    [DeletedAt]         DATETIME2,
    [RowVersion]        ROWVERSION
) ON [PRIMARY];

CREATE UNIQUE INDEX [UX_Products_MerchantId_CategoryId_Name]
    ON [catalog].[Products] ([MerchantId], [CategoryId], [Name])
    WHERE [IsDeleted] = 0;

CREATE INDEX [IX_Products_CategoryId] ON [catalog].[Products] ([CategoryId]);
CREATE INDEX [IX_Products_IsAvailable] ON [catalog].[Products] ([IsAvailable]) INCLUDE ([Price]);
```

#### ProductTopping (Join table)

```sql
CREATE TABLE [catalog].[ProductToppings] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [ProductId]         UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [catalog].[Products],
    [ToppingId]         UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [catalog].[Toppings],
    [MaxAllowed]        INT                 DEFAULT 1,  -- Can add max 1 of this topping
    [IsRequired]        BIT                 DEFAULT 0
) ON [PRIMARY];

CREATE UNIQUE INDEX [UX_ProductToppings_ProductId_ToppingId]
    ON [catalog].[ProductToppings] ([ProductId], [ToppingId]);
```

#### Topping

```sql
CREATE TABLE [catalog].[Toppings] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [MerchantId]        UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [tenant].[Merchants],
    [Name]              NVARCHAR(100)       NOT NULL,
    [Price]             DECIMAL(10,2)       DEFAULT 0,
    [CreatedAt]         DATETIME2           DEFAULT GETUTCDATE(),
    [IsDeleted]         BIT                 DEFAULT 0
) ON [PRIMARY];

CREATE UNIQUE INDEX [UX_Toppings_MerchantId_Name]
    ON [catalog].[Toppings] ([MerchantId], [Name])
    WHERE [IsDeleted] = 0;
```

### sales Schema

#### Table

```sql
CREATE TABLE [sales].[Tables] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [MerchantId]        UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [tenant].[Merchants],
    [TableNumber]       INT                 NOT NULL,
    [QRCode]            NVARCHAR(500),      -- Unique QR code string
    [Capacity]          INT                 NOT NULL,   -- Seats
    [IsOccupied]        BIT                 DEFAULT 0,  -- Currently has active order
    [Location]          NVARCHAR(100),      -- "Main hall", "Patio", etc
    [CreatedAt]         DATETIME2           DEFAULT GETUTCDATE(),
    [IsDeleted]         BIT                 DEFAULT 0
) ON [PRIMARY];

CREATE UNIQUE INDEX [UX_Tables_MerchantId_TableNumber]
    ON [sales].[Tables] ([MerchantId], [TableNumber])
    WHERE [IsDeleted] = 0;

CREATE UNIQUE INDEX [UX_Tables_QRCode] ON [sales].[Tables] ([QRCode]) WHERE [IsDeleted] = 0;
```

#### Order

```sql
CREATE TABLE [sales].[Orders] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [MerchantId]        UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [tenant].[Merchants],
    [TableId]           UNIQUEIDENTIFIER    FOREIGN KEY REFERENCES [sales].[Tables],
    [OrderNumber]       NVARCHAR(50)        NOT NULL,   -- "ORD-2024-0001"
    [Status]            NVARCHAR(50)        NOT NULL,   -- "Open", "Completed", "Cancelled"
    [SubTotal]          DECIMAL(10,2)       DEFAULT 0,  -- Before tax/discounts
    [TaxAmount]         DECIMAL(10,2)       DEFAULT 0,
    [DiscountAmount]    DECIMAL(10,2)       DEFAULT 0,
    [Total]             DECIMAL(10,2)       NOT NULL,   -- Final amount
    [PaymentMethod]     NVARCHAR(50),       -- "Cash", "Card", "QR", "Wallet"
    [Notes]             NVARCHAR(500),
    [CreatedAt]         DATETIME2           DEFAULT GETUTCDATE(),
    [CompletedAt]       DATETIME2,
    [UpdatedAt]         DATETIME2,
    [IsDeleted]         BIT                 DEFAULT 0,
    [DeletedAt]         DATETIME2,
    [RowVersion]        ROWVERSION
) ON [PRIMARY];

CREATE INDEX [IX_Orders_MerchantId] ON [sales].[Orders] ([MerchantId]);
CREATE INDEX [IX_Orders_TableId] ON [sales].[Orders] ([TableId]);
CREATE INDEX [IX_Orders_Status] ON [sales].[Orders] ([Status]);
CREATE INDEX [IX_Orders_CreatedAt] ON [sales].[Orders] ([CreatedAt]);
```

#### OrderItem

```sql
CREATE TABLE [sales].[OrderItems] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [OrderId]           UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [sales].[Orders],
    [ProductId]         UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [catalog].[Products],
    [Quantity]          INT                 NOT NULL,
    [UnitPrice]         DECIMAL(10,2)       NOT NULL,   -- Price at time of order
    [Notes]             NVARCHAR(500),      -- "No onions", etc
    [Status]            NVARCHAR(50)        DEFAULT 'Pending',  -- "Pending", "Preparing", "Ready"
    [CreatedAt]         DATETIME2           DEFAULT GETUTCDATE()
) ON [PRIMARY];

CREATE INDEX [IX_OrderItems_OrderId] ON [sales].[OrderItems] ([OrderId]);
CREATE INDEX [IX_OrderItems_ProductId] ON [sales].[OrderItems] ([ProductId]);
```

#### OrderItemTopping (Join table)

```sql
CREATE TABLE [sales].[OrderItemToppings] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [OrderItemId]       UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [sales].[OrderItems],
    [ToppingId]         UNIQUEIDENTIFIER    NOT NULL,
    [ToppingName]       NVARCHAR(100),      -- Denormalized for history
    [ToppingPrice]      DECIMAL(10,2)       -- Price at time of order
) ON [PRIMARY];

CREATE INDEX [IX_OrderItemToppings_OrderItemId] ON [sales].[OrderItemToppings] ([OrderItemId]);
```

### billing Schema

#### SubscriptionPlan

```sql
CREATE TABLE [billing].[SubscriptionPlans] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [Name]              NVARCHAR(100)       NOT NULL,
    [Description]       NVARCHAR(500),
    [MonthlyPrice]      DECIMAL(10,2)       NOT NULL,
    [Features]          NVARCHAR(MAX),      -- JSON: features array
    [MaxUsers]          INT,                -- NULL = unlimited
    [MaxProducts]       INT,                -- NULL = unlimited
    [MaxTables]         INT,                -- NULL = unlimited
    [IsActive]          BIT                 DEFAULT 1,
    [CreatedAt]         DATETIME2           DEFAULT GETUTCDATE()
) ON [PRIMARY];
```

#### Subscription

```sql
CREATE TABLE [billing].[Subscriptions] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [MerchantId]        UNIQUEIDENTIFIER    NOT NULL UNIQUE FOREIGN KEY REFERENCES [tenant].[Merchants],
    [PlanId]            UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [billing].[SubscriptionPlans],
    [StartDate]         DATE                NOT NULL,
    [EndDate]           DATE,               -- NULL = active, set on cancellation
    [RenewalDate]       DATE,               -- Annual/monthly
    [Status]            NVARCHAR(50)        NOT NULL,   -- "Active", "Cancelled", "Expired"
    [CreatedAt]         DATETIME2           DEFAULT GETUTCDATE(),
    [UpdatedAt]         DATETIME2
) ON [PRIMARY];

CREATE INDEX [IX_Subscriptions_MerchantId] ON [billing].[Subscriptions] ([MerchantId]);
```

#### AuditLog

```sql
CREATE TABLE [tenant].[AuditLogs] (
    [Id]                UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    [UserId]            UNIQUEIDENTIFIER    NOT NULL FOREIGN KEY REFERENCES [identity].[AspNetUsers],
    [MerchantId]        UNIQUEIDENTIFIER    FOREIGN KEY REFERENCES [tenant].[Merchants],
    [Action]            NVARCHAR(100)       NOT NULL,   -- "Product.Created", "Order.Updated"
    [Entity]            NVARCHAR(100)       NOT NULL,   -- Entity type
    [EntityId]          UNIQUEIDENTIFIER    NOT NULL,   -- Record ID
    [OldValues]         NVARCHAR(MAX),      -- JSON of previous state
    [NewValues]         NVARCHAR(MAX),      -- JSON of new state
    [Changes]           NVARCHAR(MAX),      -- What changed
    [IpAddress]         NVARCHAR(45),
    [CreatedAt]         DATETIME2           DEFAULT GETUTCDATE()
) ON [PRIMARY];

CREATE INDEX [IX_AuditLogs_UserId] ON [tenant].[AuditLogs] ([UserId]);
CREATE INDEX [IX_AuditLogs_MerchantId] ON [tenant].[AuditLogs] ([MerchantId]);
CREATE INDEX [IX_AuditLogs_CreatedAt] ON [tenant].[AuditLogs] ([CreatedAt]);
```

## Key Indexes for Performance

### Search Operations

- **Find products by category:** `IX_Products_CategoryId, IsAvailable`
- **Find active orders:** `IX_Orders_Status, CreatedAt`
- **Find occupied tables:** `IX_Tables_IsOccupied`

### Range Queries

- **Orders created today:** `IX_Orders_CreatedAt`
- **Recent audit logs:** `IX_AuditLogs_CreatedAt`

### Foreign Key Navigation

- All foreign key columns indexed automatically

## Views (Optional Optimization)

### v_MerchantStats

```sql
CREATE VIEW [dbo].[v_MerchantStats] AS
SELECT
    m.[Id],
    m.[Name],
    COUNT(DISTINCT o.[Id]) as OrderCount,
    COUNT(DISTINCT t.[Id]) as TableCount,
    COUNT(DISTINCT p.[Id]) as ProductCount,
    SUM(CASE WHEN o.[Status] = 'Completed' THEN o.[Total] ELSE 0 END) as Revenue
FROM [tenant].[Merchants] m
LEFT JOIN [sales].[Orders] o ON m.[Id] = o.[MerchantId] AND o.[IsDeleted] = 0
LEFT JOIN [sales].[Tables] t ON m.[Id] = t.[MerchantId] AND t.[IsDeleted] = 0
LEFT JOIN [catalog].[Products] p ON m.[Id] = p.[MerchantId] AND p.[IsDeleted] = 0
WHERE m.[IsDeleted] = 0
GROUP BY m.[Id], m.[Name];
```

## Related Documentation

- [Database Overview](overview.md) — Schema organization, concepts
- [Multi-Tenancy Design](multi-tenancy.md) — Data isolation strategy
