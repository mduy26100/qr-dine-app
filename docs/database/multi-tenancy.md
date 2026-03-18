# Multi-Tenancy Design

Data isolation strategy, global filters, and triple-layer defense for row-level tenant isolation.

## Multi-Tenancy Architecture

QRDine implements **row-level multi-tenancy** where each merchant is a separate tenant with isolated data:

```
Database
├─ Tenant A (Merchant 1)
│  ├─ Categories (filtered where MerchantId = A)
│  ├─ Products (filtered where MerchantId = A)
│  ├─ Orders (filtered where MerchantId = A)
│  └─ Audit Logs (filtered where MerchantId = A)
├─ Tenant B (Merchant 2)
│  ├─ Categories (filtered where MerchantId = B)
│  ├─ Products (filtered where MerchantId = B)
│  ├─ Orders (filtered where MerchantId = B)
│  └─ Audit Logs (filtered where MerchantId = B)
└─ Shared (Billing Plans, not filtered)
```

## Multi-Tenancy Models

| Model              | Isolation    | Data Storage | Cost    | Complexity |
| ------------------ | ------------ | ------------ | ------- | ---------- |
| **Row-level**      | Per merchant | Shared DB    | Lowest  | Medium     |
| **Schema-level**   | Per schema   | Shared DB    | Low     | Medium     |
| **Database-level** | Per DB       | Separate DB  | Highest | Simple     |

**QRDine uses:** Row-level (most cost-efficient, moderate complexity)

## Current User Context

Every request contains the active merchant context:

```csharp
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? MerchantId { get; }
    string? Email { get; }
    string? Role { get; }
}

// Implementation
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid? MerchantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst(AppClaimTypes.MerchantId);

            return Guid.TryParse(claim?.Value, out var merchantId)
                ? merchantId
                : null;
        }
    }
}
```

**Injected into:**

- DbContext (for global filters)
- Handlers (for resource checks)
- Repositories (for queries)

## Layer 1: Global Query Filters

Automatic row-level isolation at query level:

### Filter Configuration

```csharp
// In DbContext configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Categories: Only show records for current merchant
    modelBuilder.Entity<Category>()
        .HasQueryFilter(e => !e.IsDeleted && e.MerchantId == _currentUserService.MerchantId);

    // Products
    modelBuilder.Entity<Product>()
        .HasQueryFilter(e => !e.IsDeleted && e.MerchantId == _currentUserService.MerchantId);

    // Orders
    modelBuilder.Entity<Order>()
        .HasQueryFilter(e => !e.IsDeleted && e.MerchantId == _currentUserService.MerchantId);

    // Toppings
    modelBuilder.Entity<Topping>()
        .HasQueryFilter(e => !e.IsDeleted && e.MerchantId == _currentUserService.MerchantId);

    // Tables
    modelBuilder.Entity<Table>()
        .HasQueryFilter(e => !e.IsDeleted && e.MerchantId == _currentUserService.MerchantId);
}
```

### Applied SQL

When a handler queries products:

```csharp
var products = await _context.Products.ToListAsync();

// Executed SQL (filter automatically applied):
SELECT [p].[Id], [p].[Name], [p].[Price]
FROM [catalog].[Products] AS [p]
WHERE [p].[IsDeleted] = 0
  AND [p].[MerchantId] = '550e8400-e29b-41d4-a716-446655440000'  -- Current merchant
```

### Bypassing Filters (SuperAdmin Only)

```csharp
// SuperAdmin can view any merchant's deleted data if needed
var allCategoriesIncludingDeleted = await _context.Categories
    .IgnoreQueryFilters()  // Bypass global filters temporarily
    .ToListAsync();
```

**Note:** Still requires explicit authorization (`[Authorize(Roles = "SuperAdmin")]`)

## Layer 2: Automatic MerchantId Stamping

