## 3️⃣ Phân Tích Design Pattern

### 🔹 CQRS (Command Query Responsibility Segregation)

#### 📌 Được sử dụng ở đâu

- **Entire application layer** (`/src/QRDine.Application/Features/**`)
- Mỗi feature module (Catalog, Sales, Billing, Identity) đều sử dụng CQRS

#### 🎯 Mục đích

- **Tách biệt** read operations (Queries) từ write operations (Commands)
- Cho phép **independent optimization** của read và write paths
- Rõ ràng hóa intent: "Cái này thay đổi state" vs "Cái này chỉ đọc"

#### 🧠 Vấn đề giải quyết

1. **Thêm clarity** - Devs ngay lập tức biết operation có side effect không
2. **Optimization separately** - Queries có thể cache, Commands luôn fresh
3. **Different validation rules** - Commands validate strictly, Queries filter gracefully
4. **Scaling independently** - Có thể scale read replicas riêng biệt

#### ⚙️ Cách implement

- **MediatR library** - Acts as simple mediator/dispatcher
- **IRequest<T>** - Base interface cho Command/Query
- **IRequestHandler<T, TResponse>** - Handler implementation
- **IPipelineBehavior<T, TResponse>** - Cross-cutting concerns

**Flow:**

```
Controller sends Command/Query
     ↓
MediatR routes to handler
     ↓
ValidationBehavior runs (if validators exist)
     ↓
Handler executes business logic
     ↓
Response returned to controller
     ↓
ApiResponseFilter wraps in envelope
```

#### ⚖️ Trade-off

**Ưu điểm:**

- ✅ Clear separation of concerns
- ✅ Easy to test (mock handlers separately)
- ✅ Scalable (can have different read/write strategies)
- ✅ Reduced coupling between layers

**Nhược điểm:**

- ❌ More code (each operation needs Command + Handler + Validator)
- ❌ Learning curve (developers need to understand mediation pattern)
- ❌ Overkill for simple CRUD operations
- ❌ Potential over-engineering if not monitored

#### 🚫 Khi KHÔNG nên dùng

- Micro-services với simple endpoints
- Projects không cần separate read/write scaling
- V3 của startup (speed > architecture)

#### 🎤 Cách giải thích phỏng vấn

**Ngắn (2-3 câu):**

> Dự án sử dụng CQRS pattern để tách Command (write) từ Query (read) operations. Mỗi command/query là một separate handler, cho phép chúng tôi validate, cache, và optimize independent. Pattern này tăng clarity về side effects và scalability.

**Chi tiết hơn:**

> CQRS (Command Query Responsibility Segregation) là architectural pattern chia operations thành 2 loại:
>
> **Commands** - Thay đổi state (Create, Update, Delete)
>
> - Có validators
> - Không trả về dữ liệu chi tiết
> - Luôn go to database
>
> **Queries** - Chỉ đọc state
>
> - Không có side effects
> - Có thể cache kết quả
> - Có thể optimize với projections
>
> Ví dụ: `CreateProductCommand` update database, nhưng `GetProductDetailQuery` có thể trả về cached data hoặc projections tối ưu. Pattern này tất cả flow thông qua MediatR mediator - khi controller gửi request, MediatR routes tới handler + pipeline behaviors (validation, logging, caching, transactions đều auto-run).
>
> Benefit: Clear intent, independent optimization, easier testing, better separation.

#### 📊 Đánh giá mức độ sử dụng

- **Advanced** ✅
- Không phải misuse (ví dụ return data từ commands)
- Consistency maintained across whole application
- Validators + Formatters properly implemented

---

### 🔹 Repository Pattern

#### 📌 Được sử dụng ở đâu

- `src/QRDine.Infrastructure/Persistence/Repository<T>.cs` (Base)
- `src/QRDine.Infrastructure/Catalog/Repositories/` (Feature implementations)
- `src/QRDine.Infrastructure/Sales/Repositories/`
- `src/QRDine.Infrastructure/Billing/Repositories/`
- etc.

#### 🎯 Mục đích

- **Abstract** database access layer từ business logic
- Giúp **swap implementations** (SQL Server → PostgreSQL, in-memory → database)
- Provide **consistent interface** cho CRUD operations

#### 🧠 Vấn đề giải quyết

1. **Testability** - Mock repository, không cần database
2. **Flexibility** - Change DB provider without touching handlers
3. **Consistency** - All data access goes through abstraction

#### ⚙️ Cách implement

```csharp
// Generic repository interface (định nghĩa trong Application.Common)
public interface IRepository<T> : IRepositoryBase<T> where T : class
{
    // Inherited from Ardalis.Specification:
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<T?> SingleOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

// Generic base implementation (trong Infrastructure)
public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : class
{
    public Repository(ApplicationDbContext context) : base(context) { }
}

// Feature-specific interface (Application)
public interface IProductRepository : IRepository<Product> { }

// Feature-specific implementation (Infrastructure)
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }
}

// Usage in handler
public class GetProductDetailQueryHandler : IRequestHandler<GetProductDetailQuery, ProductDetailDto>
{
    private readonly IProductRepository _repo;

    public async Task<ProductDetailDto> Handle(GetProductDetailQuery req, CancellationToken ct)
    {
        var spec = new ProductWithCategorySpec(req.ProductId);
        var product = await _repo.SingleOrDefaultAsync(spec, ct);
        return _mapper.Map<ProductDetailDto>(product);
    }
}
```

#### ⚖️ Trade-off

**Ưu điểm:**

- ✅ Abstracts database details
- ✅ Easy to mock in tests
- ✅ Can swap DB providers
- ✅ Consistent CRUD interface
- ✅ Combined with Specifications = powerful query capability

**Nhược điểm:**

- ❌ One more layer of abstraction
- ❌ Generic repository can hide complex queries
- ❌ Might add unnecessary indirection for simple operations
- ❌ Feature-specific repositories often empty (just extending generic)

#### 🚫 Khi KHÔNG nên dùng

- Đơn giản projects với 1-2 DB queries
- Không cần testability
- ORM (EF Core) đã đủ transparent

#### 🎤 Cách giải thích phỏng vấn

**Ngắn:**

> Repository pattern abstracts data access layer. Chúng tôi có generic `Repository<T>` implement basic CRUD, combined với Specifications pattern để encapsulate complex queries. Mỗi feature có specific repository interfaces (IProductRepository, IOrderRepository) cho dependency injection.

**Chi tiết:**

> Repository Pattern là data access abstraction. Thay vì handlers directly touch DbContext, chúng ghi qua IRepository interface.
>
> **Implementation:**
>
> - Generic `Repository<T>` base class (from Ardalis.Specification)
> - Provides: GetByIdAsync, ListAsync, AddAsync, UpdateAsync, DeleteAsync, SaveChangesAsync
> - Feature-specific repositories (IProductRepository) extend generic interface
> - Handlers inject interfaces, không concrete implementations
>
> **Benefit:**
>
> - Testable (mock repositories)
> - Flexible DB provider swapping
> - Consistent CRUD pattern
> - Makes queries composable with Specifications
>
> **Example:**
>
> ```
> var spec = new ProductsByMerchantSpec(merchantId);
> var products = await _productRepo.ListAsync(spec, ct);
> ```
>
> Handler không biết SQL, Repository không biết business logic.

#### 📊 Đánh giá mức độ sử dụng

- **Advanced** ✅
- Combined với Specifications - very powerful
- Generic base + feature-specific interfaces - good balance
- Proper abstraction, not over-engineered

---

### 🔹 Specification Pattern

#### 📌 Được sử dụng ở đâu

- `src/QRDine.Application/Features/*/Specifications/` (dozens of specs)
- Tất cả `.ListAsync(spec)`, `.SingleOrDefaultAsync(spec)`, `.AnyAsync(spec)` calls

#### 🎯 Mục đích

- **Encapsulate query logic** (WHERE, ORDER BY, INCLUDE, pagination)
- **Reuse across handlers** - viết 1 lần, dùng nhiều nơi
- **Prevent N+1** - Specs có Select() projections

#### 🧠 Vấn đề giải quyết

