# Tenant Module

Multi-tenancy management and merchant data.

## Quick Overview

The Tenant module manages merchant organization and complete data isolation within the multi-tenant system.

## Key Features

- ✅ Merchant (restaurant) profile and settings
- ✅ Complete data isolation per merchant
- ✅ Subscription association
- ✅ Automatic MerchantId stamping
- ✅ Automatic data filtering by merchant

## Entities

| Entity       | Purpose                                       |
| ------------ | --------------------------------------------- |
| **Merchant** | Tenant organization (restaurant, cafe, store) |

## Merchant Properties

| Property         | Type      | Purpose                         |
| ---------------- | --------- | ------------------------------- |
| `Id`             | `Guid`    | Merchant identifier (tenant ID) |
| `Name`           | `string`  | Business name                   |
| `Slug`           | `string`  | URL-friendly identifier         |
| `LogoUrl`        | `string?` | Business logo                   |
| `Description`    | `string?` | Business description            |
| `Address`        | `string?` | Physical address                |
| `PhoneNumber`    | `string?` | Contact phone                   |
| `Email`          | `string?` | Contact email                   |
| `SubscriptionId` | `Guid?`   | Active subscription reference   |

## Use Cases

1. **Restaurant** registers as merchant (tenant)
2. **System** creates Merchant entity with unique ID
3. **Merchant** creates categories, products, tables
4. **System** automatically associates all data with MerchantId
5. **System** filters all queries by MerchantId (automatic)
6. **Data** is completely isolated from other merchants

## Multi-Tenancy Implementation

The Tenant module enables three-layer data isolation:

### Layer 1: Global Query Filters

All tenant-scoped entities have global query filters at EF Core level:

```csharp
builder.Entity<Category>()
    .HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
```

### Layer 2: Automatic MerchantId Stamping

`ApplicationDbContext.SaveChangesAsync` automatically sets MerchantId on new entities implementing `IMustHaveMerchant`:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var entries = ChangeTracker.Entries<IMustHaveMerchant>();
    foreach (var entry in entries.Where(e => e.State == EntityState.Added))
    {
        entry.Entity.MerchantId = _currentUserService.MerchantId;
    }
    return await base.SaveChangesAsync(cancellationToken);
}
```

### Layer 3: Handler-Level Ownership Checks

CQRS handlers verify ownership before updates/deletes:

```csharp
var category = await _repository.GetByIdAsync(command.Id);
if (category.MerchantId != _currentUserService.MerchantId)
{
    throw new ForbiddenException("Access denied");
}
```

## Tenant-Aware Entities

All entities implementing `IMustHaveMerchant` are automatically tenant-scoped:

```csharp
public interface IMustHaveMerchant
{
    Guid MerchantId { get; set; }
}
```

**Entities:**

- Category
- Product
- Table
- Order
- OrderItem
- Topping
- ToppingGroup
- Subscription

## API Endpoints

| Method | Path                           | Auth  | Purpose              |
| ------ | ------------------------------ | ----- | -------------------- |
| `GET`  | `/api/v1/admin/merchants`      | Admin | List all merchants   |
| `POST` | `/api/v1/admin/merchants`      | Admin | Create merchant      |
| `GET`  | `/api/v1/admin/merchants/{id}` | Admin | Get merchant details |
| `PUT`  | `/api/v1/admin/merchants/{id}` | Admin | Update merchant      |

## MerchantId Resolution

The system resolves `MerchantId` from:

1. **JWT claim** `merchant_id` — Primary source for authenticated requests
2. **Route parameter** `{merchantId}` — For public (storefront) endpoints
3. **Header** `X-Merchant-Id` — Alternative override (admin only)

## Documentation

- **[Complete Tenant Module Documentation](tenant-module.md)** — Full documentation with isolation details

---

**Reference:** See also [Features Overview](../) for other modules and [Database & Multi-Tenancy](../../database/) for schema details.
