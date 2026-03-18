# Data Protection

Input validation, encryption, TLS, audit logging, and data privacy measures.

## Input Validation

Validates all incoming data to prevent injection attacks and ensure data integrity.

### Validation Layer Strategy

```csharp
// Request DTO
public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; }

    [Range(0, 1, ErrorMessage = "Active must be true or false")]
    public bool Active { get; set; }
}

// Controller - automatic validation
[ApiController]
public class CategoriesController
{
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request)  // ← Automatic validation
    {
        // request is guaranteed valid or ModelState.IsValid = false
        if (!ModelState.IsValid)
            return BadRequest(GetValidationErrors(ModelState));
    }
}
```

### Custom Validators

```csharp
public class CategoryNameMustBeUniqueValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly ICategoryRepository _repository;

    public CategoryNameMustBeUniqueValidator(ICategoryRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 100).WithMessage("Name must be 2-100 characters")
            .MustAsync(async (name, ct) =>
            {
                var exists = await _repository.ExistsAsync(name, ct);
                return !exists;
            })
            .WithMessage("Category name already exists");
    }
}
```

### Validation Error Response

```json
{
  "error": {
    "type": "validation_error",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "Name",
        "message": "Name must be between 2 and 100 characters"
      },
      {
        "field": "Email",
        "message": "Email is not in valid format"
      }
    ]
  },
  "meta": { "statusCode": 400 }
}
```

## SQL Injection Prevention

All queries use **parameterized queries** (not string concatenation):

### ✅ Safe: Parameterized Queries

```csharp
// Using Entity Framework Core (parameterized by design)
var categories = await _db.Categories
    .Where(c => c.Name == userInput)
    .ToListAsync();

// Generated SQL (parameterized):
// SELECT [c].[Id], [c].[Name] FROM [catalog].[Categories] AS [c]
// WHERE [c].[Name] = @__userInput_0
```

### ❌ Dangerous: String Concatenation

```csharp
// Never do this!
var query = $"SELECT * FROM Categories WHERE Name = '{userInput}'";
var categories = _db.Categories.FromSqlRaw(query).ToList();

// If userInput = "'; DROP TABLE Categories; --"
// Query becomes: SELECT * FROM Categories WHERE Name = ''; DROP TABLE Categories; --'
```

### Raw SQL (When Necessary)

If raw SQL is required, use `FromSqlInterpolated()`:

```csharp
string searchName = userInput;
var categories = await _db.Categories
    .FromSqlInterpolated(
        $"SELECT * FROM [catalog].[Categories] WHERE [Name] LIKE '%' + {searchName} + '%'")
    .ToListAsync();
```

Parameters are automatically escaped by EF Core.

## Encryption at Rest

Sensitive data encrypted before storage:

### PII Encryption

```csharp
public class User
{
    public Guid Id { get; set; }

    [Encrypted]  // Custom attribute
    public string Email { get; set; }

    [Encrypted]
    public string PhoneNumber { get; set; }

    public string PasswordHash { get; set; }  // Not encrypted (already hashed)

    [Encrypted]
    public string? StreetAddress { get; set; }
}
```

### EF Core Encryption Provider

```csharp
services.AddDbContext<AppDbContext>((provider, options) =>
{
    var sqlConnection = new SqlConnection(connectionString);
    var aes = new AesEncryptionProvider(encryptionKey);

    options.UseSqlServer(sqlConnection)
           .AddInterceptors(new EncryptingInterceptor(aes));
});

public class EncryptingInterceptor : SaveChangesInterceptor
{
    private readonly IAesEncryptionProvider _encryptionProvider;

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var entries = eventArgs.Context.ChangeTracker
            .Entries<IMustEncrypt>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var properties = entry.Entity.GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttribute<EncryptedAttribute>() != null);

            foreach (var property in properties)
            {
                var value = property.GetValue(entry.Entity) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    var encrypted = _encryptionProvider.Encrypt(value);
                    property.SetValue(entry.Entity, encrypted);
                }
            }
        }

        return await base.SavedChangesAsync(eventArgs, cancellationToken);
    }
}
```

### Encryption Key Management

```appsettings.json
{
  "Security": {
    "EncryptionKey": "USE_AZURE_KEY_VAULT_IN_PRODUCTION"
  }
}
```

**Production:** Use Azure Key Vault, AWS KMS, or HashiCorp Vault:

```csharp
// Load from Key Vault
var keyVaultUrl = "https://mykeyvault.vault.azure.net/";
var credential = new DefaultAzureCredential();
var client = new SecretClient(new Uri(keyVaultUrl), credential);

var encryptionKeySecret = await client.GetSecretAsync("EncryptionKey");
var encryptionKey = encryptionKeySecret.Value.Value;
```

## Transport Security (TLS/HTTPS)

All data in transit encrypted:

### Enforcing HTTPS

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

app.UseHttpsRedirection();  // Redirect HTTP → HTTPS

// Enforce HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();  // HTTP Strict Transport Security (1 year)
}

