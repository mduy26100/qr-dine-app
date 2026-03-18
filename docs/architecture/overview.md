# Architecture Overview

QRDine implements **Onion Architecture** with **CQRS** (Command Query Responsibility Segregation), designed for scalability, testability, and maintainability.

## Layered Architecture

The system is organized into concentric layers, each with specific responsibilities and dependencies that flow **inward only**. Outer layers depend on inner layers; never the reverse.

```
┌─────────────────────────────────────────────────────────────┐
│  API Layer (QRDine.API)                                     │
│  ├─ Controllers, Routes, Middleware, Filters, DI Setup      │
│  └─ HTTP and Web Concerns Only                              │
├─────────────────────────────────────────────────────────────┤
│  Application Layer (QRDine.Application)                     │
│  ├─ CQRS Handlers, DTOs, Validators, Specifications         │
│  └─ Business Logic & Orchestration                          │
├─────────────────────────────────────────────────────────────┤
│  Application.Common (QRDine.Application.Common)             │
│  ├─ Abstractions, Behaviors, Shared Exceptions & Models     │
│  └─ No Implementation Details                               │
├─────────────────────────────────────────────────────────────┤
│  Domain Layer (QRDine.Domain)                               │
│  ├─ Entities, Value Objects, Business Rules                 │
│  └─ ZERO External Dependencies                              │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure Layer (QRDine.Infrastructure)               │
│  ├─ EF Core, ASP.NET Identity, External Services             │
│  └─ Repository Implementation, Database Migrations          │
└─────────────────────────────────────────────────────────────┘
```

### 2. CQRS Pattern

Commands and Queries are strictly separated for clarity and independent scaling.

**Commands** — Modify state, return results:

- Create, Update, Delete operations
- Return DTOs or status
- Can trigger side effects (emails, events)

**Queries** — Read-only state, return data:

- Fetch operations with filtering, sorting, pagination
- No side effects
- Optimizable for performance

Both routed through **MediatR** pipeline for cross-cutting concerns (validation, logging, transactions).

### 3. Multi-Tenancy by Design

Complete row-level data isolation ensures merchants cannot access each other's data:

1. **Global Query Filters** — EF Core automatically filters entities by `MerchantId`
2. **Auto Merchant Stamping** — SaveChangesAsync sets `MerchantId` on new entities
3. **Ownership Verification** — Handlers explicitly check resource ownership before modification

## Layer Details

### Domain Layer (`src/QRDine.Domain/`)

Innermost layer containing core business entities with **zero external dependencies**.

**Key abstractions:**

```csharp
public abstract class BaseEntity<TId>
{
    public TId Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public interface IMustHaveMerchant
{
    Guid MerchantId { get; set; }
}
```

**Domain modules:**

| Module       | Entities                                                             | Purpose                                     |
| ------------ | -------------------------------------------------------------------- | ------------------------------------------- |
| **Catalog**  | Category, Product, Table, Topping, ToppingGroup, ProductToppingGroup | Menu structure and table management         |
| **Sales**    | Order, OrderItem                                                     | Order lifecycle tracking                    |
| **Billing**  | Plan, Subscription, FeatureLimit, Transaction                        | Subscription and feature limiting           |
| **Tenant**   | Merchant                                                             | Merchant (tenant) data                      |
| **Identity** | ApplicationUser, ApplicationRole                                     | User and role model (from ASP.NET Identity) |

**Key enums:**

- `OrderStatus: Open, Paid, Cancelled`
- `OrderItemStatus: Pending, Processing, Completed, Cancelled`
- `FeatureType: Categories, Products, Tables, Orders`
- `SubscriptionStatus: Active, Expired, Cancelled`
- `PaymentStatus: Pending, Completed, Failed`

### Application Layer (`src/QRDine.Application/`)

Implements all business use cases through CQRS handlers. **Self-contained, organized by domain module.**

**Feature structure example (Catalog):**

```
Features/Catalog/
├── Categories/
│   ├── Commands/
│   │   ├── CreateCategory/
│   │   │   ├── CreateCategoryCommand.cs
│   │   │   ├── CreateCategoryCommandHandler.cs
│   │   │   ├── CreateCategoryCommandValidator.cs
│   │   │   └── CreateCategoryCommandResponse.cs
│   │   ├── UpdateCategory/
│   │   └── DeleteCategory/
│   ├── Queries/
│   │   ├── GetCategoriesByMerchant/
│   │   │   ├── GetCategoriesByMerchantQuery.cs
│   │   │   ├── GetCategoriesByMerchantQueryHandler.cs
│   │   │   └── GetCategoriesByMerchantQueryResponse.cs
│   │   └── GetCategoryById/
│   ├── DTOs/
│   │   ├── CategoryDto.cs
│   │   ├── CreateCategoryRequest.cs
│   │   └── UpdateCategoryRequest.cs
│   ├── Specifications/
│   │   ├── CategoriesSpecification.cs
│   │   └── ActiveCategoriesSpec.cs
│   └── Extensions/
│       └── CatalogMapperProfile.cs
├── Products/
│   └── (same structure)
├── Repositories/
│   ├── ICategoryRepository.cs
│   ├── IProductRepository.cs
│   └── ...
└── Mappings/
    └── AutoMapper profiles for Catalog DTOs
```