1. **N+1 Query Problem** - Có Include() + Select() to project early
2. **Query Logic Scattered** - Tất cả WHERE/ORDER logic ở 1 file
3. **Non-Testable Queries** - Queries embedded in handlers không test được

#### ⚙️ Cách implement

```csharp
// Base specification (từ Ardalis.Specification library)
public class ProductsByMerchantSpec : Specification<Product, ProductDto>
{
    public ProductsByMerchantSpec(Guid merchantId, int pageSize = 10, int pageNumber = 1)
    {
        Query  // Build EF query
            .Where(p => p.MerchantId == merchantId && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.ProductToppingGroups)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto  // Projection - only load needed fields
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category.Name,
                ToppingCount = p.ProductToppingGroups.Count
            });
    }
}

// Another spec - checking existence
public class ProductByNameSpec : Specification<Product>
{
    public ProductByNameSpec(Guid merchantId, string name, Guid? excludeId = null)
    {
        Query.Where(p => p.MerchantId == merchantId
                      && p.Name == name
                      && !p.IsDeleted
                      && (excludeId == null || p.Id != excludeId));
    }
}

// Usage
var spec = new ProductsByMerchantSpec(merchantId);
var products = await _productRepo.ListAsync(spec, ct);

var existsSpec = new ProductByNameSpec(merchantId, "Sushi", excludeId: currentProductId);
bool exists = await _productRepo.AnyAsync(existsSpec, ct);
```

#### ⚖️ Trade-off

**Ưu điểm:**

- ✅ Query logic centralized & reusable
- ✅ Prevents N+1 via projections
- ✅ Testable queries
- ✅ Can have named queries (self-documenting)
- ✅ Can be combined (inheritance, composition)

**Nhược điểm:**

- ❌ More files (if 1 spec per query)
- ❌ Dynamic queries harder (need builder pattern)
- ❌ Learning curve (Ardalis.Specification library)

#### 🚫 Khi KHÔNG nên dùng

- Queries chỉ dùng 1 lần
- Simple single-table SELECT

#### 🎤 Cách giải thích phỏng vấn

**Ngắn:**

> Specification pattern encapsulates query logic. Ví dụ `ProductsByMerchantSpec` defines WHERE, INCLUDE, ORDER, pagination trong 1 class, tái sử dụng across handlers, prevent N+1 queries with Select projections.

**Chi tiết:**

> Specification pattern (from Ardalis.Specification library) encapsulates reusable query logic.
>
> **Problem it solves:**
>
> 1. N+1 Query - Thường load entity, sau đó 1 loop load related data từng cái
> 2. Query logic scattered - WHERE, INCLUDE, ORDER logic ở nhiều handler
> 3. Queries không testable
>
> **How it works:**
> Each specification extends `Specification<T>` or `Specification<T, TResult>`:
>
> - T = Entity type (Product)
> - TResult = Projected DTO type (ProductDto)
>
> Bên trong spec, ta xây dựng EF query:
>
> - `.Where()` - filter
> - `.Include()` - join related tables
> - `.OrderBy()` - sort
> - `.Skip()/Take()` - pagination
> - `.Select()` - projection to DTO
>
> **Benefits:**
>
> - Named, reusable queries
> - Prevents N+1 (Select project early)
> - Queries centralized, easy to find
> - Testable
>
> **Example:**
>
> ```
> // Define once
> public class OrdersByMerchantSpec : Specification<Order, OrderDto>
> {
>     public OrdersByMerchantSpec(Guid merchantId, DateTime from, DateTime to)
>     {
>         Query
>             .Where(o => o.MerchantId == merchantId
>                      && o.CreatedAt >= from
>                      && o.CreatedAt <= to)
>             .Include(o => o.OrderItems)
>             .OrderByDescending(o => o.CreatedAt)
>             .Select(o => new OrderDto
>             {
>                 Id = o.Id,
>                 Total = o.TotalAmount,
>                 Status = o.Status.ToString(),
>                 ItemCount = o.OrderItems.Count
>             });
>     }
> }
>
> // Use many times
> var spec = new OrdersByMerchantSpec(merchantId, from, to);
> var orders = await _orderRepo.ListAsync(spec, ct);  // 1 query, optimized projection
> ```

#### 📊 Đánh giá mức độ sử dụng

- **Advanced** ✅
- Widely used across application
- Good naming (self-documenting)
- Prevents N+1 effectively (Select projections)

---

### 🔹 Mediator Pattern (via MediatR)

#### 📌 Được sử dụng ở đâu

- **Every** handler dispatch (`_mediator.Send(command/query)`)
- `src/QRDine.API/Controllers/` - Controllers dispatch commands/queries
- `src/QRDine.Application/Features/*/Commands/` & `Queries/`

#### 🎯 Mục đích

- **Decouple** requestor từ handler
- **Centralize** request routing
- **Enable pipeline behaviors** (cross-cutting concerns)

#### 🧠 Vấn đề giải quyết

1. **Coupling** - Controllers tidak directly call handlers
2. **Cross-cutting concerns** - Validation, logging, transactions tự động
3. **Dynamic dispatch** - Không cần if/switch statement

#### ⚙️ Cách implement

```csharp
// Controller sends request
[HttpPost]
public async Task<IActionResult> CreateProduct(
    [FromBody] CreateProductRequest request,
    CancellationToken ct)
{
    var command = new CreateProductCommand(request);
    var result = await _mediator.Send(command, ct);  // <-- Mediator
    return CreatedAtAction(nameof(GetProductDetail), new { id = result.Id }, result);
}

// MediatR Configuration (Program.cs)
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(applicationAssembly);  // Find all handlers
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));       // Auto-validate
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));          // Auto-log
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));      // Auto-transaction
});

// Handler (MediatR routes to this)
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponseDto>
{
    public async Task<ProductResponseDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Business logic here
    }
}

// Behaviors (cross-cutting, run for every request)
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Run validators
        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct))
        );

        var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();
        if (failures.Count > 0)
            throw new ValidationException(failures);

        // Continue to handler
        return await next();
    }
}
```

#### ⚖️ Trade-off

**Ưu điểm:**

- ✅ Decouples controllers from handlers
- ✅ Enables common behaviors (validation, logging, transactions)
- ✅ Easy to add new cross-cutting concerns
- ✅ Handlers are testable (just pass requests)

**Nhược điểm:**

- ❌ Magic (hard to trace execution at first)
- ❌ Slight performance overhead (reflection, routing)
- ❌ Need to understand MediatR library

#### 🚫 Khi KHÔNG nên dùng

- Đơn giản projects với 1-2 endpoints
- Real-time systems (overhead không chấp nhận được)

#### 🎤 Cách giải thích phỏng vấn

**Ngắn:**

> MediatR act as mediator/dispatcher between controllers and handlers. Controllers send Command/Query, MediatR routes to appropriate handler + pipeline behaviors (validation, logging, etc run automatically).

**Chi tiết:**

> MediatR is a mediator pattern library. Instead of controllers directly calling services, they send requests through MediatR.
>
> **Flow:**
>
> ```
> Controller.SendAsync(CreateProductCommand)
>     ↓
> MediatR routes to IRequestHandler<CreateProductCommand>
>     ↓
> Behaviors execute in order:
>   1. ValidationBehavior - Check input validation
>   2. LoggingBehavior - Log request details
>   3. TransactionBehavior - Begin DB transaction
>     ↓
> Handler executes (Handler.Handle())
>     ↓
> Behaviors execute after (if configured):
>   1. TransactionBehavior - Commit or rollback
>   2. LoggingBehavior - Log response
>     ↓
> Response returned
> ```
>
> **Benefits:**
>
> - Decouples controllers from handler implementations
> - Cross-cutting concerns become behaviors (not scattered across handlers)
> - Easy to add new concerns (just add IPipelineBehavior)
> - Testable handlers (just call Handle() directly)
>
> **Example:**
>
> ```csharp
> // Add new concern without changing any handler
> cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
> // Now all queries are cached automatically
> ```

#### 📊 Đánh giá mức độ sử dụng

- **Advanced** ✅
- Used comprehensively
- Behaviors well-implemented (validation, logging, transactions, caching)
- Not over-engineered

---

### 🔹 Dependency Injection Container

#### 📌 Được sử dụng ở đâu

