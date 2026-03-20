# Unit Testing - Complete Guide

📚 **Complete reference** for writing, organizing, and maintaining unit tests in the QRDine application layer.

---

## Table of Contents

1. [Test Architecture](#test-architecture)
2. [Test Infrastructure](#test-infrastructure)
3. [Building Test Data](#building-test-data)
4. [Command Handler Tests](#command-handler-tests)
5. [Service Layer Tests](#service-layer-tests)
6. [Mocking Patterns](#mocking-patterns)
7. [Advanced Scenarios](#advanced-scenarios)
8. [Run & Debug](#run--debug)
9. [Contributing Tests](#contributing-tests)

---

## Test Architecture

### Layered Approach

Tests are organized to match application layers:

```
Tests ←→ CQRS Handlers/Services ←→ Repositories ←→ Database (Mocked)
         (Application Layer)      (Infrastructure)
```

**What We Test:**
- ✅ Command & Query handlers using CQRS
- ✅ Service layer business logic
- ✅ Validation and exception handling
- ✅ Authorization and multi-tenancy

**What We Don't Test:**
- ❌ EF Core DbContext behavior
- ❌ SQL queries or database schema
- ❌ Repository implementation (use mocks)
- ❌ Infrastructure details (SignalR, Cloudinary)

### Test Organization by Module

Each feature module (Catalog, Sales, Billing, etc.) follows the same structure:

```
Features/{Module}/
├── {Entity}/
│   ├── Commands/
│   │   ├── {Action}/
│   │   │   └── {Action}{Entity}CommandHandlerTests.cs
│   │   │
│   │   └── ...
│   │
│   ├── Queries/
│   │   └── {GetWhat}{QueryName}QueryHandlerTests.cs (rare - focus on commands)
│   │
│   └── Services/
│       └── {ServiceName}Tests.cs
│
└── {SecondEntity}/
    └── Commands/...
```

**Example Structure:**

```
Features/
├── Catalog/
│   ├── Categories/
│   │   ├── Commands/Create/CreateCategoryCommandHandlerTests.cs
│   │   ├── Commands/Update/UpdateCategoryCommandHandlerTests.cs
│   │   └── Commands/Delete/DeleteCategoryCommandHandlerTests.cs
│   │
│   ├── Products/
│   │   └── Commands/...
│   │
│   └── Tables/
│       └── Commands/...
│
├── Sales/
│   ├── Orders/
│   │   ├── Commands/
│   │   │   ├── ManagementCreateOrder/ManagementCreateOrderCommandHandlerTests.cs
│   │   │   ├── StorefrontCreateOrder/StorefrontCreateOrderCommandHandlerTests.cs
│   │   │   └── CloseOrder/CloseOrderCommandHandlerTests.cs
│   │   │
│   │   └── Services/
│   │       ├── OrderCreationServiceTests.cs
│   │       └── OrderFormattingServiceTests.cs
│   │
│   └── OrderItems/
│       └── Commands/...
│
└── Billing/
    └── ...
```

---

## Test Infrastructure

### 1. Builders - Test Data Factories

**Location:** `Common/Builders/{Module}/`

Builders encapsulate object creation logic in a fluent, readable manner:

#### Basic Usage

```csharp
// Creating an entity
var category = new CategoryBuilder()
    .WithId(categoryId)
    .WithMerchantId(merchantId)
    .WithName("Electronics")
    .WithDescription("Electronic products")
    .WithParentId(null)
    .WithDisplayOrder(1)
    .Build();

// Creating a DTO
var createDto = new CreateCategoryDtoBuilder()
    .WithName("Food & Beverage")
    .WithDescription("Food products")
    .WithParentId(parentId)
    .Build();
```

#### Builder Implementation Pattern

All builders follow this pattern:

```csharp
public class CategoryBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _merchantId = Guid.NewGuid();
    private string _name = "Default Category";
    private string _description = string.Empty;
    private Guid? _parentId = null;
    private int _displayOrder = 0;
    private bool _isDeleted = false;

    public CategoryBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CategoryBuilder WithMerchantId(Guid merchantId)
    {
        _merchantId = merchantId;
        return this;
    }

    // ... more With* methods

    public Category Build()
    {
        return new Category
        {
            Id = _id,
            MerchantId = _merchantId,
            Name = _name,
            Description = _description,
            ParentId = _parentId,
            DisplayOrder = _displayOrder,
            IsDeleted = _isDeleted
        };
    }
}
```

**Key Points:**
- Every builder has **default values** (sensible defaults)
- Builders support **method chaining** (fluent API)
- Builders use **public setters** for clarity
- Final method is always `Build()` which constructs the object

**Catalog Builders:**
| Builder | Creates | Purpose |
|---------|---------|---------|
| `CategoryBuilder` | Category entity | Parent/nested category trees |
| `CreateCategoryDtoBuilder` | CreateCategoryDto | Tests creating categories |
| `UpdateCategoryDtoBuilder` | UpdateCategoryDto | Tests updating categories |
| `ProductBuilder` | Product entity | Product with prices, images |
| `CreateProductDtoBuilder` | CreateProductDto | Product creation tests |
| `TableBuilder` | Table entity | QR table mapping |
| `ToppingGroupBuilder` | ToppingGroup entity | Topping option groups |

**Sales Builders:**
| Builder | Creates | Purpose |
|---------|---------|---------|
| `OrderBuilder` | Order entity | Orders with items and status |
| `OrderItemBuilder` | OrderItem entity | Individual order line items |
| `OrderCreationDtoBuilder` | OrderCreationDto | Internal service DTO |
| `ManagementCreateOrderDtoBuilder` | ManagementCreateOrderDto | Management order creation |
| `StorefrontCreateOrderDtoBuilder` | StorefrontCreateOrderDto | Storefront order creation |

**Billing Builders:**
| Builder | Creates | Purpose |
|---------|---------|---------|
| `PlanBuilder` | Plan entity | Subscription plans |
| `SubscriptionBuilder` | Subscription entity | Active subscriptions |
| `SubscriptionCheckoutBuilder` | SubscriptionCheckoutDto | Checkout flow |
| `TransactionBuilder` | Transaction entity | Payment transactions |

### 2. Mocks - Repository & Service Factories

**Location:** `Common/Mocks/{Module}/`

Mocks provide pre-configured mock objects:

```csharp
// Repository Mocks
public static class CatalogRepositoryMocks
{
    public static Mock<ICategoryRepository> CreateCategoryRepositoryMock()
    {
        var mock = new Mock<ICategoryRepository>();
        // Configure common setups if needed
        return mock;
    }

    public static Mock<IProductRepository> CreateProductRepositoryMock()
    {
        var mock = new Mock<IProductRepository>();
        return mock;
    }
}

// Service Mocks
public static class CatalogServiceMocks
{
    public static Mock<IMapper> CreateMapperMock()
    {
        return new Mock<IMapper>();
    }

    public static Mock<INotificationService> CreateNotificationServiceMock()
    {
        return new Mock<INotificationService>();
    }
}
```

**Usage in Tests:**

```csharp
public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        // Use factory for consistency
        _categoryRepo = CatalogRepositoryMocks.CreateCategoryRepositoryMock();
        _handler = new CreateCategoryCommandHandler(_categoryRepo.Object);
    }
}
```

### 3. Fixtures - Shared Test Context

**Location:** `Common/Fixtures/`

Fixtures provide **consistent GUIDs** across tests:

```csharp
public class CatalogFixture : IDisposable
{
    // Merchant context
    public readonly Guid MerchantId = new Guid("11111111-1111-1111-1111-111111111111");

    // Categories
    public readonly Guid CategoryId = new Guid("22222222-2222-2222-2222-222222222222");
    public readonly Guid SubCategoryId = new Guid("33333333-3333-3333-3333-333333333333");

    // Products
    public readonly Guid ProductId = new Guid("44444444-4444-4444-4444-444444444444");
    public readonly Guid ProductId2 = new Guid("55555555-5555-5555-5555-555555555555");

    // Tables
    public readonly Guid TableId = new Guid("66666666-6666-6666-6666-666666666666");

    // Topping groups
    public readonly Guid ToppingGroupId = new Guid("77777777-7777-7777-7777-777777777777");

    public void Dispose()
    {
        // Cleanup if needed
    }
}
```

**Benefits:**
- Consistent IDs across all tests in a module
- Easy to create related entities (category→products)
- Enables verifying correct IDs passed to repositories
- Simplifies complex multi-entity scenarios

---

## Building Test Data

### Pattern 1: Using Builders Only

For simple tests without repository interactions:

```csharp
[Fact]
public async Task Handle_WhenValid_ShouldReturnCorrectMapping()
{
    // Arrange - just build the data
    var createDto = new CreateCategoryDtoBuilder()
        .WithName("Electronics")
        .Build();

    var command = new CreateCategoryCommand(_fixture.MerchantId, createDto);

    _categoryRepo.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((Category cat) => cat); // Return the added category

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("Electronics");
}
```

### Pattern 2: Builders + Fixture for Relationships

For tests involving related entities:

```csharp
[Fact]
public async Task Handle_UpdateProduct_ShouldMaintainCategoryRelationship()
{
    // Arrange
    var category = new CategoryBuilder()
        .WithId(_fixture.CategoryId)
        .WithMerchantId(_fixture.MerchantId)
        .Build();

    var product = new ProductBuilder()
        .WithId(_fixture.ProductId)
        .WithCategoryId(_fixture.CategoryId)  // Use fixture constant
        .WithName("Laptop")
        .Build();

    var updateDto = new UpdateProductDtoBuilder()
        .WithName("Pro Laptop")
        .WithCategoryId(_fixture.CategoryId)
        .Build();

    var command = new UpdateProductCommand(_fixture.ProductId, updateDto);

    _categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(category);
    
    _productRepo.Setup(x => x.GetByIdAsync(_fixture.ProductId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(product);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.CategoryId.Should().Be(_fixture.CategoryId);
}
```

### Pattern 3: Complex Object Graphs

For nested hierarchies:

```csharp
[Fact]
public async Task Handle_CloseOrder_ShouldRecalculateTotalAmount()
{
    // Arrange
    var order = new OrderBuilder()
        .WithId(_fixture.OrderId)
        .WithMerchantId(_fixture.MerchantId)
        .WithStatus(OrderStatus.Open)
        .Build();

    var item1 = new OrderItemBuilder()
        .WithProductId(_fixture.ProductId)
        .WithStatus(OrderItemStatus.Pending)
        .WithQuantity(2)
        .WithUnitPrice(50000m)
        .WithAmount(100000m)
        .Build();

    var item2 = new OrderItemBuilder()
        .WithProductId(_fixture.ProductId2)
        .WithStatus(OrderItemStatus.Pending)
        .WithQuantity(1)
        .WithUnitPrice(30000m)
        .WithAmount(30000m)
        .Build();

    order.OrderItems.Add(item1);
    order.OrderItems.Add(item2);

    var command = new CloseOrderCommand(_fixture.OrderId, OrderStatus.Paid);

    _orderRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Order>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(order);
    _orderRepo.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeTrue();
    item1.Status.Should().Be(OrderItemStatus.Served);
    item2.Status.Should().Be(OrderItemStatus.Served);
}
```

---

## Command Handler Tests

### Test Class Template

```csharp
namespace QRDine.Application.Tests.Features.{Module}.{Entity}.Commands.{Action}
{
    public class {Action}{Entity}CommandHandlerTests
    {
        // 1. Dependencies
        private readonly Mock<IRepository> _repository;
        private readonly Mock<IService> _service;
        private readonly {Action}{Entity}CommandHandler _handler;
        private readonly {Module}Fixture _fixture;

        // 2. Constructor - Setup
        public {Action}{Entity}CommandHandlerTests()
        {
            _fixture = new {Module}Fixture();
            _repository = new Mock<IRepository>();
            _service = new Mock<IService>();
            _handler = new {Action}{Entity}CommandHandler(_repository.Object, _service.Object);
        }

        // 3. Happy path test
        [Fact]
        public async Task Handle_ValidRequest_Should{ExpectedBehavior}()
        {
            // Arrange
            var command = new {Action}{Entity}Command(...);
            _repository.Setup(...).ReturnsAsync(...);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            _repository.Verify(..., Times.Once);
        }

        // 4. Exception test
        [Fact]
        public async Task Handle_{ErrorScenario}_ShouldThrow{Exception}()
        {
            // Arrange
            _repository.Setup(...).ReturnsAsync((Entity?)null);

            // Act & Assert
            await Assert.ThrowsAsync<{Exception}>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }

        // 5. Multi-step tests
        [Theory]
        [InlineData(Status1)]
        [InlineData(Status2)]
        public async Task Handle_WithVariousStates_Should{ExpectedBehavior}(Status status)
        {
            // Test multiple scenarios
        }
    }
}
```

### Specific Command Handler Examples

#### 1. CREATE Command

Tests for entity creation:

```csharp
[Fact]
public async Task Handle_ValidRequest_ShouldCreateCategorySuccessfully()
{
    // Arrange
    var createDto = new CreateCategoryDtoBuilder()
        .WithName("Beverages")
        .Build();
    var command = new CreateCategoryCommand(_fixture.MerchantId, createDto);

    _categoryRepo.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Category>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);  // Name doesn't exist

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("Beverages");
    _categoryRepo.Verify(
        x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
        Times.Once
    );
}

[Fact]
public async Task Handle_NameAlreadyExists_ShouldThrowConflictException()
{
    // Arrange
    var createDto = new CreateCategoryDtoBuilder()
        .WithName("Existing")
        .Build();

    _categoryRepo.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Category>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);  // Name already exists

    // Act & Assert
    await Assert.ThrowsAsync<ConflictException>(
        () => _handler.Handle(command, CancellationToken.None)
    );
}
```

#### 2. UPDATE Command

Tests for entity modifications:

```csharp
[Fact]
public async Task Handle_ValidUpdate_ShouldUpdateProperties()
{
    // Arrange
    var existingCategory = new CategoryBuilder()
        .WithId(_fixture.CategoryId)
        .WithName("Old Name")
        .Build();

    var updateDto = new UpdateCategoryDtoBuilder()
        .WithName("New Name")
        .Build();

    var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);

    _categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCategory);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Name.Should().Be("New Name");
    _categoryRepo.Verify(
        x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
        Times.Once
    );
}

[Fact]
public async Task Handle_EntityNotFound_ShouldThrowNotFoundException()
{
    // Arrange
    _categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Category?)null);

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => _handler.Handle(command, CancellationToken.None)
    );
}
```

#### 3. DELETE Command

Tests for soft/hard deletes:

```csharp
[Fact]
public async Task Handle_ExistingEntity_ShouldDelete()
{
    // Arrange
    var existingCategory = new CategoryBuilder()
        .WithId(_fixture.CategoryId)
        .Build();

    var command = new DeleteCategoryCommand(_fixture.CategoryId);

    _categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCategory);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    _categoryRepo.Verify(
        x => x.DeleteAsync(existingCategory, It.IsAny<CancellationToken>()),
        Times.Once
    );
}
```

#### 4. AUTHORIZATION Tests

Tests for multi-tenant security:

```csharp
[Fact]
public async Task Handle_DifferentMerchantOwner_ShouldThrowForbiddenException()
{
    // Arrange
    var otherMerchantId = Guid.NewGuid();
    var existingCategory = new CategoryBuilder()
        .WithId(_fixture.CategoryId)
        .WithMerchantId(_fixture.MerchantId)  // Owned by merchant1
        .Build();

    var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto)
    {
        MerchantId = otherMerchantId  // Trying to update as merchant2
    };

    _categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCategory);

    // Act & Assert
    await Assert.ThrowsAsync<ForbiddenException>(
        () => _handler.Handle(command, CancellationToken.None)
    );
}
```

---

## Service Layer Tests

Service tests verify complex business logic:

### OrderCreationService Example

```csharp
public class OrderCreationServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository;
    private readonly Mock<IProductRepository> _productRepository;
    private readonly Mock<ITableRepository> _tableRepository;
    private readonly OrderCreationService _service;
    private readonly SalesFixture _fixture;

    public OrderCreationServiceTests()
    {
        _fixture = new SalesFixture();
        _orderRepository = SalesRepositoryMocks.CreateOrderRepositoryMock();
        _productRepository = new Mock<IProductRepository>();
        _tableRepository = new Mock<ITableRepository>();
        _service = new OrderCreationService(
            _orderRepository.Object,
            _productRepository.Object,
            _tableRepository.Object
        );
    }

    [Fact]
    public async Task CreateOrAppendOrderAsync_TableNotExists_ShouldThrowNotFoundException()
    {
        var model = new OrderCreationDtoBuilder()
            .WithMerchantId(_fixture.MerchantId)
            .WithTableId(_fixture.TableId)
            .Build();

        _tableRepository
            .Setup(x => x.GetByIdAsync(_fixture.TableId, CancellationToken.None))
            .ReturnsAsync((Table?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateOrAppendOrderAsync(model, CancellationToken.None)
        );
    }
}
```

---

## Mocking Patterns

### Repository Mocking

```csharp
// Setup GetById
_categoryRepo.Setup(x => x.GetByIdAsync(categoryId, CancellationToken.None))
    .ReturnsAsync(category);

// Setup List with Specification
_categoryRepo.Setup(x => x.ListAsync(
    It.IsAny<ISpecification<Category>>(),
    It.IsAny<CancellationToken>()
))
.ReturnsAsync(new List<Category> { category1, category2 });

// Setup Any (for existence checks)
_categoryRepo.Setup(x => x.AnyAsync(
    It.IsAny<ISpecification<Category>>(),
    It.IsAny<CancellationToken>()
))
.ReturnsAsync(true);

// Setup Add
_categoryRepo.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
.Returns(Task.CompletedTask);

// Setup Update
_categoryRepo.Setup(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
.Returns(Task.CompletedTask);

// Setup Delete
_categoryRepo.Setup(x => x.DeleteAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
.Returns(Task.CompletedTask);
```

### Service Mocking

```csharp
// Mock AutoMapper
var mapper = new Mock<IMapper>();
mapper.Setup(x => x.Map<CategoryResponseDto>(It.IsAny<Category>()))
    .Returns((Category cat) => new CategoryResponseDto
    {
        Id = cat.Id,
        Name = cat.Name,
        Description = cat.Description
    });

// Mock Notification Service
var notificationService = new Mock<IOrderNotificationService>();
notificationService.Setup(x => x.NotifyOrderUpdatedAsync(
    It.IsAny<Guid>(),
    It.IsAny<Guid>(),
    It.IsAny<string>(),
    It.IsAny<CancellationToken>()
))
.Returns(Task.CompletedTask);
```

### Using It.IsAny for Flexible Matching

```csharp
// Match any value of type
_categoryRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(category);

// Match any string
_categoryRepo.Setup(x => x.ListAsync(
    It.Is<ISpecification<Category>>(s => s.Name.Contains("test")),
    It.IsAny<CancellationToken>()
))
.ReturnsAsync(categories);

// Match with predicate
_categoryRepo.Setup(x => x.AnyAsync(
    It.Is<ISpecification<Category>>(s => s.MerchantId == _fixture.MerchantId),
    It.IsAny<CancellationToken>()
))
.ReturnsAsync(true);
```

---

## Advanced Scenarios

### Multi-Step Workflows

Test complex sequential operations:

```csharp
[Fact]
public async Task Handle_CompleteOrderFlow_ShouldUpdateMultipleEntities()
{
    // Arrange
    var order = new OrderBuilder().WithStatus(OrderStatus.Open).Build();
    var item1 = new OrderItemBuilder().WithStatus(OrderItemStatus.Pending).Build();
    var item2 = new OrderItemBuilder().WithStatus(OrderItemStatus.Preparing).Build();
    order.OrderItems.Add(item1);
    order.OrderItems.Add(item2);

    var command = new CloseOrderCommand(_fixture.OrderId, OrderStatus.Paid);

    _orderRepository
        .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Order>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(order);
    _orderRepository
        .Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    _tableRepository
        .Setup(x => x.UpdateAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeTrue();
    item1.Status.Should().Be(OrderItemStatus.Served);
    item2.Status.Should().Be(OrderItemStatus.Served);
    order.Status.Should().Be(OrderStatus.Paid);
    
    _orderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Exception Message Validation

```csharp
[Fact]
public async Task Handle_ValidationFails_ShouldIncludeDetailedMessage()
{
    // Arrange
    var command = new CreateCategoryCommand(_fixture.MerchantId, new CreateCategoryDto { Name = "" });

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ValidationException>(
        () => _handler.Handle(command, CancellationToken.None)
    );

    exception.Message.Should().Contain("Category name");
}
```

### Testing Callbacks & Side Effects

```csharp
[Fact]
public async Task Handle_WhenItemUpdated_ShouldModifyOrderTotal()
{
    // Arrange
    var order = new OrderBuilder().WithTotalAmount(100000m).Build();
    var item = new OrderItemBuilder().WithAmount(50000m).Build();

    _orderRepository
        .Setup(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<OrderItem>>(), It.IsAny<CancellationToken>()))
        .Callback<IEnumerable<OrderItem>, CancellationToken>((items, _) =>
        {
            var itemList = items.ToList();
            itemList[0].Amount = 75000m;  // Simulate update
        })
        .Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    item.Amount.Should().Be(75000m);
}
```

### Theory Tests (Parametrized)

```csharp
[Theory]
[InlineData(OrderStatus.Open, OrderItemStatus.Pending, OrderItemStatus.Served)]
[InlineData(OrderStatus.Open, OrderItemStatus.Preparing, OrderItemStatus.Served)]
[InlineData(OrderStatus.Cancelled, OrderItemStatus.Pending, OrderItemStatus.Cancelled)]
public async Task Handle_WithVariousStatuses_ShouldTransitionCorrectly(
    OrderStatus orderStatus,
    OrderItemStatus fromStatus,
    OrderItemStatus expectedStatus)
{
    // Arrange
    var item = new OrderItemBuilder().WithStatus(fromStatus).Build();
    var order = new OrderBuilder().WithStatus(orderStatus).Build();
    order.OrderItems.Add(item);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    item.Status.Should().Be(expectedStatus);
}
```

---

## Run & Debug

### Command Line

```bash
# Run all tests
dotnet test tests/QRDine.Application.Tests/

# Run specific module
dotnet test tests/QRDine.Application.Tests/ --filter "Catalog"

# Run specific class
dotnet test tests/QRDine.Application.Tests/ --filter "CreateCategoryCommandHandlerTests"

# Run specific test method
dotnet test tests/QRDine.Application.Tests/ --filter "CreateCategoryCommandHandlerTests.Handle_ValidRequest_ShouldCreateSuccessfully"

# Verbose output
dotnet test tests/QRDine.Application.Tests/ -v normal

# With code coverage
dotnet test tests/QRDine.Application.Tests/ /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Debug in VS Code

1. **Set breakpoint** in test code
2. **Open Run & Debug** (Ctrl+Shift+D)
3. **Create `.vscode/launch.json`** if not exists:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Attach",
            "type": "coreclr",
            "request": "attach",
            "processId": "${command:pickProcess}"
        },
        {
            "name": ".NET Core Test Debug",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/**/**/bin/Debug/**/dotnet",
            "args": ["test", "${workspaceFolder}/tests/QRDine.Application.Tests/"],
            "cwd": "${workspaceFolder}/tests/QRDine.Application.Tests/",
            "stopAtEntry": false
        }
    ]
}
```

4. **Press F5** to debug

---

## Contributing Tests

### Before Submitting

✅ **Checklist:**
- [ ] Test follows module naming convention
- [ ] Uses Builders for all test data
- [ ] Mocks external dependencies
- [ ] Includes happy path ✅ test
- [ ] Includes exception ❌ test
- [ ] Verifies side effects with `.Verify()`
- [ ] Uses descriptive test names
- [ ] All tests pass: `dotnet test`
- [ ] No commented code

### PR Requirements

When adding new command/service:

1. **Create test file** in appropriate folder
2. **Write AAA tests** covering:
   - Happy path
   - Exception cases (NotFoundException, ConflictException, ForbiddenException)
   - Authorization if applicable
   - Side effects/verification
3. **Update builders** if new DTOs added
4. **Verify all pass** before submitting PR

### Example PR Contribution

```bash
# Create feature branch
git checkout -b feature/sales-order-tracking

# Write handler + tests
# Update builders/mocks/fixtures

# Run tests
dotnet test tests/QRDine.Application.Tests/ --filter "Sales"

# Should output: "Passed: XX, Failed: 0"

git add .
git commit -m "feat(sales): Add order tracking commands and tests"
git push origin feature/sales-order-tracking
```

---

## Reference

- [Testing README](README.md) — Quick start and patterns overview
- [CQRS Development Guide](../README.md#cqrs-patterns) — Command & handler structure
- [Catalog Module Reference](../../features/catalog/catalog-module.md) — Established test examples
- [Sales Module Reference](../../features/sales/) — Recent test implementations
- [xUnit Documentation](https://xunit.net/) — Test framework reference
- [Moq Documentation](https://github.com/moq/moq4/wiki/Quickstart) — Mocking library guide
- [FluentAssertions Documentation](https://fluentassertions.com/) — Assertion syntax reference