**Command example:**

```csharp
public record CreateCategoryCommand(CreateCategoryRequest Req) : IRequest<CreateCategoryResponse>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    private readonly IRepository<Category> _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public async Task<CreateCategoryResponse> Handle(/* ... */)
    {
        var merchantId = _currentUser.MerchantId!.Value;

        var category = new Category
        {
            Name = Req.Name,
            Description = Req.Description
            // MerchantId auto-stamped by SaveChangesAsync
        };

        await _repository.AddAsync(category);
        await _repository.SaveChangesAsync();

        return new CreateCategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };
    }
}
```

**Query example:**

```csharp
public record GetCategoriesByMerchantQuery(Guid MerchantId) : IRequest<List<CategoryDto>>;

public class GetCategoriesByMerchantQueryHandler : IRequestHandler<GetCategoriesByMerchantQuery, List<CategoryDto>>
{
    private readonly IRepository<Category> _repository;
    private readonly IMapper _mapper;

    public async Task<List<CategoryDto>> Handle(/* ... */)
    {
        var spec = new CategoriesSpec(MerchantId).WithoutDeleted();
        var categories = await _repository.ListAsync(spec);
        return _mapper.Map<List<CategoryDto>>(categories);
    }
}
```

**Key patterns:**

- **Specifications** — Use `Ardalis.Specification` for reusable query logic
- **DTOs** — Separate request and response models from domain entities
- **Validators** — Implement `IValidator<T>` with FluentValidation
- **Mappers** — AutoMapper profiles in each feature module
- **Repositories** — Feature-specific repository interfaces for data access

### Application.Common Layer (`src/QRDine.Application.Common/`)

Shared abstractions, exceptions, and pipeline behaviors decoupled from feature implementation.

**Key abstractions:**

```
Abstractions/
├── Persistence/
│   ├── IApplicationDbContext.cs
│   ├── IDatabaseTransaction.cs
│   └── IRepository<T>.cs (generic repository)
├── Identity/
│   └── ICurrentUserService.cs (user context)
├── ExternalServices/
│   └── IFileUploadService.cs (Cloudinary abstraction)
├── Notifications/
│   └── IEmailService.cs (email abstraction)
└── Caching/
    └── ICacheService.cs (Redis abstraction)
```

**Custom exceptions:**

```
Exceptions/
├── ValidationException (400)
├── BusinessRuleException (400)
├── NotFoundException (404)
├── ConflictException (409)
├── ForbiddenException (403)
├── ConcurrencyException (409)
└── ApplicationExceptionBase
```

**Pipeline behaviors (MediatR):**

```
Behaviors/
├── ValidationBehavior.cs (auto-validates via FluentValidation)
├── TransactionBehavior.cs (wraps commands in transactions)
├── LoggingBehavior.cs (logs command execution)
└── CachingBehavior.cs (caches query results)
```

### Infrastructure Layer (`src/QRDine.Infrastructure/`)

Implements all external concerns and provides implementations for abstractions defined in Application.Common.

**Key modules:**

| Module               | Responsibility                                            |
| -------------------- | --------------------------------------------------------- |
| **Persistence**      | EF Core DbContext, migrations, repository implementations |
| **Identity**         | JWT generation, user/role services, password hashing      |
| **ExternalServices** | Cloudinary file uploads, QR code generation               |
| **Email**            | SMTP email service via MailKit                            |
| **Caching**          | Redis-based distributed cache                             |
| **SignalR**          | Real-time order update hubs                               |

**Example: Repository Implementation**

```csharp
public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : class
{
    public Repository(ApplicationDbContext dbContext) : base(dbContext) { }

    // Inherits from Ardalis.Specification
    // Provides ListAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync
    // Automatically applies global query filters
}
```

**Dependency Injection Registration Order:**

```csharp
services
    .AddInfrastructure()        // Persistence, external services
    .AddSecurity()              // Identity, JWT
    .AddCrossCutting()          // CORS, versioning
    .AddApplication()           // MediatR, AutoMapper
    .AddFeatures()              // Feature-specific repositories
    .AddPresentation();         // Swagger, controllers
```

