# Development Guidelines

Code standards, patterns, and best practices for contributing to QRDine.

## Code Standards

### C# Coding Conventions

- **Naming:** `PascalCase` for classes, methods, properties; `camelCase` for parameters and local variables
- **Access modifiers:** Always explicit (public, private, protected, internal)
- **Null coalescing:** Use `??` and `??=` operators
- **String interpolation:** Use `$"string {variable}"` instead of `string.Format`
- **LINQ:** Prefer method syntax over query syntax
- **Async:** Use `async/await`; never use `.Result` or `.Wait()`
- **Disposables:** Use `using` statements for `IDisposable`

### File Organization

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
// ... other usings alphabetically

namespace QRDine.Application.Features.Catalog.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(CreateCategoryRequest Req) : IRequest<CreateCategoryResponse>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    // Implementation
}
```

**Order:**

1. Using statements (alphabetically)
2. Namespace declaration
3. Public types (records, classes, interfaces)
4. Private classes (handlers, validators)

### Comments

- Avoid obvious comments
- Document "why" not "what"
- Use XML documentation for public APIs

```csharp
// Good
/// <summary>
/// Creates a category only if duplicate check passes.
/// </summary>
/// <remarks>
/// Throws BusinessRuleException if category name already exists for this merchant.
/// </remarks>
public async Task<CreateCategoryResponse> Handle(/* ... */)
{
    // Validate uniqueness before insert to fail fast
    var existingCategory = await _repository.FirstOrDefaultAsync(
        spec => spec.Name == request.Name && spec.MerchantId == merchantId);

    if (existingCategory != null)
        throw new BusinessRuleException("Category name must be unique");
}

// Avoid
public class CategoryService
{
    // Creates a category
    public async Task<CreateCategoryResponse> CreateCategory(/* ... */)
    {
        // Check if category exists
        var exists = /* ... */;
        // If exists, throw exception
        if (exists)
            throw new Exception("Exists");
    }
}
```

## CQRS Patterns

### Command Pattern

Commands are used for write operations (Create, Update, Delete).

```csharp
// File: CreateCategoryCommand.cs
public record CreateCategoryCommand(CreateCategoryRequest Req) : IRequest<CreateCategoryResponse>;

// File: CreateCategoryCommandValidator.cs
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Req.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(255).WithMessage("Category name must not exceed 255 characters");
    }
}

// File: CreateCategoryCommandHandler.cs
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(
        IRepository<Category> categoryRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var merchantId = _currentUserService.MerchantId!.Value;

        // Business validation
        var existingCategory = await _categoryRepository.FirstOrDefaultAsync(
            new CategoriesByNameSpecs(merchantId, request.Req.Name),
            cancellationToken);

        if (existingCategory != null)
            throw new BusinessRuleException("Category with this name already exists");

        // Create entity
        var category = new Category
        {
            Name = request.Req.Name,
            Description = request.Req.Description,
            DisplayOrder = request.Req.DisplayOrder ?? 0
            // MerchantId auto-stamped by SaveChangesAsync
        };

        // Persist
        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CreateCategoryResponse>(category);
    }
}
```

**Command Guidelines:**

1. Use `record` type for immutability
2. Inherit from `IRequest<TResponse>`
3. Create dedicated validator implementing `AbstractValidator<T>`
4. Handler implements `IRequestHandler<TCommand, TResponse>`
5. Always validate before business logic
6. Throw appropriate exceptions (not generic `Exception`)
7. Use specifications for complex queries

### Query Pattern

Queries are for read operations.

```csharp
// File: GetCategoriesByMerchantQuery.cs
public record GetCategoriesByMerchantQuery(Guid MerchantId) : IRequest<List<CategoryDto>>;

// File: GetCategoriesByMerchantQueryHandler.cs
public class GetCategoriesByMerchantQueryHandler : IRequestHandler<GetCategoriesByMerchantQuery, List<CategoryDto>>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoriesByMerchantQueryHandler(
        IRepository<Category> categoryRepository,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesByMerchantQuery request, CancellationToken cancellationToken)
    {
        var spec = new CategoriesByMerchantIdSpecification(request.MerchantId)
            .WithoutDeleted();

        var categories = await _categoryRepository.ListAsync(spec, cancellationToken);
        return _mapper.Map<List<CategoryDto>>(categories);
    }
}
```

**Query Guidelines:**

1. No side effects (read-only)
2. Use specifications for filtering
3. Project to DTOs, not domain entities
4. Return appropriate collection types (List, IEnumerable, etc.)

## Repository Pattern

Use the generic repository with specifications:

```csharp
// Get single entity
var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