Prevents accidental assignment to wrong tenant:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
{
    var merchantId = _currentUserService.MerchantId;

    // Auto-stamp all new multi-tenant entities
    if (merchantId.HasValue)
    {
        var entries = ChangeTracker
            .Entries<IMustHaveMerchant>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity.MerchantId == default(Guid))  // Not already set
                entry.Entity.MerchantId = merchantId.Value;
        }
    }

    return await base.SaveChangesAsync(cancellationToken);
}
```

### Scenario: Preventing Cross-Tenant Assignment

```csharp
// Attacker attempts: POST /api/v1/management/categories
// Request body:
{
  "name": "Desserts",
  "merchantId": "other-merchant-id"  // Trying to assign to different merchant
}

// Application logic:
public async Task Handle(CreateCategoryCommand request)
{
    var category = new Category
    {
        Name = request.Name,
        MerchantId = request.MerchantId  // Attacker's value
    };

    await _context.Categories.AddAsync(category);
    // SaveChangesAsync() stamping:
    // category.MerchantId = current user's merchant  // OVERWRITTEN
}
```

## Layer 3: Explicit Ownership Verification

Final defense for cross-tenant queries:

```csharp
public async Task<UpdateCategoryResponse> Handle(
    UpdateCategoryCommand request,
    CancellationToken cancellationToken)
{
    var currentMerchantId = _currentUserService.MerchantId!.Value;

    // 1. Get category (filter auto-applied)
    var category = await _categoryRepository.GetByIdAsync(
        request.Id,
        cancellationToken);

    if (category == null)
        throw new NotFoundException("Category", request.Id);

    // 2. EXPLICIT ownership check (defensive programming)
    if (category.MerchantId != currentMerchantId)
        throw new ForbiddenException(
            "You do not have permission to modify this category");

    // 3. Update
    category.Name = request.Name;
    await _categoryRepository.UpdateAsync(category, cancellationToken);
}
```

### Defense Depth Analysis

Even if Layer 1 (global filter) had a bug:

```
QueryFilter (Layer 1) — BUG: Filter not applied
  → Returns data from ANY merchant

Layer 2 (Stamping) — Not applicable (read-only scenario)

Layer 3 (Explicit Check) — Catches it!
  → if (category.MerchantId != currentMerchantId)
  → Throws ForbiddenException
```

## Shared Entities

Some entities are shared across merchants (NOT filtered):

### Billing Subscriptions

```csharp
// ❌ NOT filtered by MerchantId
modelBuilder.Entity<SubscriptionPlan>()
    .HasNoQueryFilter();  // Explicit: shared data

modelBuilder.Entity<Subscription>()
    .HasNoQueryFilter();  // Shared (but can be uniquely queried by MerchantId)
```

**Why:** Pricing plans are platform-level (same for all merchants)

### Querying Shared Data

```csharp
// Any authenticated user can see all plans (for comparison)
var plans = await _context.SubscriptionPlans.ToListAsync();

// But only their own subscription
var mySubscription = await _context.Subscriptions
    .FirstOrDefaultAsync(s => s.MerchantId == _currentUserService.MerchantId);
```

## Soft Deletes with Multi-Tenancy

Ensures deleted records remain isolated:

```csharp
// Delete (soft)
category.IsDeleted = true;
category.DeletedAt = DateTime.UtcNow;
await _context.SaveChangesAsync();

// View deleted records (admin feature)
var deletedCategories = await _context.Categories
    .IgnoreQueryFilters()  // Bypass MerchantId filter AND IsDeleted filter
    .Where(c => c.IsDeleted && c.MerchantId == merchantId)  // Re-apply MerchantId filter
    .ToListAsync();
```

## Indexes for Multi-Tenancy

Proper indexes crucial for performance:

```sql
-- ✅ Good: Multi-tenant queries efficient
CREATE INDEX [IX_Categories_MerchantId_IsDeleted]
    ON [catalog].[Categories] ([MerchantId], [IsDeleted]);

-- ✅ Good: Product lookup by merchant and name
CREATE UNIQUE INDEX [UX_Products_MerchantId_CategoryId_Name]
    ON [catalog].[Products] ([MerchantId], [CategoryId], [Name])
    WHERE [IsDeleted] = 0;

-- ❌ Bad: Redundant, doesn't include MerchantId
CREATE INDEX [IX_Categories_IsDeleted]
    ON [catalog].[Categories] ([IsDeleted]);