- `src/QRDine.API/DependencyInjection/` (modular registration)
- `src/QRDine.API/Program.cs` (registration calls)

#### 🎯 Mục đích

- **Invert control** - Framework creates instances, not code
- **Loose coupling** - Depend on abstractions, not implementations
- **Centralize configuration** - All services registered in 1 place

#### 🧠 Vấn đề giải quyết

1. **Tight coupling** - Without DI, classes create dependencies (new ServiceImpl())
2. **Testability** - Hard to mock without DI
3. **Configuration scattered** - Service creation logic everywhere

#### ⚙️ Cách implement

```csharp
// Built-in .NET DI container (trong Program.cs)
var builder = WebApplication.CreateBuilder(args);

// Modular registration pattern
builder.Services
    .AddInfrastructure()        // EF, Identity, external services
    .AddApplication()            // MediatR, AutoMapper, validators
    .AddControllers()            // Controllers
    .AddSwaggerGen()             // Swagger
    .AddCors()                   // CORS
    .AddRateLimiter()            // Rate limiting
    // ... etc

// Extension methods for modularity (trong separate files)
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthTokenService, AuthTokenService>();

        services.AddScoped<IFileUploadService, CloudinaryFileUploadService>();
        services.AddScoped<IEmailService, BrevoApiEmailService>();
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(applicationAssembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(applicationAssembly);
        services.AddAutoMapper(applicationAssembly);

        return services;
    }
}
```

#### ⚖️ Trade-off

**Ưu điểm:**

- ✅ Loose coupling (depend on abstractions)
- ✅ Easy to mock/test
- ✅ Centralized configuration
- ✅ Modular registration

**Nhược điểm:**

- ❌ Slight performance overhead (reflection, tree building)
- ❌ Complexity for simple projects
- ❌ Hidden dependencies (không rõ instance created ở đâu)

#### 🚫 Khi KHÔNG nên dùng

- Very small projects (< 5 classes)

#### 🎤 Cách giải thích phỏng vấn

**Ngắn:**

> DI container (built-in .NET) manages object creation. Modular extension methods (AddInfrastructure, AddApplication) register services. Controllers/handlers request dependencies via constructor injection, framework provides instances.

---

### 🔹 Unit of Work Pattern

#### 📌 Được sử dụng ở đâu

- `src/QRDine.Infrastructure/Persistence/ApplicationDbContext.cs`
- `SaveChangesAsync()` method

#### 🎯 Mục đích

- **Single transaction** cho multiple operations
- **Atomic commits** - tất cả changes or nothing

#### 🧠 Vấn đề giải quyết

1. **Partial commits** - Nếu 3 saves, first 2 succeed nhưng third fails
2. **Data inconsistency** - Related data out of sync

#### ⚙️ Cách implement

```csharp
// EF DbContext IS Unit of Work
public class ApplicationDbContext : DbContext
{
    public override async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        // Auto-stamp MerchantId
        var entries = ChangeTracker.Entries<IMustHaveMerchant>()
            .Where(e => e.State == EntityState.Added);
        foreach (var entry in entries)
        {
            entry.Entity.MerchantId = _currentUserService.MerchantId.Value;
        }

        // All tracked changes committed in single transaction
        return await base.SaveChangesAsync(ct);
    }
}

// TransactionBehavior ensures commands use UoW
public class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _context;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var transaction = await _context.BeginTransactionAsync(ct);

        try
        {
            var result = await next();
            await transaction.CommitAsync(ct);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}

// Usage: Save multiple entities
public async Task Handle(CreateOrderCommand request, CancellationToken ct)
{
    var order = new Order { /* ... */ };
    var items = new[] { new OrderItem { /* ... */ } };

    await _orderRepo.AddAsync(order, ct);
    await _itemRepo.AddAsync(items[0], ct);

    // Single atomic save
    await _orderRepo.SaveChangesAsync(ct);
}
```

#### 📊 Đánh giá

- **Good** ✅
- EF DbContext naturally implements UoW
- TransactionBehavior ensures Commands use transactions

---

### 🔹 Pipeline Behavior Pattern

#### 📌 Được sử dụng ở đâu

- `src/QRDine.Application.Common/Behaviors/` (4 behaviors)
- MediatR `IPipelineBehavior<,>` implementations

#### 🎯 Mục đích

- **Cross-cutting concerns** without scattering in handlers
- **Intercept** request/response

#### ⚙️ Behaviors implement

1. **ValidationBehavior** - Auto-validate inputs (FluentValidation)
2. **LoggingBehavior** - Log request/response
3. **TransactionBehavior** - Wrap commands in DB transaction
4. **CachingBehavior** - Cache query results

#### 📊 Đánh giá

- **Advanced** ✅
- Well-implemented
- Reduces code duplication

---

### 🔹 Multi-Tenancy Pattern (Row-Level Security)

#### 📌 Được sử dụng ở đâu

- EF Core global query filters
- `IMustHaveMerchant` interface
- `TenantResolutionMiddleware`
- `ICurrentUserService`

#### 🎯 Mục đích

- **Data isolation** - Merchants chỉ see their own data
- **Automatic filtering** - No need to manually add WHERE MerchantId

#### 🧠 Vấn đề giải quyết

1. **Data leakage** - Merchant A sees Merchant B data
2. **Manual filtering** - Every query must `WHERE MerchantId == current`
3. **Authorization logic** scattered

#### ⚙️ Cách implement

```csharp
// Layer 1: Global Query Filter in EF Core
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Category>()
        .HasQueryFilter(e => !CurrentMerchantId.HasValue
                          || e.MerchantId == CurrentMerchantId);

    builder.Entity<Product>()
        .HasQueryFilter(e => !CurrentMerchantId.HasValue
                          || e.MerchantId == CurrentMerchantId);
    // ... for all tenant-scoped entities
}

public Guid CurrentMerchantId => _currentUserService.MerchantId ?? Guid.Empty;

// Layer 2: Marker interface for tenant-scoped entities
public interface IMustHaveMerchant
{
    Guid MerchantId { get; set; }
}

// All tenant-scoped entities implement this
public class Product : BaseEntity, IMustHaveMerchant
{
    public Guid MerchantId { get; set; }  // Filtered by global query filter
    public virtual Merchant Merchant { get; set; }
}

// Layer 3: Auto-stamping on SaveChanges
public override async Task<int> SaveChangesAsync(CancellationToken ct)
{
    var entries = ChangeTracker.Entries<IMustHaveMerchant>()
        .Where(e => e.State == EntityState.Added);

    foreach (var entry in entries)
    {
        entry.Entity.MerchantId = _currentUserService.MerchantId.Value;
    }

    return await base.SaveChangesAsync(ct);
}

// Layer 4: MerchantId resolution from multiple sources
public class TenantResolutionMiddleware
{
    public async Task Invoke(HttpContext context)
    {
        var merchantId = context.User?.FindFirst(AppClaimTypes.MerchantId)?.Value
                      ?? context.GetRouteValue("merchantId")?.ToString()
                      ?? context.Request.Query["merchantId"].FirstOrDefault();

        if (Guid.TryParse(merchantId, out var id))
        {
            context.Items[HttpContextKeys.ResolvedMerchantId] = id;
        }

        await _next(context);
    }
}

// Layer 5: CurrentUserService gets it from claims or HttpContext
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid? MerchantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;

            // 1. Try JWT claim (authenticated)
            var claimValue = context?.User?.FindFirst(AppClaimTypes.MerchantId)?.Value;
            if (Guid.TryParse(claimValue, out var id))
                return id;

            // 2. Try HttpContext items (from middleware resolution)
            if (context?.Items.TryGetValue(HttpContextKeys.ResolvedMerchantId, out var obj) == true
                && obj is Guid contextId)
                return contextId;

            return null;
        }
    }
}

// Usage: No WHERE needed!
var products = await _productRepo.ListAsync(spec, ct);
// Query automatically filtered: WHERE MerchantId == current && !IsDeleted
```

#### ⚖️ Trade-off

**Ưu điểm:**

- ✅ Automatic filtering (less error-prone)
- ✅ Multiple isolation layers (query filter + explicit checks)
- ✅ Hard to accidentally leak data
- ✅ Clean queries (no manual WHERE clauses)

