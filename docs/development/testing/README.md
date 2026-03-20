# Unit Testing Guide

🧪 **QRDine** uses **xUnit** with **Moq**, **FluentAssertions**, and **AutoFixture** for comprehensive unit testing of application logic.

Our test suite covers **CQRS command handlers** and **service layer** across all domain modules with **36+ passing tests** and growing.

---

## Current Status

| Module | Scope | Tests | Status |
|--------|-------|-------|--------|
| **Catalog** | Categories, Products, Tables, ToppingGroups | 12 | ✅ Complete |
| **Sales** | Orders, OrderItems | 6 | ✅ Complete |
| **Billing** | Plans, Subscriptions, FeatureLimits | 5 | ✅ Complete |

**Total: 36+ passing unit tests across command handlers and services**

---

## Quick Start - Writing Your First Test

### 1️⃣ Create Test Class

```csharp
// Location: tests/QRDine.Application.Tests/Features/{Module}/{Entity}/Commands/{Action}/
using Xunit;
using Moq;
using FluentAssertions;

namespace QRDine.Application.Tests.Features.Catalog.Categories.Commands.Create
{
    public class CreateCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepo;
        private readonly CreateCategoryCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public CreateCategoryCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _categoryRepo = new Mock<ICategoryRepository>();
            _handler = new CreateCategoryCommandHandler(_categoryRepo.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateSuccessfully()
        {
            // Arrange
            var command = new CreateCategoryCommand(_fixture.MerchantId, new CreateCategoryDto { Name = "Food" });
            _categoryRepo.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Category>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Food");
            _categoryRepo.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
```

### 2️⃣ Use Builders for Test Data

```csharp
// Builders are reusable test data factories
var category = new CategoryBuilder()
    .WithId(_fixture.CategoryId)
    .WithMerchantId(_fixture.MerchantId)
    .WithName("Electronics")
    .WithParentId(null)
    .Build();

var createDto = new CreateCategoryDtoBuilder()
    .WithName("New Category")
    .WithParentId(_fixture.CategoryId)
    .Build();
```

### 3️⃣ Mock Dependencies

```csharp
// Repository mocks
_categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, CancellationToken.None))
    .ReturnsAsync(category);

// Service mocks
var mockMapper = CatalogServiceMocks.CreateMapperMock();
mockMapper.Setup(x => x.Map<CategoryResponseDto>(It.IsAny<Category>()))
    .Returns(new CategoryResponseDto { Id = category.Id, Name = category.Name });
```

### 4️⃣ Follow AAA Pattern

**Arrange** → Setup test data and mocks  
**Act** → Execute the code being tested  
**Assert** → Verify results and side effects

```csharp
[Fact]
public async Task Handle_WithoutPermission_ShouldThrowForbiddenException()
{
    // Arrange
    var unauthorizedMerchantId = Guid.NewGuid();
    var command = new DeleteCategoryCommand(_fixture.CategoryId) { MerchantId = unauthorizedMerchantId };
    _categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new CategoryBuilder().WithMerchantId(_fixture.MerchantId).Build());

    // Act & Assert
    await Assert.ThrowsAsync<ForbiddenException>(
        () => _handler.Handle(command, CancellationToken.None)
    );
}
```

---

## Test Infrastructure

### 📦 Test Project Structure

```
tests/QRDine.Application.Tests/
├── Features/
│   ├── Catalog/
│   │   ├── Categories/Commands/     # Command handler tests
│   │   ├── Products/Commands/
│   │   └── ...
│   ├── Sales/
│   ├── Billing/
│   └── ...
├── Common/
│   ├── Builders/                    # Test data factories
│   │   ├── Catalog/
│   │   ├── Sales/
│   │   └── Billing/
│   ├── Mocks/                       # Repository/Service mocks
│   │   ├── CatalogRepositoryMocks.cs
│   │   ├── CatalogServiceMocks.cs
│   │   └── ...
│   └── Fixtures/                    # Shared test context with fixed GUIDs
│       ├── CatalogFixture.cs
│       ├── SalesFixture.cs
│       └── BillingFixture.cs
├── GlobalUsings.cs                  # Shared using statements
└── QRDine.Application.Tests.csproj
```

### 🏗️ Builders (Test Data Factories)

Builders use the **fluent builder pattern** for readable test data:

```csharp
// Entity Builder
var order = new OrderBuilder()
    .WithId(orderId)
    .WithMerchantId(merchantId)
    .WithStatus(OrderStatus.Open)
    .WithTotalAmount(150000m)
    .AddItem(new OrderItemBuilder().WithProductId(productId).Build())
    .Build();

// DTO Builder
var createOrderDto = new ManagementCreateOrderDtoBuilder()
    .WithTableId(tableId)
    .WithSessionId(sessionId)
    .AddItem(new ManagementCreateOrderItemDtoBuilder().WithProductId(productId).Build())
    .Build();
```

**Available Builders:**
- **Catalog**: CategoryBuilder, ProductBuilder, TableBuilder, ToppingGroupBuilder, ...
- **Sales**: OrderBuilder, OrderItemBuilder, OrderCreationDtoBuilder, ...
- **Billing**: PlanBuilder, SubscriptionBuilder, TransactionBuilder, ...

### 🎭 Mocks (Repository & Service Factories)

Mock factories provide pre-configured mocks with common setups:

```csharp
// Repository mocks with ReturnsAsync already configured
var categoryRepoMock = new Mock<ICategoryRepository>();
categoryRepoMock.Setup(x => x.GetByIdAsync(...)).ReturnsAsync(...);

// Service mocks
var mapperMock = CatalogServiceMocks.CreateMapperMock();
```

