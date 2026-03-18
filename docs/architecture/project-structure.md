# Project Structure

Complete guide to the directory organization and file structure of QRDine.

## Top-Level Organization

```
qr-dine-app/
├── src/                        # Source code
├── docs/                       # Documentation (this directory)
├── LICENSE                     # MIT License
├── README.md                   # Project overview
├── QRDine.sln                  # Visual Studio solution
└── .gitignore                  # Git ignore rules
```

## Source Code (`src/`)

Five main projects implementing Onion Architecture layers:

```
src/
├── QRDine.API/                 # Presentation Layer (ASP.NET Core)
├── QRDine.Application/         # Application Layer (CQRS & Business Logic)
├── QRDine.Application.Common/  # Shared Abstractions & Common Types
├── QRDine.Domain/              # Domain Layer (Entities, Rules)
└── QRDine.Infrastructure/      # Infrastructure Layer (EF Core, Identity, External Services)
```

## QRDine.API (Presentation Layer)

HTTP API entry point. Thin controllers, middleware, dependency injection orchestration.

```
QRDine.API/
├── Program.cs                          # Application startup, service registration
├── appsettings.json                    # Base configuration
├── appsettings.Development.json        # Development environment overrides
├── QRDine.API.csproj                   # Project file with dependencies
├── QRDine.API.http                     # Swagger/HTTP test requests
├── GlobalUsings.cs                     # Global using statements for all files

├── Controllers/                        # HTTP endpoint handlers
│   ├── Admin/
│   │   ├── MerchantsController.cs      # Merchant management (SuperAdmin only)
│   │   └── PlansController.cs          # Billing plan management
│   ├── Identity/
│   │   ├── AuthController.cs           # Login, register, token refresh
│   │   └── UsersController.cs          # User registration, profile management
│   ├── Management/
│   │   ├── Catalog/
│   │   │   ├── CategoriesController.cs # Merchant's category CRUD
│   │   │   ├── ProductsController.cs   # Merchant's product CRUD
│   │   │   ├── TablesController.cs     # Merchant's table CRUD
│   │   │   └── ToppingGroupsController.cs
│   │   ├── Sales/
│   │   │   ├── OrdersController.cs     # Order management
│   │   │   └── OrderItemsController.cs # Order item management
│   │   ├── Dashboard/
│   │   │   └── DashboardController.cs  # Merchant analytics
│   │   └── Staffs/
│   │       └── StaffsController.cs     # Staff management
│   └── Storefront/
│       ├── Catalog/
│       │   ├── CategoriesController.cs # Public category listing
│       │   ├── ProductsController.cs   # Public product catalog
│       │   └── TablesController.cs     # Public table info, QR codes
│       └── Sales/
│           └── OrdersController.cs     # Customer ordering

├── Middlewares/                        # Request/response processing pipeline
│   ├── ExceptionHandlingMiddleware.cs  # Centralized error handling
│   ├── TenantResolutionMiddleware.cs   # Extract MerchantId from request
│   ├── StorefrontSubscriptionMiddleware.cs # Validate storefront access
│   └── SubscriptionEnforcementMiddleware.cs # Check feature subscriptions

├── Filters/                            # Action & result filters
│   ├── ApiResponseFilter.cs            # Wrap responses in envelope
│   └── FeatureLimitFilter.cs           # Enforce subscription limits

├── Attributes/                         # Custom action attributes
│   ├── CheckFeatureLimitAttribute.cs   # Check if feature is available
│   └── SkipSubscriptionCheckAttribute.cs # Bypass subscription checks

├── Constants/
│   ├── CookieNames.cs                  # Cookie key constants
│   ├── RateLimitPolicies.cs            # Rate limiting policy names
│   └── SwaggerGroups.cs                # Swagger API group identifiers

├── Responses/                          # Response model envelopes
│   ├── ApiResponse.cs                  # Success response wrapper
│   ├── ApiError.cs                     # Error details
│   └── Meta.cs                         # Response metadata

├── Services/                           # API-specific services
│   ├── AuthCookieService.cs            # Refresh token cookie management
│   └── IAuthCookieService.cs           # Interface for above

├── Requests/                           # API request models
│   └── Catalog/
│       └── (contain request DTOs from features)

├── DependencyInjection/                # Service registration modules
│   ├── ServiceCollectionExtensions.cs  # Main registration orchestrator
│   ├── Application/
│   │   ├── MediatRRegistration.cs      # MediatR pipeline setup
│   │   └── AutoMapperRegistration.cs   # AutoMapper configuration
│   ├── ApplicationBuilderExtensions.cs # Middleware registration
│   ├── CrossCutting/
│   │   ├── ApiVersioningRegistration.cs
│   │   └── CorsRegistration.cs
│   ├── Features/
│   │   ├── CatalogsFeatureRegistration.cs
│   │   ├── SalesFeatureRegistration.cs
│   │   └── (other feature registrations)
│   ├── Infrastructure/
│   │   ├── PersistenceRegistration.cs
│   │   ├── ExternalServicesRegistration.cs
│   │   └── (other infrastructure registrations)
│   ├── Presentation/
│   │   └── PresentationRegistration.cs
│   └── Security/
│       ├── IdentityRegistration.cs
│       └── JwtRegistration.cs

└── obj/ & bin/                         # Build output directories

```