**Nhược điểm:**

- ❌ Global query filters sometimes confusing
- ❌ Must remember every entity implements IMustHaveMerchant
- ❌ Harder to debug queries ("why didn't this entity show up?")

#### 📊 Đánh giá

- **Advanced** ✅
- Well-designed (3-layer: filters + auto-stamp + explicit checks)
- Good balance security + developer experience

---

## 4️⃣ Phân Tích Nguyên Tắc Thiết Kế (SOLID & Others)

### 🔹 S - Single Responsibility Principle

**Áp dụng ở:**

- Controllers (chỉ dispatch, không contain logic)
- Handlers (1 handler = 1 use case)
- Services (BrevoApiEmailService chỉ xử lý email; AuthTokenService chỉ JWT)
- Repositories (chỉ data access)

**Đánh giá:**

- ✅ **Tốt** - Mỗi class có 1 trách nhiệm rõ ràng
- ❌ Minor: Some services lớn (OrderCreationService có nhiều methods)

---

### 🔹 O - Open/Closed Principle

**Áp dụng ở:**

- Repository pattern (mở extension via IRepository<T>, đóng modification)
- Pipeline behaviors (mở thêm behaviors mà không modify existing)
- Specifications (open for new specs, closed for modification)

**Đánh giá:**

- ✅ **Tốt** - Có thể thêm mới mà không modify existing code

---

### 🔹 L - Liskov Substitution Principle

**Áp dụng ở:**

- Repository implementations (IProductRepository, IOrderRepository đều substituable với IRepository<T>)
- Exception types (tất cả extend ApplicationExceptionBase)
- Behaviors (ValidationBehavior, LoggingBehavior đều implement IPipelineBehavior)

**Đánh giá:**

- ✅ **Tốt** - Subclasses can replace parent without breaking

---

### 🔹 I - Interface Segregation Principle

**Áp dụng ở:**

- `IRepository<T>` - chỉ CRUD methods
- `ICurrentUserService` - chỉ user context
- `IEmailService` - chỉ email methods
- Feature-specific repositories (IProductRepository, IOrderRepository)

**Đánh giá:**

- ✅ **Tốt** - Clients implement exactly what they need

---

### 🔹 D - Dependency Inversion Principle

**Áp dụng ở:**

- Tất cả handlers inject interfaces, không concrete classes
- Controllers inject IMediator, không handlers directly
- Services inject abstractions (IRepository<T>, ICacheService, IEmailService)

**Đánh giá:**

- ✅ **Rất tốt** - Whole system depends on abstractions

---

### 🔹 DRY - Don't Repeat Yourself

**Áp dụng ở:**

- Specifications (WHERE/INCLUDE logic reused)
- Repository methods (generic base class)
- Mapper profiles (centralized DTO mapping)
- Exception mapping (centralized in ExceptionHandlingMiddleware)

**Đánh giá:**

- ✅ **Tốt** - Không nhiều duplication

---

### 🔹 KISS - Keep It Simple, Stupid

**Đánh giá:**

- ⚠️ **Moderate**
- ✅ Controllers đơn giản (just dispatch)
- ✅ Handlers focused
- ❌ Some complexity (Specification pattern, CQRS) có learning curve
- ❌ Middleware pipeline có 5+ middlewares

---

### 🔹 YAGNI - You Aren't Gonna Need It

**Đánh giá:**

- ✅ **Tốt** - Không see unnecessary features
- ❌ Minor: Some abstractions (FeatureLimitService, SubscriptionEnforcement) mà có thể simplify

---

### 🔹 Separation of Concerns

**Triển khai ở:**

- Distinct layers (Domain, Application, Infrastructure, API)
- Each handler focuses on 1 use case
- Middleware cho cross-cutting concerns
- Behaviors cho automatic validation/logging/transactions
- Repositories cho data access

**Đánh giá:**

- ✅ **Rất tốt** - Clear separation across layers

---

## 5️⃣ Phân Tích Luồng Xử Lý Chính (Request Flow)

### 📊 Flow: Create Product (Command)

```
1. HTTP Request (POST /api/v1/management/products)
   ├─ Header: Authorization: Bearer <jwt_token>
   ├─ Body: { name: "Pizza", price: 50000, categoryId: "..." }
   └─ Content-Type: application/json

2. Middleware Pipeline (Program.cs order)
   ├─ ForwardedHeaders                  # Trust proxy headers for forwarded IPs
   ├─ ExceptionHandlingMiddleware       # 🔴 FIRST: Wrap entire pipeline in try-catch
   ├─ Serilog RequestLogging            # Log HTTP request
   ├─ HTTPS Redirection                 # Force HTTPS
   ├─ CORS Middleware                   # Check origin
   ├─ RateLimiter                       # Check endpoint rate limit (global)
   ├─ Authentication Middleware         # Parse JWT token -> extract claims
   ├─ TenantResolutionMiddleware        # Extract MerchantId from:
   │                                     #   1. JWT claim (primary)
   │                                     #   2. Route param {merchantId}
   │                                     #   3. Query param ?merchantId=
   ├─ StorefrontSubscriptionMiddleware  # Validate storefront access (skip if management)
   ├─ Authorization Middleware          # Check user roles (must be Merchant/Staff)
   ├─ SignalR Hub Mapping (/hubs/order) # Real-time hubs
   └─ SubscriptionEnforcementMiddleware # Validate subscription active

3. Controller Action
   ├─ CategoriesController.CreateProduct()
   ├─ Input model binding (JSON → CreateProductRequest object)
   └─ Dispatch to MediatR
       var command = new CreateProductCommand(request);
       var result = await _mediator.Send(command, cancellationToken);

4. MediatR Routing & Pipeline
   ├─ Find handler: CreateProductCommandHandler : IRequestHandler<CreateProductCommand, >
   └─ Execute pipeline behaviors in order:

   a) ValidationBehavior
      ├─ Find IValidator<CreateProductCommand> (CreateProductCommandValidator)
      ├─ Validate input:
      │  ├─ Name: NotEmpty, MaxLength(256)
      │  ├─ CategoryId: NotEmpty, exists in DB
      │  └─ Price: GreaterThan(0)
      └─ If validation fails → throw ValidationException

   b) LoggingBehavior
      ├─ Log request: { type: "CreateProductCommand", properties: {...} }
      └─ Continue to handler

   c) TransactionBehavior
      ├─ Begin transaction
      └─ Continue to handler

5. CreateProductCommandHandler.Handle()
   ├─ Extract MerchantId from ICurrentUserService.MerchantId (from JWT claim)
   ├─ Validate category exists & belongs to merchant:
   │  └─ var categorySpec = new CategoryByIdSpec(request.Dto.CategoryId);
   │     var category = await _categoryRepo.SingleOrDefaultAsync(spec, ct)
   │     if (category?.MerchantId != merchantId) → ForbiddenException
   ├─ Check product name duplicate (if needed):
   │  └─ Use ProductByNameSpec
   ├─ Create Product entity:
   │  └─ var product = new Product {
   │       Name = request.Dto.Name,
   │       Price = request.Dto.Price,
   │       CategoryId = request.Dto.CategoryId,
   │       // MerchantId NOT SET HERE (auto-stamped in SaveChanges)
   │     };
   ├─ Add to repository
   │  └─ await _productRepo.AddAsync(product, ct);
   └─ Persist to database
      └─ await _productRepo.SaveChangesAsync(ct);

6. SaveChangesAsync() (ApplicationDbContext)
   ├─ Auto-stamp MerchantId on new IMustHaveMerchant entities:
   │  └─ entry.Entity.MerchantId = _currentUserService.MerchantId.Value;
   ├─ Set audit timestamps (CreatedAt, UpdatedAt)
   ├─ Notify cache invalidation (clear category cache)
   └─ Execute SaveChanges() → EF Core generates INSERT SQL

7. Database (SQL Server)
   ├─ BEGIN TRANSACTION (from TransactionBehavior)
   ├─ INSERT [catalog].[Products] (Id, MerchantId, CategoryId, Name, Price, ...)
   │  VALUES (@id, @merchantId, @categoryId, @name, @price, ...)
   ├─ COMMIT TRANSACTION
   └─ Return to application

8. Handler returns ProductResponseDto
   ├─ Map Product entity → ProductResponseDto (AutoMapper)
   └─ Return to MediatR

9. MediatR Pipeline (after handler)
   ├─ TransactionBehavior: Commit transaction (already committed by EF)
   ├─ LoggingBehavior: Log response & execution time
   └─ Return result

10. Controllers.CreateProduct() returns
    ├─ Map ProductResponseDto → IActionResult
    ├─ Return CreatedAtAction(nameof(GetProductDetail),
                              new { id = result.Id }, result);
    └─ Status 201 Created

11. ApiResponseFilter (Result Filter)
    ├─ Intercept controller result
    ├─ Wrap in ApiResponse envelope:
    │  {
    │    "data": { id, name, price, ... },
    │    "error": null,
    │    "meta": {
    │      "timestamp": "2026-03-22T10:30:45Z",
    │      "path": "/api/v1/management/products",
    │      "method": "POST",
    │      "statusCode": 201,
    │      "traceId": "...",
    │      "requestId": "..."
    │    }
    │  }
    └─ Return wrapped response

12. HTTP Response (201 Created)
    ├─ Status: 201
    ├─ Body: ApiResponse
    ├─ Header Location: /api/v1/management/products/{newId}
    └─ Content-Type: application/json

13. Serilog RequestLogging
    └─ Log complete HTTP request/response with timing
```