**Available Mocks:**
- `CatalogRepositoryMocks` — Category, Product, Table repositories
- `CatalogServiceMocks` — Mapper, notification services
- `SalesRepositoryMocks` — Order, OrderItem repositories
- `SalesServiceMocks` — Order creation, formatting services
- `BillingRepositoryMocks` — Plan, Subscription, FeatureLimit repositories
- `BillingServiceMocks` — Subscription, billing services
- `BillingExternalServicesMocks` — PayOS payment integration

### 🔧 Fixtures (Shared Test Context)

Fixtures provide **consistent GUIDs** across tests to ensure data relationships:

```csharp
var fixture = new CatalogFixture();

fixture.MerchantId      // Guid for all category/product tests
fixture.CategoryId      // Guid for category
fixture.ProductId       // Guid for product
fixture.TableId         // Guid for table
fixture.ToppingGroupId  // Guid for topping group
```

---

## Test Patterns by Scenario

### ✅ Happy Path Tests

Verify the command/handler works correctly with valid input:

```csharp
[Fact]
public async Task Handle_ValidRequest_ShouldCreateSuccessfully()
{
    var command = new CreateCategoryCommand(merchantId, validDto);
    var result = await _handler.Handle(command, CancellationToken.None);
    result.Should().NotBeNull();
}
```

### ❌ Exception Handling Tests

Verify exceptions are thrown for error conditions:

```csharp
[Fact]
public async Task Handle_CategoryNotFound_ShouldThrowNotFoundException()
{
    var command = new DeleteCategoryCommand(nonExistentId);
    _categoryRepo.Setup(x => x.GetByIdAsync(...)).ReturnsAsync((Category?)null);
    
    await Assert.ThrowsAsync<NotFoundException>(
        () => _handler.Handle(command, CancellationToken.None)
    );
}
```

### 🔐 Authorization Tests

Verify ownership and permission checks:

```csharp
[Fact]
public async Task Handle_DifferentMerchant_ShouldThrowForbiddenException()
{
    var otherMerchantId = Guid.NewGuid();
    var category = new CategoryBuilder().WithMerchantId(_fixture.MerchantId).Build();
    var command = new UpdateCategoryCommand(category.Id, updateDto) { MerchantId = otherMerchantId };
    
    _categoryRepo.Setup(x => x.GetByIdAsync(...)).ReturnsAsync(category);
    
    await Assert.ThrowsAsync<ForbiddenException>(
        () => _handler.Handle(command, CancellationToken.None)
    );
}
```

### 🔄 State Transitions & Complex Logic

Verify complex business logic:

```csharp
[Fact]
public async Task Handle_CancelOrder_ShouldMarkAllItemsSold()
{
    var order = new OrderBuilder().WithStatus(OrderStatus.Open).Build();
    var item1 = new OrderItemBuilder().WithStatus(OrderItemStatus.Pending).Build();
    var item2 = new OrderItemBuilder().WithStatus(OrderItemStatus.Preparing).Build();
    order.AddItem(item1).AddItem(item2);
    
    var command = new CloseOrderCommand(order.Id, OrderStatus.Cancelled);
    var result = await _handler.Handle(command, CancellationToken.None);
    
    item1.Status.Should().Be(OrderItemStatus.Cancelled);
    item2.Status.Should().Be(OrderItemStatus.Cancelled);
    order.TotalAmount.Should().Be(0m);
}
```

### 📢 Side Effects & Notifications

Verify service calls for side effects:

```csharp
[Fact]
public async Task Handle_WhenOrderCreated_ShouldNotifyStateChange()
{
    var command = new StorefrontCreateOrderCommand(...);
    var mockNotificationService = new Mock<IOrderNotificationService>();
    
    var result = await _handler.Handle(command, CancellationToken.None);
    
    mockNotificationService.Verify(
        x => x.NotifyOrderUpdatedAsync(
            _fixture.MerchantId,
            result.TableId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ),
        Times.Once
    );
}
```

---

## Running Tests

### Run All Tests
```bash
dotnet test tests/QRDine.Application.Tests/
```

### Run Tests by Module
```bash
# Catalog tests only
dotnet test tests/QRDine.Application.Tests/ --filter "Catalog"

# Sales tests only
dotnet test tests/QRDine.Application.Tests/ --filter "Sales"
```

### Run Specific Test Class
```bash
dotnet test tests/QRDine.Application.Tests/ --filter "CreateCategoryCommandHandlerTests"
```

### Run with Coverage
```bash
dotnet test tests/QRDine.Application.Tests/ --collect:"XPlat Code Coverage"
```

---

## Best Practices

✅ **DO:**
- Use builders for all test data setup
- Follow AAA pattern (Arrange, Act, Assert)
- Test one scenario per test method
- Name tests descriptively: `Handle_[Scenario]_Should[Result]`
- Mock external dependencies (repositories, services)
- Use `CancellationToken.None` for unit tests
- Verify side effects with `.Verify()` calls
- Use fixtures for consistent GUIDs across tests

❌ **DON'T:**
- Test internal implementation (test behavior, not implementation)
- Create circular dependencies between test classes
- Use `Thread.Sleep()` for async wait
- Ignore exception messages in assertion
- Mock the subject under test
- Create tests that depend on test execution order
- Use random data (use fixtures/builders instead)

---

## Next Steps

→ **[Read the Complete Testing Guide](testing-guide.md)** for detailed patterns, advanced scenarios, and contribution guidelines.

→ **[View Catalog Tests Example](../../../features/catalog/)** to see established patterns in action.

---

## Reference

- [Development Guidelines](../README.md) — Main development documentation
- [CQRS & Command Patterns](../README.md#cqrs-command-query-responsibility-segregation) — Command structure overview
- [Architecture Overview](../../architecture/) — System design and layers