## QRDine.Application (Application Layer)

CQRS handlers, DTOs, validators, specifications. Organized by domain feature.

```
QRDine.Application/
├── GlobalUsings.cs

├── Features/                           # Domain modules (CQRS organized)
│   ├── Catalog/
│   │   ├── Categories/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateCategory/
│   │   │   │   │   ├── CreateCategoryCommand.cs
│   │   │   │   │   ├── CreateCategoryCommandHandler.cs
│   │   │   │   │   ├── CreateCategoryCommandValidator.cs
│   │   │   │   │   └── CreateCategoryCommandResponse.cs
│   │   │   │   ├── UpdateCategory/
│   │   │   │   └── DeleteCategory/
│   │   │   ├── Queries/
│   │   │   │   ├── GetCategoriesByMerchant/
│   │   │   │   ├── GetCategoryById/
│   │   │   │   └── GetCategoryHierarchy/
│   │   │   ├── DTOs/
│   │   │   │   ├── CategoryDto.cs
│   │   │   │   ├── CreateCategoryRequest.cs
│   │   │   │   └── UpdateCategoryRequest.cs
│   │   │   ├── Specifications/
│   │   │   │   ├── CategoryWithProductsSpec.cs
│   │   │   │   └── ActiveCategoriesSpec.cs
│   │   │   └── Extensions/
│   │   │       └── CatalogCategoryMapperProfile.cs
│   │   ├── Products/
│   │   │   ├── Commands/ (Create, Update, Delete, UploadImage, etc.)
│   │   │   ├── Queries/ (Get, Search, GetByCategory, etc.)
│   │   │   ├── DTOs/
│   │   │   ├── Specifications/
│   │   │   └── Extensions/
│   │   ├── Tables/
│   │   │   ├── Commands/ (Create, Update, Delete, GenerateQR)
│   │   │   ├── Queries/
│   │   │   ├── DTOs/
│   │   │   └── Extensions/
│   │   ├── ToppingGroups/
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── DTOs/
│   │   ├── Mappings/
│   │   │   └── AutoMapper configurations for all catalog entities
│   │   └── Repositories/
│   │       ├── ICategoryRepository.cs
│   │       ├── IProductRepository.cs
│   │       └── (interfaces for feature-specific queries)
│   ├── Sales/
│   │   ├── Orders/
│   │   │   ├── Commands/ (Create, Update, ChangeStatus, Cancel)
│   │   │   ├── Queries/ (Get, List, GetByTable)
│   │   │   ├── DTOs/
│   │   │   └── Extensions/
│   │   ├── OrderItems/
│   │   │   ├── Commands/ (Add, Remove, UpdateStatus)
│   │   │   ├── Queries/
│   │   │   └── DTOs/
│   │   ├── Mappings/
│   │   └── Repositories/
│   ├── Identity/
│   │   ├── Commands/
│   │   │   ├── Login/ (LoginCommand, handler, validator)
│   │   │   ├── RegisterMerchant/
│   │   │   ├── RegisterStaff/
│   │   │   ├── ConfirmRegister/
│   │   │   └── RefreshToken/
│   │   ├── Queries/
│   │   │   └── (identity-related queries if needed)
│   │   ├── DTOs/
│   │   │   ├── LoginRequestDto.cs
│   │   │   ├── LoginResponseDto.cs
│   │   │   ├── RegisterMerchantDto.cs
│   │   │   └── TokenClaimsDto.cs
│   │   └── Services/
│   │       ├── ILoginService.cs
│   │       ├── IRegisterService.cs
│   │       └── (implementations in Infrastructure)
│   ├── Tenant/
│   │   ├── Commands/ (Create merchant, etc.)
│   │   ├── Queries/
│   │   ├── DTOs/
│   │   └── Extensions/
│   ├── Billing/
│   │   ├── Commands/ (ManageSubscription, etc.)
│   │   ├── Queries/ (GetPlan, ListPlans, GetMerchantSubscriptions)
│   │   ├── DTOs/
│   │   └── Extensions/
│   ├── Dashboards/
│   │   ├── Queries/ (GetMerchantAnalytics, GetSalesReport)
│   │   └── DTOs/
│   └── Staffs/
│       ├── Commands/ (CreateStaff, UpdateStaff, etc.)
│       ├── Queries/
│       ├── DTOs/
│       └── Extensions/

└── obj/ & bin/

```

