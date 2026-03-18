# Tenant Module - Complete Documentation

Multi-tenancy management and merchant data.

---

## Domain Entities

### Merchant

**File:** `src/QRDine.Domain/Tenant/Merchant.cs`

Represents a single tenant (restaurant, cafe, or retail store) within the multi-tenant system.

| Property         | Type      | Description                                        |
| ---------------- | --------- | -------------------------------------------------- |
| `Id`             | `Guid`    | Merchant identifier (tenant ID)                    |
| `Name`           | `string`  | Business name                                      |
| `Slug`           | `string`  | URL-friendly identifier (auto-generated from Name) |
| `LogoUrl`        | `string?` | Cloudinary URL to business logo                    |
| `Description`    | `string?` | Business description/bio                           |
| `Address`        | `string?` | Physical business address                          |
| `PhoneNumber`    | `string?` | Contact phone number                               |
| `Email`          | `string?` | Contact email address                              |
| `SubscriptionId` | `Guid?`   | Reference to active Subscription                   |

**Navigation properties:** `Subscription`, `Categories`, `Products`, `Tables`, `Orders`, `Users`, `Transactions`

---

## Multi-Tenancy Architecture

The Tenant module enables complete data isolation through a three-layer defense strategy:

### Layer 1: Global Query Filters (EF Core Level)

Every tenant-scoped entity has a global query filter applied at DbContext configuration:

```csharp
// Example: Category entity configuration
builder.Entity<Category>()
    .HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId.Value);
```

**What it does:**

- Automatically filters all queries to include only data for the current merchant
- Prevents accidental cross-tenant data access at query execution time
- Applies transparently without explicit filtering in LINQ

**Coverage:**

- ✅ Categories, Products, Tables, Toppings
- ✅ Orders, OrderItems
- ✅ Subscriptions, Transactions
- ✅ All tenant-scoped entities

### Layer 2: Automatic MerchantId Stamping (SaveChangesAsync)

When saving new entities, `ApplicationDbContext` automatically sets `MerchantId` from current user context:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var merchantIdClaim = _currentUserService.MerchantId;

    var entries = ChangeTracker.Entries<IMustHaveMerchant>();
    foreach (var entry in entries.Where(e => e.State == EntityState.Added))
    {
        entry.Entity.MerchantId = merchantIdClaim.Value;
    }

    return await base.SaveChangesAsync(cancellationToken);
}
```

**What it does:**

- Prevents developers from accidentally creating data without MerchantId
- Ensures all newly created entities automatically belong to the current merchant
- No developer action required; automatic on any save

### Layer 3: Handler-Level Ownership Checks

CQRS command handlers verify ownership before modifying data:

```csharp
public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponseDto>
{
    public async Task<CategoryResponseDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

        // Ownership check
        if (category.MerchantId != _currentUserService.MerchantId)
        {
            throw new ForbiddenException("Access denied");
        }

        category.Name = request.Name;
        await _repository.UpdateAsync(category, cancellationToken);
        return _mapper.Map<CategoryResponseDto>(category);
    }
}
```

**What it does:**

- Final security check before any update/delete operation
- Explicitly throws `ForbiddenException` if user attempts to access another merchant's data
- Provides defense-in-depth in case Layer 1 or 2 has a bug

---

## Tenant-Aware Entities

Entities that require multi-tenant isolation implement `IMustHaveMerchant`:

```csharp
public interface IMustHaveMerchant
{
    Guid MerchantId { get; set; }
}
```

**All tenant-scoped entities:**

- **Catalog** — Category, Product, Table, Topping, ToppingGroup, ProductToppingGroup
- **Sales** — Order, OrderItem
- **Billing** — Subscription, Transaction
- **Identity** — ApplicationUser (with MerchantId for staff/merchants)

**Admin/Platform entities (NOT tenant-scoped):**

- Plan
- ApplicationRole
- Permission

---

## MerchantId Resolution Strategy

The system determines `MerchantId` in multiple ways depending on context:

### 1. JWT Claim (Authenticated Users)

For authenticated requests, `merchant_id` claim is extracted from JWT:

```csharp
var merchantId = _currentUserService.MerchantId;
// Returns the MerchantId from JWT claim for Merchant or Staff users
// Returns null for SuperAdmin users
```

**Used for:**

- Management API requests (`/api/v1/management/...`)
- Automatic stamping on SaveChangesAsync
- Authorization checks

### 2. Route Parameter (Public/Storefront)

For public storefront endpoints, MerchantId is passed as route parameter:

```
GET /api/v1/storefront/merchants/{merchantId}/categories
GET /api/v1/storefront/merchants/{merchantId}/products
```

**Used for:**

- Public menu browsing
- No authentication required
- Explicit tenant selection per request

### 3. Header Override (Admin Only)

SuperAdmin can override using `X-Merchant-Id` header:

```
GET /api/v1/admin/merchants/categories
X-Merchant-Id: {merchantId}
```

**Used for:**

- Admin debugging and support
- Cross-tenant data access (admin only)
- Rate limited and logged

---

## Current MerchantId Context

The `ICurrentUserService` provides current tenant context:

```csharp
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? MerchantId { get; }  // Current tenant ID
    IList<string> Roles { get; }
    bool IsAuthenticated { get; }
}
```

**Resolution logic:**

1. If user is `SuperAdmin` → `MerchantId` = `null`
2. If user is `Merchant` → `MerchantId` = User's associated merchant
3. If user is `Staff` → `MerchantId` = User's employer merchant
4. If not authenticated → `MerchantId` = `null`

---

## Isolation Verification

### Test Data Isolation

```csharp
// Setup: Create two merchants
var merchant1 = new Merchant { Name = "Restaurant A" };
var merchant2 = new Merchant { Name = "Restaurant B" };

// Create category under merchant1
var category = new Category { Name = "Appetizers", MerchantId = merchant1.Id };

// Query as merchant2 user
_currentUserService.MerchantId = merchant2.Id;
var categories = await _repository.ListAsync();

// Result: Empty list (category not returned due to global query filter)
Assert.Empty(categories);
```

### Query Filter Active

All LINQ queries automatically include the global filter:

```csharp
// Developer writes simple query
var categories = await _repository.ListAsync();

// EF Core internally generates:
// WHERE IsDeleted = 0 AND MerchantId = {currentMerchantId}

// Data from other merchants is never returned, even by mistake
```

---

## Isolation at Each Layer

| Layer           | Mechanism                                  | When Applied        |
| --------------- | ------------------------------------------ | ------------------- |
| Database        | Schema isolation (one schema per merchant) | Design time         |
| Query Execution | Global query filters                       | Every query         |
| Entity Stamping | Auto-set MerchantId                        | SaveChangesAsync    |
| Business Logic  | Ownership checks                           | Handler execution   |
| API Response    | No sensitive headers leaked                | Response generation |

---

## Merchant Slug Generation

Merchant slugs provide URL-friendly tenant identifiers:

```csharp
// From merchant name "My Restaurant"
// Generated slug: "my-restaurant"

// If slug already exists, add numeric suffix:
// "my-restaurant-1"
// "my-restaurant-2"
```

**Used for:**

- Public storefront URLs
- SEO-friendly merchant identification
- Optional alternative to GUID in URLs

---

## Soft Delete Support

Merchants support soft deletes via `IsDeleted` flag:

- Deleted merchants are excluded by global query filter
- Historical data retention
- Recovery capability
- No permanent data loss

---

**Reference:** See also [Tenant Module Overview](README.md), [Features Overview](../) for other modules, and [Database & Multi-Tenancy](../../database/) for schema implementation.