### 📊 Flow: Get Products (Query)

```
1-3. [Same as Create Product: HTTP → Middleware → Controller]

4. Controller dispatches Query
   var query = new GetProductsPagedQuery(merchantId, pageSize: 20, pageNumber: 1);
   var result = await _mediator.Send(query, cancellationToken);

5. MediatR Pipeline (Query has NO validator - read-only)
   ├─ ValidationBehavior: Skip if no validator registered
   ├─ LoggingBehavior: Log query
   ├─ TransactionBehavior: Skip for queries (no transaction needed)
   └─ Execute GetProductsPagedQueryHandler

6. GetProductsPagedQueryHandler.Handle()
   ├─ Get current MerchantId from ICurrentUserService
   ├─ Build specification: ProductsByMerchantSpec(merchantId, pageSize, pageNumber)
   ├─ Fetch from cache if exists (CachingBehavior)
   │  OR fetch from database:
   │     var spec = new ProductsByMerchantSpec(...);
   │     var products = await _productRepo.ListAsync(spec, ct);
   │     // Query includes: WHERE MerchantId == current + !IsDeleted
   │     //                ORDER BY CreatedAt DESC
   │     //                SKIP/TAKE for pagination
   │     //                SELECT projection to DTO
   │     // All in 1 optimized query (no N+1)
   ├─ Get total count: var totalCount = await _productRepo.CountAsync(countSpec, ct);
   └─ Return PagedResult<ProductDto>

7. Cache (if applicable)
   ├─ CachingBehavior stores result in Redis with TTL
   └─ Next request returns cached result (no DB hit)

8. [Same as Create Product: Handler → Controller → ApiResponseFilter → Response]
```

---

## 6️⃣ Phân Tích Các Thành Phần Quan Trọng

### 🔐 Authentication & Authorization

#### Implementation

**JWT Token Generation (LoginService)**

```csharp
public class LoginService
{
    public async Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto dto, CancellationToken ct)
    {
        // 1. Get user from database
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            throw new BadRequestException("Invalid email or password");

        // 2. Check password
        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            throw new BadRequestException("Invalid email or password");

        // 3. Check subscription (if Merchant)
        if (user.MerchantId.HasValue)
        {
            var subscription = await _subscriptionRepo.GetActiveByMerchantAsync(user.MerchantId.Value, ct);
            if (subscription?.Status != SubscriptionStatus.Active)
                throw new BadRequestException("Subscription expired or inactive");
        }

        // 4. Generate JWT token
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        // 5. Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _refreshTokenRepo.AddAsync(refreshTokenEntity, ct);
        await _refreshTokenRepo.SaveChangesAsync(ct);

        return new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresIn = 600 // 10 minutes
        };
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            // Critical for multi-tenancy
            new(AppClaimTypes.MerchantId, user.MerchantId?.ToString() ?? "")
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new(ClaimTypes.Role, role));
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(10),  // Access token: 10 min
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
```

**JWT Configuration (Program.cs)**

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.ValidateIssuerSigningKey = true;
        options.IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret));
        options.ValidateIssuer = false;
        options.ValidateAudience = false;
        options.ValidateLifetime = true;
        options.ClockSkew = TimeSpan.FromSeconds(30);
    });
```

**Authorization (Role-based)**

```csharp
[Authorize(Roles = "Merchant,Staff")]  // Controllers check roles
[ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
public class ProductsController : ControllerBase { }

// Also enforced via Middleware
app.UseAuthentication();
app.UseAuthorization();
```

#### Mục đích

- 🎯 Secure API endpoints
- 🎯 Identify users & their merchants
- 🎯 Multi-tenant context via JWT claims

#### Đánh giá

- ✅ **Tốt**: JWT tokens, refresh tokens, proper expiry
- ✅ Merchant isolation via claims
- ❌ Minor: No token revocation (blacklist) mechanism

---

### 📜 Logging (Serilog + Seq)

#### Implementation

**Serilog Configuration (Program.cs)**

```csharp
builder.Services.AddSerilog((provider, config) =>
{
    config
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "QRDine.API")
        .Enrich.WithProperty("Environment", env.EnvironmentName)
        .Enrich.WithMachineName()
        .WriteTo.Console()
        .WriteTo.Seq("http://seq:5341");  // Log to Seq server (Docker)
});
```

**Serilog Request Logging Middleware**

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (context, diagnostics) =>
    {
        if (context.User?.FindFirst(ClaimTypes.NameIdentifier) is ClaimsIdentity identity)
        {
            diagnostics.Set("UserId", identity.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            diagnostics.Set("MerchantId", identity.FindFirst(AppClaimTypes.MerchantId)?.Value);
        }
    };
});
```

**Structured Logging in Handlers**

```csharp
_logger.LogInformation(
    "Creating product: {ProductName} for merchant {MerchantId}",
    request.Dto.Name, merchantId);

_logger.LogError(ex,
    "Failed to create product for merchant {MerchantId}",
    merchantId);
```

**Logging Behavior (MediatR)**

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);

        var timer = Stopwatch.StartNew();
        var response = await next();
        timer.Stop();

        _logger.LogInformation(
            "Completed {RequestName} in {ElapsedMilliseconds}ms",
            requestName, timer.ElapsedMilliseconds);

        return response;
    }
}
```

#### Mục đích

- 🎯 Production diagnostics
- 🎯 Request/response tracking
- 🎯 Performance monitoring

#### Đánh giá

- ✅ **Tốt**: Structured logging, Serilog + Seq setup
- ✅ Request/response logging middleware
- ✅ User/merchant context enriched

---

### ✅ Validation

#### Implementation

**FluentValidation Validator**

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private readonly ICategoryRepository _categoryRepo;

    public CreateProductCommandValidator(ICategoryRepository categoryRepo)
    {
        _categoryRepo = categoryRepo;

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(256).WithMessage("Product name must not exceed 256 characters");

        RuleFor(x => x.Dto.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThan(1_000_000_000).WithMessage("Price exceeds maximum");

        RuleFor(x => x.Dto.CategoryId)
            .NotEmpty().WithMessage("Category ID is required")
            .MustAsync(async (categoryId, ct) =>
                await _categoryRepo.AnyAsync(
                    new CategoryByIdSpec(categoryId), ct))
            .WithMessage("Category does not exist");
    }
}
```

**ValidationBehavior (Auto-run)**

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();  // No validators, proceed

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

**Exception Handling**

```csharp
// In ExceptionHandlingMiddleware
case ValidationException validationEx:
    statusCode = HttpStatusCode.BadRequest;
    apiError = new ApiError
    {
        Type = "validation-error",
        Message = "The following validation errors occurred.",
        Details = validationEx.Errors  // Field-level error messages
    };
    break;
```

**Response Format**