```

### Index Strategy

Every index includes MerchantId as leading column to ensure partition pruning.

## Tenant Context Resolution

How MerchantId is determined per request:

### 1. From JWT Claim

```csharp
// Most common: from authenticated user's token
var claim = context.User.FindFirst(AppClaimTypes.MerchantId);
merchantId = Guid.Parse(claim.Value);
```

### 2. From Route Parameter

```csharp
// Not used in QRDine (all data filtered by current user)
// But would look like:
// GET /api/v1/merchants/{merchantId}/products
// [Authorize]
// public async Task<IActionResult> GetProducts(Guid merchantId)
// {
//     if (merchantId != _currentUserService.MerchantId && !IsSuperAdmin)
//         return Forbidden();
// }
```

### 3. From Subdomain (Alternative)

```
Subdomain-based routing (not used in QRDine):
- merchant1.qrdine.com → MerchantId = merchant1_uuid
- merchant2.qrdine.com → MerchantId = merchant2_uuid

Extracted in middleware:
public class TenantResolutionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        var subdomain = host.Split('.')[0];

        context.Items[TenantKeys.TenantId] = subdomain;
        await _next(context);
    }
}
```

## SuperAdmin Access

SuperAdmins can access any merchant's data:

```csharp
public override void OnModelCreating(ModelBuilder modelBuilder)
{
    var isSuperAdmin = _currentUserService.Role == "SuperAdmin";

    if (!isSuperAdmin)
    {
        // Apply filters for non-admins
        modelBuilder.Entity<Category>()
            .HasQueryFilter(e => !e.IsDeleted && e.MerchantId == _currentUserService.MerchantId);
    }
    // SuperAdmin: no filters applied
}
```

**Important:** Even SuperAdmins are logged in audit trails

## Testing Multi-Tenancy Isolation

### Unit Test: Verify Filter Applied

```csharp
[Test]
public async Task GetCategories_ShouldOnlyReturnCurrentMerchantCategories()
{
    // Given: Two merchants with categories
    var merchant1 = new Merchant { Id = Guid.NewGuid(), Name = "Restaurant A" };
    var merchant2 = new Merchant { Id = Guid.NewGuid(), Name = "Restaurant B" };

    var category1 = new Category { MerchantId = merchant1.Id, Name = "Appetizers" };
    var category2 = new Category { MerchantId = merchant2.Id, Name = "Desserts" };

    // When: Query as merchant1
    _currentUserService.MerchantId = merchant1.Id;
    var result = await _categoryRepository.GetAllAsync();

    // Then: Only merchant1 categories returned
    Assert.Single(result);
    Assert.Equal(category1.Id, result[0].Id);
}
```

### Integration Test: Verify Explicit Check

```csharp
[Test]
public async Task UpdateCategory_DifferentMerchant_ShouldThrow()
{
    // Given: Category belongs to merchant2
    var category = new Category { MerchantId = merchant2.Id };

    // When: Current user is merchant1
    _currentUserService.MerchantId = merchant1.Id;

    // Then: Should throw ForbiddenException
    var ex = await Assert.ThrowsAsync<ForbiddenException>(
        () => _handler.Handle(new UpdateCategoryCommand { Id = category.Id }));

    Assert.Contains("permission", ex.Message);
}
```

## Migration Notes

### Adding New Multi-Tenant Entity

When creating new entity type:

```csharp
public class MenuTemplate
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }  // ← CRUCIAL
    public string Name { get; set; }
    public bool IsDeleted { get; set; }

    // Relationships
    public Merchant Merchant { get; set; }
}

// In DbContext
modelBuilder.Entity<MenuTemplate>()
    .HasQueryFilter(e => !e.IsDeleted &&
                        e.MerchantId == _currentUserService.MerchantId);
```

**Checklist:**

- [ ] Entity has `MerchantId` property
- [ ] Global query filter configured
- [ ] Indexes include MerchantId
- [ ] Handler verifies ownership
- [ ] Unit/integration tests added

## Related Documentation

- [Database Overview](overview.md) — Schema organization
- [Database Entities](entities.md) — Table definitions
- [Security - Authorization](../security/authorization.md) — Resource ownership checks