## QRDine.Application.Common (Shared Abstractions)

Interfaces and base classes shared across layers.

```
QRDine.Application.Common/
├── GlobalUsings.cs

├── Abstractions/                       # Interface definitions
│   ├── Identity/
│   │   ├── ICurrentUserService.cs      # Current user context (ID, roles, merchant)
│   │   └── IIdentityService.cs         # User/role operations
│   ├── Persistence/
│   │   ├── IApplicationDbContext.cs    # DbContext abstraction
│   │   ├── IRepository.cs              # Generic repository
│   │   └── IDatabaseTransaction.cs     # Transaction abstraction
│   ├── ExternalServices/
│   │   ├── IFileUploadService.cs       # File upload (Cloudinary)
│   │   └── IQrCodeService.cs           # QR code generation
│   ├── Notifications/
│   │   └── IEmailService.cs            # Email sending
│   └── Caching/
│       └── ICacheService.cs            # Distributed caching

├── Exceptions/                         # Custom exception types
│   ├── ValidationException.cs          # Input validation errors (400)
│   ├── BusinessRuleException.cs        # Business logic errors (400)
│   ├── NotFoundException.cs            # Resource not found (404)
│   ├── ConflictException.cs            # Conflict/duplicate (409)
│   ├── ForbiddenException.cs           # Access denied (403)
│   ├── ConcurrencyException.cs         # Concurrency error (409)
│   └── ApplicationExceptionBase.cs     # Base for all custom exceptions

├── Behaviors/                          # MediatR pipeline behaviors
│   ├── ValidationBehavior.cs           # Auto-run FluentValidation
│   ├── TransactionBehavior.cs          # Wrap commands in DB transactions
│   ├── LoggingBehavior.cs              # Log request/response
│   └── CachingBehavior.cs              # Cache query results

├── Constants/
│   ├── AppClaimTypes.cs                # JWT claim type constants
│   └── (other shared constants)

├── Models/
│   ├── FileUploadRequest.cs            # File upload model
│   └── (other shared models)

├── Templates/
│   └── (email templates, if any)

└── obj/ & bin/

```

## QRDine.Domain (Domain Layer)

Business entities with zero external dependencies.