// Get with specification
var spec = new CategoryWithProductsSpecification(categoryId);
var category = await _categoryRepository.FirstOrDefaultAsync(spec, cancellationToken);

// List with specification
var spec = new ActiveCategoriesSpecification(merchantId).OrderBy(c => c.DisplayOrder);
var categories = await _categoryRepository.ListAsync(spec, cancellationToken);

// Add new entity
var category = new Category { Name = "Appetizers" };
await _categoryRepository.AddAsync(category, cancellationToken);
await _categoryRepository.SaveChangesAsync(cancellationToken);

// Update entity
category.Name = "Starters";
category.UpdatedAt = DateTime.UtcNow;
await _categoryRepository.UpdateAsync(category, cancellationToken);

// Soft delete
await _categoryRepository.DeleteAsync(category, cancellationToken); // Sets IsDeleted = true
```

## Specification Pattern

Encapsulate query logic in specifications:

```csharp
public class CategoriesByMerchantIdSpecification : BaseSpecification<Category>
{
    public CategoriesByMerchantIdSpecification(Guid merchantId)
    {
        Query
            .Where(c => c.MerchantId == merchantId && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder);
    }
}

public class CategoryWithProductsSpecification : BaseSpecification<Category>
{
    public CategoryWithProductsSpecification(Guid categoryId)
    {
        Query
            .Where(c => c.Id == categoryId && !c.IsDeleted)
            .Include(c => c.Products.Where(p => !p.IsDeleted));
    }
}

// Usage
var spec = new CategoriesByMerchantIdSpecification(merchantId);
var categories = await _categoryRepository.ListAsync(spec);
```

**Benefits:**

- Reusable query logic
- Testable specifications
- Separation of concerns
- Single responsibility

## DTOs (Data Transfer Objects)

Separate request and response models from domain entities:

```csharp
// Request DTO
public class CreateCategoryRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = default!;

    [StringLength(1000)]
    public string? Description { get; set; }

    public int? DisplayOrder { get; set; }
}

// Response DTO
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

// Mapper profile
public class CatalogMapperProfile : Profile
{
    public CatalogMapperProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count(p => !p.IsDeleted)));

        CreateMap<CreateCategoryRequest, Category>();
    }
}
```

## Validation Pattern

Use FluentValidation for all input validation:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Req.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(255).WithMessage("Product name is too long");

        RuleFor(x => x.Req.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Price is unreasonably high");

        RuleFor(x => x.Req.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.Req.Description)
            .MaximumLength(2000).WithMessage("Description is too long");
    }
}
```

**Guidelines:**

- Validate input format (length, type, ranges)
- Throw `ValidationException` if validation fails
- Use FluentValidation, not manual if/else
- Combine with database checks for uniqueness constraints

## Exception Handling

Use appropriate custom exceptions:

```csharp
// Validation error
throw new ValidationException("Input validation failed", errors: new[]
{
    new ValidationFailure("name", "Name is required")
});

// Business rule violation
throw new BusinessRuleException("Product with same name already exists");

// Not found
throw new NotFoundException("Category", categoryId);

// Access denied
throw new ForbiddenException("You do not have permission to update this category");

// Resource conflict
throw new ConflictException("Merchant has active subscription");

// Optimistic concurrency
throw new ConcurrencyException("Data was modified by another user");
```

## Controller Pattern

Keep controllers thin; dispatch to MediatR:

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management/categories")]
[Authorize(Roles = "Merchant,Staff")]
[ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<CreateCategoryResponse>> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(request);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        [FromRoute] Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(id, request);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
```

**Guidelines:**

- No business logic in controllers
- Use dependency injection for required services
- Return appropriate status codes
- Use async/await
- Validate authorization via attributes

## Testing

Create unit tests for:

1. Command handlers
2. Query handlers
3. Validators
4. Domain entities
5. Specifications

```csharp
[Fact]
public async Task CreateCategory_WithValidInput_ReturnsSuccess()
{
    // Arrange
    var repository = new Mock<IRepository<Category>>();
    var mapper = new Mock<IMapper>();
    var currentUser = new Mock<ICurrentUserService>();

    currentUser.Setup(x => x.MerchantId).Returns(Guid.NewGuid());
    var handler = new CreateCategoryCommandHandler(repository.Object, currentUser.Object, mapper.Object);
    var command = new CreateCategoryCommand(new CreateCategoryRequest { Name = "Appetizers" });

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    repository.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

## Related Documentation

- [Architecture Overview](../architecture/overview.md)
- [API Conventions](../api/overview.md)
- [Configuration Guide](../configuration/environment.md)
