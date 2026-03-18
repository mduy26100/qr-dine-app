# Onion Architecture Patterns & Design Details

Advanced patterns, design decisions, and technical details of QRDine's Onion Architecture implementation.

---

## Design Patterns Used

| Pattern                  | Usage                                        |
| ------------------------ | -------------------------------------------- |
| **Onion Architecture**   | Concentric layered dependency structure      |
| **CQRS**                 | Separated command/query handling             |
| **Repository**           | Abstract data access layer                   |
| **Specification**        | Encapsulate query logic                      |
| **Dependency Injection** | IoC container for loose coupling             |
| **Pipeline Behavior**    | Cross-cutting concerns (validation, logging) |
| **Value Object**         | Immutable domain concepts                    |
| **Factory**              | Complex object creation                      |
| **Observer**             | SignalR for real-time updates                |

---

## Extensibility Points

### Adding a New Feature

1. **Define entities** in `src/QRDine.Domain/<Module>/`
2. **Create CQRS handlers** in `src/QRDine.Application/Features/<Module>/`
   - Commands for write operations
   - Queries for read operations
   - Validators using FluentValidation
3. **Implement abstractions** in `src/QRDine.Infrastructure/`
   - Repository implementations
   - External service integrations
4. **Register services** in DI container
   - Add to appropriate registration module
5. **Create API endpoints** in `src/QRDine.API/Controllers/`
   - Thin controllers dispatching to MediatR
   - Proper authorization attributes

### Adding Cross-Cutting Concern

1. Create pipeline behavior implementing `IPipelineBehavior<,>`
2. Register in `ServiceCollectionExtensions`
3. Applied to all commands/queries automatically

Example: Custom logging behavior

```csharp
public class CustomLoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<CustomLoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Executing {typeof(TRequest).Name}");
        var result = await next();
        _logger.LogInformation($"Completed {typeof(TRequest).Name}");
        return result;
    }
}
```

### Adding External Service

1. Define interface in `src/QRDine.Application.Common/Abstractions/ExternalServices/`
2. Implement in `src/QRDine.Infrastructure/ExternalServices/`
3. Inject via DI in handlers
4. Easy to mock for testing

Example: Email service

```csharp
// Interface (Application.Common)
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

// Implementation (Infrastructure)
public class EmailService : IEmailService
{
    private readonly IOptions<EmailSettings> _options;

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Implementation details...
    }
}

// Usage (Handler)
public class SendWelcomeEmailCommandHandler : IRequestHandler<SendWelcomeEmailCommand>
{
    private readonly IEmailService _emailService;

    public async Task Handle(SendWelcomeEmailCommand request, CancellationToken cancellationToken)
    {
        await _emailService.SendAsync(
            request.Email,
            "Welcome to QRDine",
            "...",
            cancellationToken);
    }
}
```

---

## Application Layer Features

### DTOs Organization

**Request Models** (input validation)

```
Features/<Module>/DTOs/
├── Create<Entity>Request.cs
├── Update<Entity>Request.cs
└── ...
```

**Response Models** (output data)

```
Features/<Module>/DTOs/
├── <Entity>Dto.cs
├── <Entity>ResponseDto.cs
└── ...
```

Keep DTOs separate from domain entities to enable independent evolution.

### Specifications Pattern

Encapsulate query logic in reusable specifications:

```csharp
// QuerySpec for filtered products
public class ProductsByMerchantSpec : Specification<Product>
{
    public ProductsByMerchantSpec(Guid merchantId, int pageSize = 10, int pageNumber = 1)
    {
        Query
            .Where(p => p.MerchantId == merchantId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category.Name
            });
    }
}

// Usage
var spec = new ProductsByMerchantSpec(merchantId);
var products = await _repository.ListAsync(spec);
```

Benefits:

- Reusable across handlers
- Testable query logic
- Prevents N+1 queries with Select() projections
- Centralized query optimization

### Validators Pattern

FluentValidation for all command inputs:

```csharp
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.ParentId)
            .Must((command, parentId) => ValidateHierarchy(command, parentId))
            .WithMessage("Category hierarchy must not exceed 2 levels");
    }

    private bool ValidateHierarchy(CreateCategoryCommand command, Guid? parentId)
    {
        if (!parentId.HasValue) return true; // Root category - always valid

        // Custom validation logic...
        return true;
    }
}

// Auto-triggered by ValidationBehavior
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

---

## Infrastructure Layer Details

### Repository Implementation Pattern

Generic repository with specification support:

```csharp
public class Repository<T> : RepositoryBase<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    private readonly ApplicationDbContext _dbContext;

    public Repository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator.GetQuery(_dbContext.Set<T>().AsQueryable(), spec);
    }
}
```

### Transaction Management

Commands wrapped in transactions via TransactionBehavior:

```csharp
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only wrap commands in transactions, not queries
        if (request is not ICommand)
            return await next();

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next();
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"Transaction failed: {ex.Message}");
            throw;
        }
    }
}
```

---

## Performance Considerations

### Query Optimization

1. **Projection to DTOs** (Select) prevents loading unnecessary properties

   ```csharp
   Query.Select(p => new ProductDto { Id = p.Id, Name = p.Name })
   ```

2. **Eager Loading** with Include() prevents N+1 queries

   ```csharp
   Query.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
   ```

3. **Pagination** for large result sets

   ```csharp
   Query.Skip((page - 1) * pageSize).Take(pageSize)
   ```

4. **Indexing** on frequently filtered columns
   ```csharp
   builder.HasIndex(e => e.MerchantId).IsUnique(false);
   ```

### Caching Strategy

- **Read-only data** → Redis cache with TTL
- **User-specific data** → Short TTL or no caching
- **Subscription data** → Cache invalidated on update

```csharp
public class CachedProductQuery : IRequest<ProductDto>
{
    public Guid ProductId { get; set; }
    public bool BypassCache { get; set; }
}

public class CachedProductQueryHandler : IRequestHandler<CachedProductQuery, ProductDto>
{
    private readonly ICacheService _cacheService;
    private readonly IRepository<Product> _repository;

    public async Task<ProductDto> Handle(CachedProductQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"product_{request.ProductId}";

        if (!request.BypassCache)
        {
            var cached = await _cacheService.GetAsync<ProductDto>(cacheKey);
            if (cached != null) return cached;
        }

        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        var dto = MapToDto(product);

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));
        return dto;
    }
}
```

### Async/Await Best Practices

- All I/O operations are async (database, file, HTTP)
- No `.Result` or `.Wait()` - always await
- Use `CancellationToken` for timeout support

```csharp
// ✅ Good
public async Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken)
{
    return await _repository.ListAsync(spec, cancellationToken);
}

// ❌ Bad - blocking call
public List<Product> GetProducts()
{
    return _repository.ListAsync(spec).Result;
}
```

---

## Middleware Execution Order

Middleware pipeline in `Program.cs` from first to last:

```
1. ExceptionHandlingMiddleware
   └─ Wraps all downstream with try-catch

2. TenantResolutionMiddleware
   └─ Extract and set CurrentMerchantId

3. SubscriptionEnforcementMiddleware
   └─ Check if merchant subscription valid

4. StorefrontSubscriptionMiddleware
   └─ Check tier limits for storefront

5. CORS Middleware
   └─ Allow/deny cross-origin requests

6. Authentication Middleware
   └─ Validate JWT tokens

7. Authorization Middleware
   └─ Check roles and policies

8. Controllers & Endpoints
```

---

## Testing Strategy

### Unit Testing Handlers

Mock dependencies, test business logic:

```csharp
[TestClass]
public class CreateCategoryCommandHandlerTests
{
    private Mock<IRepository<Category>> _repositoryMock;
    private Mock<ICurrentUserService> _currentUserMock;
    private CreateCategoryCommandHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _repositoryMock = new Mock<IRepository<Category>>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _handler = new CreateCategoryCommandHandler(_repositoryMock.Object, _currentUserMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidCommand_CreatesCategory()
    {
        // Arrange
        var command = new CreateCategoryCommand { Name = "Appetizers" };
        _currentUserMock.Setup(x => x.MerchantId).Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

**Reference:** See also [Architecture Overview](overview.md) for core layer information and [Project Structure](project-structure.md) for folder organization.