```
QRDine.Domain/
├── QRDine.Domain.csproj                # No NuGet dependencies

├── Common/
│   ├── BaseEntity.cs                   # Base class with Id, CreatedAt, UpdatedAt, IsDeleted
│   ├── BaseEntity<TId>.cs              # Generic base entity
│   └── IMustHaveMerchant.cs            # Marker interface for tenant isolation

├── Catalog/                            # Menu structure entities
│   ├── Category.cs                     # Category with hierarchy support
│   ├── Product.cs                      # Product with pricing, images
│   ├── Table.cs                        # Restaurant table with QR code
│   ├── Topping.cs                      # Customization option
│   ├── ToppingGroup.cs                 # Grouping of toppings
│   └── ProductToppingGroup.cs          # Linking products to topping groups

├── Sales/                              # Order management entities
│   ├── Order.cs                        # Order header
│   ├── OrderItem.cs                    # Individual item in order
│   └── OrderCodeGenerator.cs           # Order code generation logic

├── Billing/                            # Subscription & feature limiting
│   ├── Plan.cs                         # Pricing plan
│   ├── Subscription.cs                 # Merchant subscription
│   ├── FeatureLimit.cs                 # Feature availability per plan
│   └── Transaction.cs                  # Payment transaction

├── Tenant/                             # Multi-tenancy
│   └── Merchant.cs                     # Tenant entity

├── Enums/
│   ├── OrderStatus.cs                  # Open, Paid, Cancelled
│   ├── OrderItemStatus.cs              # Pending, Processing, Completed, Cancelled
│   ├── FeatureType.cs                  # Features: Categories, Products, Tables, Orders
│   ├── SubscriptionStatus.cs           # Active, Expired, Cancelled
│   ├── PaymentStatus.cs                # Pending, Completed, Failed
│   └── PaymentMethod.cs                # COD, Card, Bank Transfer

├── Constants/
│   └── (domain-level constants)

└── obj/ & bin/

```

## QRDine.Infrastructure (Infrastructure Layer)

External service implementations - EF Core, Identity, Cloudinary, Email, etc.

```
QRDine.Infrastructure/
├── GlobalUsings.cs

├── Persistence/                        # Database & EF Core
│   ├── ApplicationDbContext.cs         # DbContext with all DbSets
│   ├── Repository.cs                   # Generic repository implementation
│   ├── DatabaseTransaction.cs          # Transaction wrapper
│   ├── Constants/
│   │   └── SchemaNames.cs              # DB schema constants
│   ├── Configurations/                 # EF Core Fluent API configurations
│   │   ├── Identity/
│   │   │   ├── RefreshTokenConfiguration.cs
│   │   │   ├── PermissionConfiguration.cs
│   │   │   └── RolePermissionConfiguration.cs
│   │   ├── Catalog/
│   │   │   ├── CategoryConfiguration.cs
│   │   │   ├── ProductConfiguration.cs
│   │   │   ├── TableConfiguration.cs
│   │   │   ├── ToppingGroupConfiguration.cs
│   │   │   ├── ToppingConfiguration.cs
│   │   │   └── ProductToppingGroupConfiguration.cs
│   │   ├── Sales/
│   │   │   ├── OrderConfiguration.cs
│   │   │   └── OrderItemConfiguration.cs
│   │   ├── Billing/
│   │   │   ├── FeatureLimitConfiguration.cs
│   │   │   ├── PlanConfiguration.cs
│   │   │   ├── SubscriptionConfiguration.cs
│   │   │   └── TransactionConfiguration.cs
│   │   └── Tenant/
│   │       └── MerchantConfiguration.cs
│   ├── Migrations/                     # EF Core code-first migrations
│   │   ├── 20260222115111_InitialCreate.cs
│   │   ├── 20260222122524_InitTenantAndCatalogSchema.cs
│   │   ├── 20260222131228_AddSalesSchema.cs
│   │   ├── 20260223163951_AddCategoryHierarchy.cs
│   │   ├── 20260224062428_AddToppingEntities.cs
│   │   ├── 20260305140701_InitSalesModule.cs
│   │   ├── 20260309181801_AddBillingSchema.cs
│   │   └── (additional migrations)
│   └── Seeding/                        # Database seed data
│       ├── IdentitySeeder.cs           # Seed roles and admin user
│       └── PlanSeeder.cs               # Seed billing plans

├── Identity/                           # ASP.NET Core Identity & JWT
│   ├── Models/
│   │   ├── ApplicationUser.cs          # Custom user entity
│   │   ├── ApplicationRole.cs          # Custom role entity
│   │   ├── RefreshToken.cs             # Refresh token storage
│   │   └── Permission.cs               # Fine-grained permissions
│   ├── Services/
│   │   ├── LoginService.cs             # Login handler
│   │   ├── RegisterService.cs          # Registration handler
│   │   ├── JwtTokenGenerator.cs        # JWT generation
│   │   ├── CurrentUserService.cs       # Extract current user from token
│   │   └── IdentityService.cs          # User/role operations
│   └── Constants/
│       └── IdentityConstants.cs        # Claim types, policy names

├── ExternalServices/                   # Third-party integrations
│   ├── Cloudinary/
│   │   └── CloudinaryFileUploadService.cs # Image upload implementation
│   └── QrCode/
│       └── QrCodeService.cs            # QR code generation

├── Email/                              # Email sending
│   ├── EmailService.cs                 # SMTP email implementation
│   └── IEmailService.cs                # Interface from Common

├── Caching/                            # Redis caching
│   ├── CacheService.cs                 # Redis implementation
│   └── ICacheService.cs                # Interface from Common

├── Cryptography/                       # Encryption utilities
│   └── CryptographyService.cs          # Token/password encryption

├── Configuration/                      # Configuration models
│   ├── CloudinarySettings.cs
│   ├── JwtSettings.cs
│   ├── EmailSettings.cs
│   └── (other settings)

├── SignalR/                            # Real-time communication
│   └── Hubs/
│       └── OrderHub.cs                 # Real-time order updates

├── Catalog/, Sales/, Tenant/, Staffs/  # Feature-specific repository implementations
│   └── (repository implementations extending Repository<T>)

└── obj/ & bin/

```