// Require HTTPS for API
app.MapControllers();
app.MapGet("/**", context =>
{
    if (!context.Request.IsHttps)
        return context.Response.WriteAsync("HTTPS required");
    return Task.CompletedTask;
});
```

### Strict Transport Security (HSTS)

```csharp
services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);        // 1 year
    options.IncludeSubDomains = true;               // Apply to subdomains
    options.Preload = true;                         // Include in HSTS preload list
});
```

**Response Header:**

```
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
```

### TLS Configuration

```csharp
services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(https =>
    {
        // Minimum TLS 1.2
        https.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                            System.Security.Authentication.SslProtocols.Tls13;
    });
});
```

## Audit Logging

Tracks all sensitive operations:

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? MerchantId { get; set; }
    public string Action { get; set; }              // "Order.Created", "Category.Updated"
    public string Entity { get; set; }              // Entity name
    public Guid EntityId { get; set; }              // Record ID
    public string? OldValues { get; set; }          // JSON of previous state
    public string NewValues { get; set; }           // JSON of new state
    public string? Changes { get; set; }            // What changed
    public DateTime CreatedAt { get; set; }
    public string IpAddress { get; set; }
}
```

### Automatic Audit Interception

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
{
    var auditEntries = new List<AuditEntry>();

    var entries = ChangeTracker.Entries()
        .Where(e => e.Entity is not AuditLog
                && e.State == EntityState.Modified
                || e.State == EntityState.Added);

    foreach (var entry in entries)
    {
        var auditEntry = new AuditEntry(entry)
        {
            UserId = _currentUserService.UserId,
            MerchantId = _currentUserService.MerchantId,
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        };

        auditEntries.Add(auditEntry);
    }

    var result = await base.SaveChangesAsync(cancellationToken);

    foreach (var auditEntry in auditEntries)
    {
        AuditLogs.Add(auditEntry.ToAuditLog());
    }

    if (auditEntries.Any())
        await base.SaveChangesAsync(cancellationToken);

    return result;
}
```

### Audit Log Query

```csharp
// Get all changes by user
var userActions = await _db.AuditLogs
    .Where(a => a.UserId == userId && a.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    .OrderByDescending(a => a.CreatedAt)
    .ToListAsync();

// Sample logs:
// 2024-02-01 14:23:45 | merchant.admin@example.com | Order.Updated | Order-123 | Status: Pending → Completed
// 2024-02-01 10:15:30 | merchant.admin@example.com | Category.Deleted | Category-456
// 2024-01-31 18:00:00 | staff@example.com | Order.Created | Order-122
```

## Soft Deletes

Data never permanently deleted (enables recovery, audit trail):

```csharp
public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }  // Soft delete marker
    public DateTime? DeletedAt { get; set; }
}

public class Category : EntityBase
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; }
}
```

### Global Query Filter

Automatically excludes soft-deleted records:

```csharp
builder.Entity<Category>()
    .HasQueryFilter(e => !e.IsDeleted);

// All queries automatically filtered:
var active = await _db.Categories.ToListAsync();
// SELECT * FROM [catalog].[Categories] WHERE [IsDeleted] = 0
```

### Soft Delete Implementation

```csharp
public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
{
    var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

    if (category == null)
        throw new NotFoundException("Category", request.Id);

    // Mark as deleted (not physical delete)
    category.IsDeleted = true;
    category.DeletedAt = DateTime.UtcNow;

    await _categoryRepository.UpdateAsync(category, cancellationToken);

    // Audit log automatically created by SaveChangesAsync() interceptor
    return Unit.Value;
}
```

### Recovery

```csharp
public async Task RecoverCategoryAsync(Guid categoryId)
{
    // Include soft-deleted records for this query only
    var category = await _db.Categories
        .IgnoreQueryFilters()  // Bypass IsDeleted filter
        .FirstOrDefaultAsync(x => x.Id == categoryId && x.IsDeleted);

    if (category == null)
        throw new NotFoundException("Deleted category not found");

    category.IsDeleted = false;
    category.DeletedAt = null;

    await _db.SaveChangesAsync();
}
```

## Concurrency Control

Optimistic concurrency via row version:

```csharp
public class Order : EntityBase
{
    public Guid MerchantId { get; set; }

    [ConcurrencyCheck]  // Include in concurrency checks
    [Timestamp]         // Automatically managed by EF Core
    public byte[] RowVersion { get; set; }

    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
}
```

### Update with Concurrency Check

```csharp
try
{
    var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

    order.Status = OrderStatus.Completed;

    await _db.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Another process modified this record while we were editing
    throw new BusinessRuleException(
        "Order has been modified by another user. Please refresh and try again.");
}
```

## Secrets Management Best Practices

### ✅ Do This

- Store secrets in Azure Key Vault / AWS Secrets Manager / HashiCorp Vault
- Use managed identities (no API keys in code)
- Rotate secrets regularly (every 90 days)
- Never commit `.env` or secrets files

### ❌ Don't Do This

- Hard-code secrets in source code
- Store secrets in `.json` config files
- Use same secrets in dev/staging/prod
- Leave default credentials unchanged

### Local Development Secrets

```csharp
// appsettings.Development.json
{
  "Jwt": {
    "Secret": "dev-only-secret-key-not-for-production-use-only-for-local-testing-64-chars-min"
  }
}

// User Secrets for sensitive dev data
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "your-dev-secret"
```

## Related Documentation

- [Authentication & Passwords](authentication.md) — Password hashing, JWT
- [Authorization & Permissions](authorization.md) — Access control
- [Security Overview](overview.md) — Security architecture
- [Configuration - Secrets Management](../configuration/secrets-management.md) — Production secrets