```json
{
  "data": null,
  "error": {
    "type": "validation-error",
    "message": "The following validation errors occurred.",
    "details": [
      {
        "propertyName": "Name",
        "errorMessage": "Product name is required."
      },
      {
        "propertyName": "Price",
        "errorMessage": "Price must be greater than 0."
      }
    ]
  },
  "meta": { ... }
}
```

#### Mục đích

- 🎯 Input validation before business logic
- 🎯 Auto-run via MediatR behavior
- 🎯 Localized Vietnamese error messages

#### Đánh giá

- ✅ **Tốt**: FluentValidation, auto-run, detailed errors
- ✅ Vietnamese messages
- ✅ Async validation support (database checks)

---

### ⚠️ Exception Handling

#### Implementation

**Custom Exception Hierarchy**

```csharp
public abstract class ApplicationExceptionBase : Exception { }

public class BadRequestException : ApplicationExceptionBase { }
public class ValidationException : ApplicationExceptionBase { }
public class BusinessRuleException : ApplicationExceptionBase { }
public class NotFoundException : ApplicationExceptionBase { }
public class ForbiddenException : ApplicationExceptionBase { }
public class ConflictException : ApplicationExceptionBase { }
public class ConcurrencyException : ApplicationExceptionBase { }
```

**Global Exception Handling Middleware**

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        HttpStatusCode statusCode;
        ApiError apiError;

        switch (ex)
        {
            case BadRequestException badRequestEx:
                statusCode = HttpStatusCode.BadRequest;
                apiError = new ApiError
                {
                    Type = "bad-request",
                    Message = badRequestEx.Message
                };
                break;

            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                apiError = new ApiError
                {
                    Type = "validation-error",
                    Message = validationEx.Message,
                    Details = validationEx.Errors  // Field-level details
                };
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                apiError = new ApiError
                {
                    Type = "not-found",
                    Message = notFoundEx.Message
                };
                break;

            case ForbiddenException forbiddenEx:
                statusCode = HttpStatusCode.Forbidden;
                apiError = new ApiError
                {
                    Type = "forbidden",
                    Message = forbiddenEx.Message
                };
                break;

            case ConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict;
                apiError = new ApiError
                {
                    Type = "conflict",
                    Message = conflictEx.Message
                };
                break;

            case ConcurrencyException concurrencyEx:
                statusCode = HttpStatusCode.Conflict;
                apiError = new ApiError
                {
                    Type = "concurrency-error",
                    Message = concurrencyEx.Message
                };
                break;

            default:
                _logger.LogError(ex, "Unhandled exception");
                statusCode = HttpStatusCode.InternalServerError;
                apiError = new ApiError
                {
                    Type = "internal-server-error",
                    Message = "An internal error occurred."
                    // NO stack trace in response (security)
                };
                break;
        }

        await WriteErrorResponse(context, statusCode, apiError);
    }

    private async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, ApiError error)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.Fail(error);
        response.Meta = new Meta
        {
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path,
            Method = context.Request.Method,
            StatusCode = (int)statusCode,
            TraceId = context.TraceIdentifier,
            ClientIp = context.Connection.RemoteIpAddress?.ToString()
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
```

**HTTP Status Mapping**

| Exception Type        | HTTP Status | Error Type                |
| --------------------- | ----------- | ------------------------- |
| BadRequestException   | 400         | "bad-request"             |
| ValidationException   | 400         | "validation-error"        |
| BusinessRuleException | 400         | "business-rule-violation" |
| NotFoundException     | 404         | "not-found"               |
| ForbiddenException    | 403         | "forbidden"               |
| ConflictException     | 409         | "conflict"                |
| ConcurrencyException  | 409         | "concurrency-error"       |
| Unhandled Exception   | 500         | "internal-server-error"   |

#### Mục đích

- 🎯 Graceful error handling
- 🎯 Consistent error response format
- 🎯 Prevent stack trace leakage

#### Đánh giá

- ✅ **Tốt**: Comprehensive exception mapping
- ✅ Middleware position (first in pipeline)
- ✅ No sensitive data leaked

---

### ⚡ Caching (Redis)

#### Implementation

**Redis Service**

```csharp
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _database.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, ttl);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _database.KeyDeleteAsync(key);
    }
}
```

**Caching Behavior (MediatR)**

```csharp
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheableQuery
{
    private readonly ICacheService _cacheService;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;

        // Try get from cache
        var cachedResult = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return cachedResult;
        }

        // Cache miss - execute handler
        var result = await next();

        // Set cache with TTL
        await _cacheService.SetAsync(cacheKey, result, request.CacheDuration, cancellationToken);

        return result;
    }
}
```

**Cacheable Query Interface**

```csharp
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}

