# Authorization & Permissions

Role-based access control, resource-level authorization, and permission management.

## Role-Based Access Control (RBAC)

Four predefined roles with hierarchical permissions:

| Role           | Description                    | Typical User           | Permissions                   |
| -------------- | ------------------------------ | ---------------------- | ----------------------------- |
| **SuperAdmin** | Platform administrator         | Platform admins        | All operations, all merchants |
| **Merchant**   | Restaurant/store owner/manager | Restaurant owners      | Manage own store, add staff   |
| **Staff**      | Employee at merchant location  | Waiters, kitchen staff | View orders, update statuses  |
| **Guest**      | Anonymous customer             | Diners via QR code     | Read-only public endpoints    |

### Role Hierarchy

Roles are NOT hierarchical in QRDine (unlike some systems):

```csharp
// ❌ SuperAdmin is NOT automatically a Merchant
[Authorize(Roles = "Merchant")]
public async Task<ResponseDto> UpdateCategory(...)
{
    // SuperAdmin cannot call this without explicit permission
}

// ✅ More flexible: Check by merchant ownership OR superadmin
[Authorize]
public async Task<ResponseDto> UpdateCategory(...)
{
    if (user.Role != "Merchant" && user.Role != "SuperAdmin")
        throw new ForbiddenException();
}
```

## Applying Authorization

### Controller-Level Authorization

```csharp
[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "SuperAdmin")]  // All methods require SuperAdmin
public class AdminController : ControllerBase
{
    [HttpGet("merchants")]
    public async Task<IActionResult> GetMerchants() { }

    [HttpPost("merchants")]
    public async Task<IActionResult> CreateMerchant() { }
}
```

### Method-Level Authorization

```csharp
[ApiController]
[Route("api/v1/management")]
[Authorize]  // All methods require authentication
public class ManagementController : ControllerBase
{
    [HttpGet("categories")]
    [Authorize(Roles = "Merchant,Staff")]  // Specific roles
    public async Task<IActionResult> GetCategories() { }

    [HttpPost("categories")]
    [Authorize(Roles = "Merchant")]  // Merchant only
    public async Task<IActionResult> CreateCategory() { }

    [AllowAnonymous]  // Public endpoint
    [HttpGet("public-info")]
    public async Task<IActionResult> GetPublicInfo() { }
}
```

### Multiple Roles

```csharp
// Any of these roles can access
[Authorize(Roles = "Merchant,SuperAdmin")]
public async Task<IActionResult> GetAnalytics() { }

// Comma-separated = OR logic (any role works)
```

## Resource-Level Authorization

Role-based authorization is **necessary but not sufficient**. Verify resource ownership explicitly:

### Example: Update Category

```csharp
public async Task<UpdateCategoryResponse> Handle(
    UpdateCategoryCommand request,
    CancellationToken cancellationToken)
{
    // 1. Authentication check (automatic by [Authorize])
    var currentMerchantId = _currentUserService.MerchantId!.Value;

    // 2. Get resource
    var category = await _categoryRepository.GetByIdAsync(
        request.Id,
        cancellationToken);

    if (category == null)
        throw new NotFoundException("Category", request.Id);

    // 3. Role-based check (could be implicit via data isolation)
    if (!await AuthorizationService.IsAuthorizedAsync(currentUser, "EditCategory", category))
        throw new ForbiddenException();

    // 4. Resource ownership check (CRITICAL!)
    if (category.MerchantId != currentMerchantId)
        throw new ForbiddenException(
            "You do not have permission to modify this category");

    // 5. Update resource
    category.Name = request.Name;
    category.Description = request.Description;
    category.UpdatedAt = DateTime.UtcNow;

    await _categoryRepository.UpdateAsync(category, cancellationToken);

    return new UpdateCategoryResponse
    {
        Id = category.Id,
        Name = category.Name
    };
}
```

### Triple-Layer Defense

1. **Authentication** — User is who they claim (JWT valid)
2. **Role-Based Authorization** — User has required role
3. **Resource-Level Authorization** — User owns/can access resource

```
Request
  ↓
[Authorize(Roles = "Merchant")]
  ↓ (user not authenticated OR missing role)
401/403 Forbidden
  ↓ (pass)
Handler executes
  ↓
if (resource.MerchantId != currentMerchantId)
  ↓ (pass)
403 Forbidden
  ↓ (pass)
Proceed with operation
```