The order ensures inner layers are registered before outer layers that depend on them.

### API Layer (`src/QRDine.API/`)

Outermost layer handling all HTTP concerns.

**Key components:**

```
Controllers/
├── Identity/
│   ├── AuthController.cs (login, register)
│   └── UsersController.cs (user management)
├── Management/
│   ├── Catalog/ (products, categories, tables)
│   ├── Sales/ (orders, order items)
│   └── Dashboard/ (merchant dashboard)
├── Storefront/
│   ├── Catalog/ (public product listing)
│   └── Sales/ (customer ordering)
└── Admin/
    ├── MerchantsController.cs
    └── PlansController.cs

Middleware/
├── ExceptionHandlingMiddleware.cs (global error handling)
├── TenantResolutionMiddleware.cs (extract merchant from requests)
├── SubscriptionEnforcementMiddleware.cs (enforce feature limits)
└── StorefrontSubscriptionMiddleware.cs

Filters/
├── ApiResponseFilter.cs (wrap all responses)
└── FeatureLimitFilter.cs (check subscription limits)

DependencyInjection/
├── ServiceCollectionExtensions.cs (main orchestrator)
├── Infrastructure, Security, CrossCutting, Application, Features, Presentation/
│   └── Specific registration modules
```

**Controller pattern:**

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management/categories")]
[Authorize(Roles = "Merchant")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest req,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(req);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
```

Controllers are thin, dispatching all logic to CQRS handlers.

## Data Flow Example: Creating a Product

```
1. HTTP Request
   POST /api/v1/management/products
   Authorization: Bearer <token>

2. API Layer (Controllers)
   CategoriesController receives request
   Validates input model binding

3. API Layer (Middleware/Filters)
   ExceptionHandlingMiddleware wraps pipeline
   TenantResolutionMiddleware extracts MerchantId from auth
   ApiResponseFilter prepares response envelope

4. Authentication & Authorization
   JWT token validated
   User roles checked (must be Merchant)
   MerchantId resolved from claims

5. Application Layer (CQRS)
   MediatR receives CreateProductCommand
   ValidationBehavior runs FluentValidation
   Creates new Product entity

6. Domain Layer (Business Logic)
   Product entity validates: name, price, category
   Specifications ensure no duplicate products

7. Infrastructure Layer (Persistence)
   Repository.AddAsync(product) queued
   SaveChangesAsync triggered by TransactionBehavior

8. Database (EF Core + SQL Server)
   MerchantId auto-stamped (from CurrentUserService)
   INSERT executed with audit timestamps

9. Application Layer (Response)
   Handler returns ProductDto

10. API Layer (Response)
    ApiResponseFilter wraps in success envelope:
    {
      "data": { ProductDto },
      "meta": { timestamp, statusCode, traceId }
    }

11. HTTP Response
    201 Created
    Location header with product URL
```

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

## Extensibility Points

**Adding a new feature:**

1. Define entities in `src/QRDine.Domain/<Module>/`
2. Create CQRS handlers in `src/QRDine.Application/Features/<Module>/`
3. Implement abstractions in `src/QRDine.Infrastructure/`
4. Register services in DI container
5. Create API endpoints in `src/QRDine.API/Controllers/`

**Adding cross-cutting concern:**

1. Create pipeline behavior implementing `IPipelineBehavior<,>`
2. Register in `ServiceCollectionExtensions`
3. Applied to all commands/queries automatically

**Adding external service:**

1. Define interface in `src/QRDine.Application.Common/Abstractions/ExternalServices/`
2. Implement in `src/QRDine.Infrastructure/ExternalServices/`
3. Inject via DI in handlers
4. Easy to mock for testing

## Performance Considerations

1. **Query Optimization** — Use specifications with Select() to project DTOs
2. **Pagination** — Implement pagination in specifications for large result sets
3. **Caching** — Redis caching for frequently accessed read-only data
4. **Async/Await** — All I/O operations are async for scalability
5. **Global Query Filters** — Automatically applied, but respect index strategy
6. **Lazy Loading Disabled** — Explicit eager loading prevents N+1 queries

## Security Architecture

1. **Authentication** — JWT Bearer tokens with refresh token rotation
2. **Authorization** — Role-based access control (RBAC) + resource ownership checks
3. **Input Validation** — FluentValidation on all command inputs
4. **Data Isolation** — Global query filters and manual ownership checks
5. **Error Handling** — No sensitive information in error responses
6. **Rate Limiting** — Configurable per endpoint
7. **CORS** — Whitelist-based origin validation