// Example Query
public record GetProductDetailQuery(Guid ProductId)
    : IRequest<ProductDetailDto>, ICacheableQuery
{
    public string CacheKey => $"product-detail-{ProductId}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
```

#### Mục đích

- 🎯 Reduce database queries
- 🎯 Improve response time
- 🎯 Handle high-traffic read scenarios

#### Đánh giá

- ✅ **Tốt**: Redis integration, TTL support
- ✅ Automatic via behavior
- ⚠️ Cache invalidation (no explicit invalidation on updates - TTL only)

---

## 7️⃣ Ưu Điểm Của Hệ Thống

### ✅ 1. **Clean Architecture & Clear Separation**

- ✓ 5-layer architecture rõ ràng (Domain, Application, Common, Infrastructure, API)
- ✓ Dependencies flow từ ngoài vào trong (Onion pattern tuân thủ)
- ✓ Each layer has clear responsibility
- **Impact**: Dễ test, dễ maintain, dễ scale

### ✅ 2. **CQRS Pattern**

- ✓ Commands (write) & Queries (read) separated
- ✓ Handlers focused on single use case
- ✓ Easy to add validation, logging, caching independently
- **Impact**: Clear intent, easier to optimize, better testing

### ✅ 3. **Advanced Multi-Tenancy**

- ✓ Row-level security via EF Core global query filters
- ✓ 3-layer isolation (filters + auto-stamp + explicit checks)
- ✓ Hard to accidentally leak data
- ✓ Multiple resolution strategies (JWT claim, route param, query param)
- **Impact**: Multi-tenant isolation bulletproof, SaaS-ready

### ✅ 4. **Specification Pattern + Repository**

- ✓ Query logic centralized & reusable
- ✓ Prevents N+1 queries (Select projections)
- ✓ Testable queries
- ✓ Generic repository + feature-specific interfaces
- **Impact**: Optimized data access, consistent CRUD pattern

### ✅ 5. **Comprehensive Exception Handling**

- ✓ Typed exceptions (BadRequest, NotFound, Conflict, etc.)
- ✓ Global middleware catches everything
- ✓ Consistent error response format
- ✓ No stack trace leakage (security)
- **Impact**: Predictable API responses, better error diagnostics

### ✅ 6. **Structured Logging (Serilog + Seq)**

- ✓ Request/response logging middleware
- ✓ Structured data enriched (UserId, MerchantId, etc.)
- ✓ Centralized log server (Seq)
- ✓ Easy to trace requests
- **Impact**: Production diagnostics, performance monitoring

### ✅ 7. **MediatR Pipeline Behaviors**

- ✓ Validation automatically runs (no need to manually validate in each handler)
- ✓ Logging automatic
- ✓ Transactions automatic for commands
- ✓ Caching automatic for queries
- **Impact**: Less boilerplate, consistent behavior across handlers

### ✅ 8. **Strong Type Safety**

- ✓ C# strong typing
- ✓ Records for immutable DTOs
- ✓ Enums for statuses
- ✓ Fluent validation
- **Impact**: Compile-time errors caught, fewer runtime bugs

### ✅ 9. **SOLID Principles Well-Applied**

- ✓ SRP: Each class has single responsibility
- ✓ OCP: Can add features without modifying existing (extensions methods, behaviors, specs)
- ✓ LSP: Substitutable implementations (repositories, services)
- ✓ ISP: Segregated interfaces (ICurrentUserService just user context, etc.)
- ✓ DIP: Entire system depends on abstractions
- **Impact**: Maintainable, extensible, testable code

### ✅ 10. **Docker & Deployment Ready**

- ✓ Docker multi-stage build
- ✓ Docker Compose setup (API + SQL Server + Redis)
- ✓ Health checks endpoints (/health/live, /health/ready)
- ✓ Environment configuration (.env support)
- **Impact**: One-command deployment, prod-ready

### ✅ 11. **API Documentation**

- ✓ Swagger/OpenAPI integration
- ✓ API grouped by purpose (Identity, Management, Storefront, Admin)
- ✓ Each endpoint described
- **Impact**: Developer-friendly, clear API contract

### ✅ 12. **Rate Limiting & Security**

- ✓ Global rate limiter
- ✓ CORS configured
- ✓ JWT tokens with expiry
- ✓ Role-based authorization
- **Impact**: Production-grade security

---

## 8️⃣ Hạn Chế (Analysis Only)

### ⚠️ 1. **Over-Engineering cho Simple Operations**

- ❌ Mỗi endpoint cần Command + Validator + Handler + Response + Spec = 5 files minimum
- ❌ CQRS overkill cho simple CRUD
- ⚠️ More boilerplate than needed
- **Impact on scale**: Codebase grows quickly, maintenance effort increases

### ⚠️ 2. **Cache Invalidation**

- ❌ No explicit cache invalidation (only TTL-based)
- ❌ Product updated → cache still old for 5 minutes
- ⚠️ Potential stale data in reports/dashboards
- **Impact**: Data consistency issues in high-update scenarios

### ⚠️ 3. **Global Query Filters Complexity**

- ❌ Sometimes confusing (why entity not appearing? → filter!)
- ❌ Harder to debug queries
- ⚠️ Can accidentally return empty when filter silent fails
- **Impact**: Debugging difficulty, unexpected behaviors

### ⚠️ 4. **Middleware Pipeline Depth**

- ❌ 5+ middlewares in pipeline
- ⚠️ Each adds latency
- ⚠️ Hard to trace execution order
- **Impact**: Slight performance overhead, complex debugging

### ⚠️ 5. **SignalR Real-time May Not Scale**

- ❌ Only 1 OrderHub implemented
- ⚠️ SignalR doesn't scale horizontally (sticky sessions needed)
- ⚠️ If running multiple API instances, order updates may miss subscribers
- **Impact**: Scaling real-time features requires additional work (Redis backplane)

### ⚠️ 6. **No Explicit Transaction Rollback in Handlers**

- ❌ Relies on TransactionBehavior (implicit)
- ⚠️ If handler throws, transaction auto-rolls back (good)
- ⚠️ But no explicit `using` statement makes it less obvious
- **Impact**: Developers may not realize transactions active

### ⚠️ 7. **Feature Limit Enforcement**

- ❌ FeatureLimitFilter just returns 402 (payment required)
- ⚠️ No grace period / warning before limit hit
- ⚠️ No usage tracking UI for merchants
- **Impact**: Poor UX for merchants approaching limits

### ⚠️ 8. **No Soft Delete Restoration**

- ❌ Soft deletes (IsDeleted = true) but no restore functionality
- ⚠️ Data lost forever from merchant perspective
- **Impact**: Merchants can't undo accidental deletes

### ⚠️ 9. **No Audit Trail**

- ❌ No who-changed-what history
- ❌ Only CreatedAt/UpdatedAt timestamps
- ⚠️ Can't track "When was this product last modified?" or "Who deleted it?"
- **Impact**: No audit trail for compliance/debugging

### ⚠️ 10. **Specification Count**

- ❌ Dozens of `.cs` files for specifications
- ⚠️ Folder structure becomes cluttered
- ⚠️ Some repetitive (ProductByNameSpec, TableByIdSpec, etc.)
- **Impact**: Maintenance overhead, folder navigation complexity

---

## 9️⃣ Góc Nhìn Phỏng Vấn (RẤT QUAN TRỌNG)

### 🎤 Giới Thiệu Dự Án (3-5 Câu Ngắn Gọn)

**Phiên bản 1 (ngắn nhất):**

> QRDine là một multi-tenant SaaS platform cho quản lý nhà hàng. Chúng tôi xây dựng bằng .NET 8 Web API sử dụng Clean Architecture + CQRS pattern. Hệ thống cho phép chủ cửa hàng quản lý menu, bàn, đơn hàng, còn khách hàng quét QR để đặt hàng online. Đặc điểm chính: multi-tenancy (row-level security), real-time updates (SignalR), payment integration (PayOS).

**Phiên bản 2 (chi tiết trung bình):**

> QRDine là một SaaS platform cho ngành F&B, cho phép chủ cửa hàng (merchant) nhà hàng/cafe kỷ quản lý điểm bán hàng của họ thông qua một web dashboard. Khách hàng quét mã QR ở bàn để xem menu, chọn(order), và theo dõi trạng thái trong real-time.
>
> **Kiến trúc:**
>
> - **Backend**: .NET 8 Web API với Clean Architecture (5 layers: Domain, Application, Application.Common, Infrastructure, API)
> - **Pattern**: CQRS (Command Query Responsibility Segregation) + Repository + Specification
> - **Database**: SQL Server + EF Core 8
> - **Multi-Tenancy**: Row-level isolation via EF global query filters + JWT claims
> - **Real-time**: SignalR hubs cho live order updates
>
> **Chính sách bảng giá:**
>
> - Merchant có subscription-based (plan khác nhau)
> - Feature limits (ví dụ: free plan = 5 tables, pro = unlimited)
> - Payment via PayOS VNPay integration
>
> **Tech Stack:**
>
> - **Backend**: ASP.NET Core 8, EF Core 8, MediatR, FluentValidation
> - **Database**: SQL Server, Redis caching
> - **External**: Cloudinary (image), Brevo (email), PayOS (payment)
> - **DevOps**: Docker, Docker Compose
> - **Monitoring**: Serilog + Seq

### 💡 Điểm Mạnh Nên Highlight Khi Phỏng Vấn

#### 🔥 1. **Clean Architecture & CQRS**

> Dự án tuân thủ nguyên tắc Clean Architecture (Onion Architecture) rất nghiêm ngặt. Mỗi layer có trách nhiệm rõ ràng:
>
> - **Domain**: Business entities, no framework dependencies
> - **Application**: CQRS handlers + Specifications
> - **Infrastructure**: Database, external services, JWT
> - **API**: Controllers (thin), middleware
>
> CQRS pattern tách Commands (write) từ Queries (read), cho phép chúng tôi:
>
> - Validate input strictly (commands) vs gracefully filter (queries)
> - Cache query results mà không invalidate strategies
> - Scale read replicas separately
>
> **Why it matters in interviews**: Kiến trúc clean rất quantifiable - đặc biệt ở production systems.

#### 🔥 2. **Multi-Tenancy - Production-Ready**

> Multi-tenancy implementation có 3 layers:
>
> 1. **EF Core global query filters** - Automatic WHERE MerchantId = current
> 2. **Auto-stamping** - SaveChangesAsync tự động set MerchantId
> 3. **Explicit checks** - Handlers validate resource ownership
>
> Điều này gần như impossible để leak dữ liệu của merchant này sang merchant khác.
>
> **Why it matters**: Multi-tenancy đúng khó, rất ít developers làm well. SaaS businesses phụ thuộc vào đó.

#### 🔥 3. **Advanced Pattern Usage**

> - **Repository + Specification**: N+1 query prevention via Select() projections
> - **MediatR Pipeline Behaviors**: Validation, logging, transactions automatic (không need boilerplate)
> - **DIP (Dependency Inversion)**: Mỗi layer depend on abstractions, không concrete classes
>
> **Why it matters**: Cho thấy deep understanding của design patterns, không just copy-paste từ tutorials.

#### 🔥 4. **Separation of Concerns**

> - Validation: FluentValidation + ValidationBehavior (run automatically)
> - Logging: Serilog structured logging + request middleware + behavior
> - Error handling: Typed exceptions + global middleware mapping
> - Transactions: TransactionBehavior (command auto-wrapped)
>
> **Why it matters**: Maintainability at scale - chúng ta avoid "God classes" và scattered logic.

#### 🔥 5. **API Security & Auth**

> - JWT tokens (10-min access, 7-day refresh)
> - Role-based authorization (SuperAdmin, Merchant, Staff)
> - MerchantId resolution từ JWT claims (multi-tenant context)
> - CORS whitelist
> - Rate limiting
> - No sensitive data in error responses
>
> **Why it matters**: Phỏng vấn sẽ ask về security assumptions. Production system phải secure from start.

---

### ⚖️ Các Quyết Định Trade-off Đáng Chú Ý

**Q: Tại sao dùng CQRS?**

> CQRS có overhead (more files, more code), nhưng ở QRDine:
>
> - Read operations (queries) có thể cache 5 minutes (sales report queries)
> - Write operations (commands) luôn fresh
> - Nếu không tách, cache strategy phức tạp hơn (khi nào invalidate?)
>
> **Trade-off**: More boilerplate để gain independent optimization strategies.

**Q: Tại sao không dùng Microservices?**

> Monolith (không MS) chose vì:
>
> - Complexity overkill cho 1 SaaS product hiện tại
> - Multi-tenancy dễ implement ở single database (row-level filters)
> - Deploy/monitor simpler (1 service vs 5+)
> - Database transactions guarantee consistency
>
> **Future**: Nếu scale massive, có thể split (Billing service, Catalog service, etc.)

**Q: Generic Repository vs Specific?**

> Chúng tôi dùng **generic Repository<T>** base + **feature-specific interfaces** (IProductRepository, IOrderRepository):
>
> - Generic: Avoid code duplication (GetById, ListAsync, AddAsync, etc)
> - Specific: Namespacing + DI clarity (which repo for which feature?)
>
> **Alternative**: Không dùng repository (direct EF Core) - nhưng less testable, less abstraction.

**Q: EF Core Global Filters vs Explicit WHERE?**

> Global filters tự động, nhưng:
>
> - ✅ Less error-prone (developer không quên add WHERE)
> - ❌ Sometimes confusing (why empty result?)
>
> **Workaround**: Explicit MerchantId checks in handlers (defense-in-depth).

---

### 🎤 Chuẩn Bị Câu Hỏi Common

**"Làm sao bạn handle concurrency?"**

> EF Core configured với SQL Server optimistic locking:
>
> - Entities có `RowVersion` (timestamp)
> - Nếu 2 users update cùng entity → second one gets `DbUpdateConcurrencyException`
> - Exception mapped tới `ConcurrencyException` (409 Conflict)
> - Client displays: "Data was modified by someone else, please refresh"

**"Performance - làm sao scale?"**

> - **Queries**: Redis caching (TTL 5m). Query results cached automatically by CachingBehavior.
> - **Database**: SQL Server + indexing on MerchantId, CreatedAt
> - **Real-time**: SignalR Hubs (nhưng chỉ single instance hiện tại; would need Redis backplane for scale)
> - **Reads**: Can add read replicas (EF Core supports targeting read replicas)
> - **Writes**: EF bulk operations for batch inserts

**"Làm sao test system này?"**

> - **Unit tests**: Mock repositories, services. Handlers pure functions (just pass request, get response)
> - **Repository tests**: In-memory EF Core DbContext
> - **Integration tests**: TestContainers (Docker SQL Server instance)
> - **End-to-end**: API test calls real endpoints
> - See `/tests/QRDine.Application.Tests/` for examples

**"Multi-tenant security - có bug nào?"**

> - Row-level filters automatic (hard to leak)
> - Explicit ownership check in handlers (defense-in-depth)
> - JWT claims validation (must have valid merchant_id claim)
> - Only minor concern: cache invalidation (stale data for 5 min max)

**"Làm sao xử lý payment?"**

> - PayOS integration: Creates checkout link
> - Merchant redirected to PayOS → VNPay
> - Server-side webhook validates payment
> - If successful: Update Subscription status to Active
> - If failed: Update to Expired
> - Atomic transaction (all or nothing)

---

### 📊 Tự Đánh Giá (Self-Assessment)

Nếu bạn là developer của project này, đây là cách trả lời "Tại sao kiến trúc này tốt?":

| Aspect                     | Rating     | Evidence                                                       |
| -------------------------- | ---------- | -------------------------------------------------------------- |
| **Separation of Concerns** | ⭐⭐⭐⭐⭐ | 5 layers, clear responsibilities, behaviors for cross-cutting  |
| **Testability**            | ⭐⭐⭐⭐⭐ | Pure handlers, mock-friendly interfaces, Unit tests included   |
| **Scalability**            | ⭐⭐⭐⭐   | Redis caching, read replicas ready, async/await throughout     |
| **Security**               | ⭐⭐⭐⭐⭐ | JWT, multi-tenant isolation, rate limiting, error sanitization |
| **Maintainability**        | ⭐⭐⭐⭐   | CQRS clear intent, specifications named, logging comprehensive |
| **Code Reusability**       | ⭐⭐⭐⭐⭐ | Specs, behaviors, validators reused across handlers            |
| **Simplicity**             | ⭐⭐⭐     | CQRS has boilerplate, middleware pipeline depth                |
| **Production Readiness**   | ⭐⭐⭐⭐⭐ | Docker, health checks, error handling, structured logging      |

---

## 🔟 Tổng Kết

### 📈 Đánh Giá Tổng Thể

**Mức độ hệ thống:**

- **Junior** ❌
- **Mid-level** ⚠️ (có những design decisions advanced)
- **Senior** ✅ (clean architecture, CQRS, multi-tenancy, SOLID principles)

**Khả năng production:**

- ✅ **Fully production-ready** - Docker support, health checks, monitoring, error handling

**Độ maintainable:**

- ✅ **High** - Clear architecture, separation of concerns, testable code

**Khả năng scale:**

- ⭐⭐⭐⭐ (out of 5) - Caching, async/await, pero SignalR & queue cần optimization

---

### 🎓 Kỹ Năng Được Thể Hiện

1. **Architecture Knowledge**
   - ✅ Clean Architecture / Onion Architecture
   - ✅ CQRS pattern
   - ✅ Design Patterns (Repository, Specification, Mediator, etc.)

2. **Backend Development**
   - ✅ .NET 8 / ASP.NET Core
   - ✅ EF Core 8 (ORM mastery)
   - ✅ SQL Server
   - ✅ MediatR, FluentValidation, AutoMapper

3. **Software Engineering Principles**
   - ✅ SOLID principles applied correctly
   - ✅ DRY, KISS, YAGNI
   - ✅ Separation of Concerns

4. **Production Excellence**
   - ✅ Structured logging (Serilog + Seq)
   - ✅ Exception handling
   - ✅ Docker & containerization
   - ✅ Health checks & monitoring

5. **Database & Caching**
   - ✅ Multi-tenancy (row-level security)
   - ✅ Redis caching strategies
   - ✅ EF Core optimistic locking

6. **Security**
   - ✅ JWT authentication
   - ✅ Role-based authorization
   - ✅ Data isolation

---

### 💬 Phỏng Vấn - Best Approach

**Khi giải thích cho interviewer:**

1. **"Start with why"** - Tại sao kiến trúc như vậy? (SaaS requirements, scale)
2. **High-level first** - Mô tả tổng thể layer 5 lớp
3. **Deep-dive on expertise** - Chọn 1-2 pattern bạn proud of (ví dụ: multi-tenancy)
4. **Be ready for trade-offs** - Acknowledge downsides (CQRS boilerplate, cache invalidation)
5. **Show growth** - "Nếu scale 10x, chúng tôi sẽ..."

---

### 🚀 Kết Luận

**QRDine là một exemplary production-ready SaaS backend:**

- ✅ Senior-level architecture
- ✅ Comprehensive pattern usage
- ✅ Security & isolation by design
- ✅ Operational excellence (logging, monitoring, deployment)
- ⚠️ Slight over-engineering (CQRS overhead)
- ⚠️ Cache invalidation strategy (TTL-only)

**Điểm 8.5/10** cho một SaaS backend của team: Kiến trúc solid, patterns applied well, production-ready. Para 9.5/10, cần: stronger caching strategy, audit trail, explicit transaction management, real-time scaling (Redis backplane).

---

**Report Prepared By:** Senior Software Architect  
**Với kiến thức:** Clean Architecture, CQRS, .NET, SaaS  
**Suitable for:** Technical interviews, architecture reviews, team onboarding