## Data Isolation (Multi-Tenancy Security)

Prevents merchants from accessing other merchants' data:

### Global Query Filters

Automatically applied to all queries:

```csharp
builder.Entity<Category>()
    .HasQueryFilter(e => !e.IsDeleted && e.MerchantId == CurrentMerchantId);

builder.Entity<Product>()
    .HasQueryFilter(e => !e.IsDeleted && e.MerchantId == CurrentMerchantId);

builder.Entity<Order>()
    .HasQueryFilter(e => !e.IsDeleted && e.MerchantId == CurrentMerchantId);
```

**Behavior:**

```sql
-- Executed query:
SELECT * FROM [catalog].[Categories]
WHERE [IsDeleted] = 0
AND [MerchantId] = '550e8400-e29b-41d4-a716-446655440000'

-- Attacker cannot bypass:
SELECT * FROM [catalog].[Categories]
WHERE 1=1;  -- Still filtered by MerchantId in EF Core
```

### Automatic MerchantId Stamping

New entities automatically assigned current merchant:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
{
    var merchantId = _currentUserService.MerchantId;

    if (merchantId.HasValue)
    {
        var entries = ChangeTracker
            .Entries<IMustHaveMerchant>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
            entry.Entity.MerchantId = merchantId.Value;
    }

    return await base.SaveChangesAsync(cancellationToken);
}
```

**Prevents:** Manually assigning MerchantId, bypassing isolation

## Permission Policies

Optional fine-grained permission system (not required for RBAC):

```csharp
// Define policy
services.AddAuthorizationBuilder()
    .AddPolicy("CanDeleteOrder", policy =>
        policy.RequireRole("SuperAdmin")
              .AddRequirements(new CanDeleteOrderRequirement()));

// Use policy
[Authorize(Policy = "CanDeleteOrder")]
public async Task<IActionResult> DeleteOrder(Guid orderId) { }

// Implement requirement
public class CanDeleteOrderRequirement : IAuthorizationRequirement { }

public class CanDeleteOrderHandler : AuthorizationHandler<CanDeleteOrderRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanDeleteOrderRequirement requirement)
    {
        // Check additional conditions (e.g., order status)
        if (context.User.HasClaim("role", "SuperAdmin"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

## Authorization Errors

### 401 Unauthorized

User is not authenticated:

```json
{
  "error": {
    "type": "unauthorized",
    "message": "Authentication required"
  },
  "meta": { "statusCode": 401 }
}
```

**Causes:**

- Missing Authorization header
- Invalid JWT token
- Expired access token

### 403 Forbidden

User is authenticated but not authorized:

```json
{
  "error": {
    "type": "forbidden",
    "message": "You do not have permission to access this resource"
  },
  "meta": { "statusCode": 403 }
}
```

**Causes:**

- Missing required role
- Resource owned by different merchant
- Insufficient permissions

## Checking Authorization Programmatically

```csharp
// Option 1: Check role
if (!User.IsInRole("Merchant"))
    throw new ForbiddenException();

// Option 2: Check claim
var merchantId = User.FindFirst(AppClaimTypes.MerchantId)?.Value;

// Option 3: Inject ICurrentUserService
public class MyHandler
{
    private readonly ICurrentUserService _currentUserService;

    public async Task Handle()
    {
        if (_currentUserService.Role != "Merchant")
            throw new ForbiddenException();
    }
}
```

## Common Authorization Patterns

### Pattern 1: SuperAdmin Override

```csharp
if (currentUser.Role == "SuperAdmin")
    return; // Allow operation

if (resource.MerchantId != currentMerchantId)
    throw new ForbiddenException();
```

### Pattern 2: Owner Check

```csharp
if (resource.CreatedByUserId != currentUser.Id &&
    currentUser.Role != "SuperAdmin")
    throw new ForbiddenException();
```

### Pattern 3: Status-Based Authorization

```csharp
// Can only update order if in certain status
if (order.Status != OrderStatus.Open)
    throw new BusinessRuleException("Cannot modify closed orders");
```

## Related Documentation

- [Authentication & Passwords](authentication.md) — JWT, password security
- [Data Protection](data-protection.md) — Encryption, audit logging
- [API Authentication](../api/authentication.md) — Token usage