## Documentation (`docs/`)

```
docs/
├── README.md                           # Documentation index
├── development/
│   ├── README.md                       # Development documentation overview
│   └── getting-started.md              # Setup and installation guide

├── architecture/
│   ├── README.md                       # Architecture documentation overview
│   ├── overview.md                     # Onion Architecture & CQRS design
│   ├── project-structure.md            # This file
│   └── patterns-and-design.md          # Design patterns & implementation details

├── features/
│   ├── README.md                       # Features overview
│   ├── catalog/
│   │   ├── README.md
│   │   └── catalog-module.md
│   ├── identity/
│   │   ├── README.md
│   │   └── identity-module.md
│   ├── sales/
│   │   ├── README.md
│   │   └── sales-module.md
│   ├── billing/
│   │   ├── README.md
│   │   └── billing-module.md
│   ├── tenant/
│   │   ├── README.md
│   │   └── tenant-module.md
│   └── staffs/
│       ├── README.md
│       └── staffs-module.md

└── deployment/
    ├── README.md                       # Deployment documentation overview
    └── troubleshooting.md              # Common issues & solutions

```

## Key File Patterns

### Controllers

- **Route:** `[Route("api/v{version:apiVersion}/management/...")]`
- **Naming:** `{Domain}{Resource}Controller.cs`
- **Pattern:** Thin controllers dispatching to MediatR

### CQRS Handlers

- **Command:** `{ActionName}Command.cs`, `{ActionName}CommandHandler.cs`
- **Query:** `{ActionName}Query.cs`, `{ActionName}QueryHandler.cs`
- **Validator:** `{ActionName}CommandValidator.cs` (implements `IValidator<T>`)
- **Response:** `{ActionName}{Command|Query}Response.cs`

### DTOs

- **Request:** `{Action}Request.cs` (for commands)
- **Response:** `{Entity}Dto.cs` (for queries)

### Entity Configurations

- **Pattern:** `{Entity}Configuration.cs` implementing `IEntityTypeConfiguration<T>`
- **Location:** `Persistence/Configurations/{Module}/`

### Specifications

- **Pattern:** `{Entity}{Description}Specification.cs` or `{Entity}Specification.cs`
- **Base:** Inherits from `BaseSpecification<T>`

## Build Artifacts

- **Debug build:** `/bin/Debug/net8.0/`
- **Release build:** `/bin/Release/net8.0/`
- **Object files:** `/obj/`
- **NuGet cache:** `~/.nuget/packages/` (local machine)

## Configuration Hierarchy

1. `appsettings.json` (base)
2. `appsettings.Development.json` (development overrides)
3. `appsettings.Production.json` (production overrides, if exists)
4. Environment variables (override all files)
5. User Secrets (development only)
