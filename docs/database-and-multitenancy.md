# Database & Multi-Tenancy

This document describes the database design, Entity Framework Core configuration, multi-tenancy strategy, and data seeding in QRDine.

---

## Database Provider

- **SQL Server** (configured via `DefaultConnection` in `appsettings.json`)
- **Entity Framework Core 8** as ORM
- Code-First migrations stored in `src/QRDine.Infrastructure/Persistence/Migrations/`

---

## Schema Organization

The database is partitioned into four logical schemas to separate domain concerns:

| Schema | Tables | Module |
|--------|--------|--------|
| `identity` | Users, Roles, UserClaims, UserRoles, UserLogins, RoleClaims, UserTokens, RefreshTokens, Permissions, RolePermissions | ASP.NET Core Identity |
| `tenant` | Merchants | Tenant management |
| `catalog` | Categories, Products, Tables, ToppingGroups, Toppings, ProductToppingGroups | Menu & restaurant structure |
| `sales` | Orders, OrderItems | Order management |

Schema names are defined as constants in `src/QRDine.Infrastructure/Persistence/Constants/SchemaNames.cs`:

```csharp
internal class SchemaNames
{
    public const string Identity = "identity";
    public const string Tenant = "tenant";
    public const string Catalog = "catalog";
    public const string Sales = "sales";
}
```

---

## Multi-Tenancy Strategy

QRDine implements **data isolation at the row level** using a combination of three mechanisms:

### 1. Global Query Filters

Applied in `src/QRDine.Infrastructure/Persistence/ApplicationDbContext.cs` during `OnModelCreating`. Every tenant-scoped entity is automatically filtered by the current user's `MerchantId`:

```csharp
builder.Entity<Category>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
builder.Entity<Product>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
builder.Entity<Table>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
builder.Entity<Order>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
builder.Entity<ToppingGroup>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
```

**Behavior:**
- When `CurrentMerchantId` is `null` (e.g., SuperAdmin or unauthenticated requests), the filter passes all records.
- When `CurrentMerchantId` has a value, only records belonging to that merchant are returned.
- `CurrentMerchantId` is resolved from the `ICurrentUserService`, which reads the `merchant_id` claim from the JWT token.

### 2. Automatic MerchantId Stamp

On every `SaveChangesAsync()`, the `ApplicationDbContext` intercepts all newly added entities implementing `IMustHaveMerchant` and automatically assigns the current user's `MerchantId`:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var merchantId = _currentUserService.MerchantId;

    if (merchantId.HasValue)
    {
        var entries = ChangeTracker.Entries<IMustHaveMerchant>()
                                   .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            entry.Entity.MerchantId = merchantId.Value;
        }
    }

    // ... SaveChanges with concurrency handling
}
```

This means **handlers never need to manually set `MerchantId`** on new entities — it is applied transparently by the DbContext.

### 3. Handler-Level Ownership Checks

For operations like `UpdateCategory` and `DeleteCategory`, the handlers explicitly verify that the entity's `MerchantId` matches the current user's merchant:

```csharp
if (existingCategory.MerchantId != merchantId)
    throw new ForbiddenException("You do not have permission to delete this category.");
```

---

## Entity Configurations

All EF Core entity configurations use the Fluent API and are applied explicitly in `OnModelCreating`. Configuration files are located in:

```
src/QRDine.Infrastructure/Persistence/Configurations/
├── Catalog/
│   ├── CategoryConfiguration.cs
│   ├── ProductConfiguration.cs
│   ├── ProductToppingGroupConfiguration.cs
│   ├── TableConfiguration.cs
│   ├── ToppingConfiguration.cs
│   └── ToppingGroupConfiguration.cs
├── Identity/
│   ├── PermissionConfiguration.cs
│   ├── RefreshTokenConfiguration.cs
│   └── RolePermissionConfiguration.cs
├── Sales/
│   ├── OrderConfiguration.cs
│   └── OrderItemConfiguration.cs
└── Tenant/
    └── MerchantConfiguration.cs
```

---

## DbSet Registrations

Defined in `src/QRDine.Infrastructure/Persistence/ApplicationDbContext.cs`:

```csharp
// Identity
public DbSet<RefreshToken> RefreshTokens { get; set; }
public DbSet<Permission> Permissions { get; set; }
public DbSet<RolePermission> RolePermissions { get; set; }

// Tenant
public DbSet<Merchant> Merchants { get; set; }

// Catalog
public DbSet<Category> Categories { get; set; }
public DbSet<Product> Products { get; set; }
public DbSet<Table> Tables { get; set; }
public DbSet<ProductToppingGroup> ProductToppingGroups { get; set; }
public DbSet<ToppingGroup> ToppingGroups { get; set; }
public DbSet<Topping> Toppings { get; set; }

// Sales
public DbSet<Order> Orders { get; set; }
public DbSet<OrderItem> OrderItems { get; set; }
```

---

## Transaction Management

The `ApplicationDbContext` implements `IApplicationDbContext.BeginTransactionAsync()` which returns an `IDatabaseTransaction` wrapper around EF Core's `IDbContextTransaction`:

```csharp
public async Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
{
    var transaction = await Database.BeginTransactionAsync(cancellationToken);
    return new DatabaseTransaction(transaction);
}
```

Handlers use explicit transaction management for multi-step operations (e.g., `CreateCategoryCommandHandler`, `UpdateCategoryCommandHandler`, `RegisterService.RegisterMerchantAsync`).

---

## Concurrency Handling

`SaveChangesAsync` catches `DbUpdateConcurrencyException` and wraps it in a custom `ConcurrencyException`:

```csharp
catch (DbUpdateConcurrencyException)
{
    throw new ConcurrencyException("The data has been changed by someone else. Please try again.");
}
```

---

## Data Seeding

On application startup, `IdentitySeeder` (invoked via `SeedDataAsync()` in `Program.cs`) seeds:

1. **System Roles** — `SuperAdmin`, `Merchant`, `Staff`, `Guest`
2. **SuperAdmin User** — Email: `admin@qrdine.com`, Username: `superadmin`

Defined in `src/QRDine.Infrastructure/Persistence/Seeding/IdentitySeeder.cs`.

---

## Migration History

| Migration | Description |
|-----------|-------------|
| `20260222115111_InitialCreate` | Initial database schema |
| `20260222122524_InitTenantAndCatalogSchema` | Add Tenant and Catalog schemas |
| `20260222131228_AddSalesSchema` | Add Sales schema (Orders, OrderItems) |
| `20260223163951_AddCategoryHierarchy` | Add parent-child category support |
| `20260223171424_AddCategoryCompositeIndexes` | Add composite indexes for categories |
| `20260224062428_AddToppingEntities` | Add Topping, ToppingGroup, ProductToppingGroup |

---

## Generic Repository Pattern

The repository layer uses the **Ardalis.Specification** library:

- **`IRepository<T>`** (in `src/QRDine.Application.Common/Abstractions/Persistence/IRepository.cs`) extends `IRepositoryBase<T>` from Ardalis.
- **`Repository<T>`** (in `src/QRDine.Infrastructure/Persistence/Repository.cs`) extends `RepositoryBase<T>` and provides the `ApplicationDbContext` and `DbSet<T>`.
- Feature-specific repositories (e.g., `ICategoryRepository`, `IProductRepository`) extend `IRepository<T>` with domain-specific methods.
