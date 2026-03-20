# Development

Development guidelines, coding standards, and patterns.

## Contents

- **[Getting Started](getting-started.md)** — Complete setup and development guide:
  - Prerequisites and installation
  - Database configuration and migrations
  - Running the application locally
  - Development workflow (building, debugging, testing)
  - Common local development issues

- **[Local Setup & Development Guidelines](local-setup.md)** — Development best practices and patterns:
  - C# coding conventions and best practices
  - CQRS pattern with code examples
  - Repository and Specification pattern usage
  - DTO organization (request/response models)
  - FluentValidation pattern
  - Exception handling hierarchy
  - Controller pattern (thin controllers)
  - Async/await best practices

- **[Unit Testing Guide](testing/)** — Complete testing documentation and patterns:
  - [Quick Start](testing/README.md) — Test infrastructure, builders, mocks, quick examples
  - [Complete Testing Guide](testing/testing-guide.md) — Deep dive into all patterns, advanced scenarios, contributing
  - 36+ passing tests across Catalog, Sales, and Billing modules
  - Command handler patterns, service layer tests, mocking strategies

## Deployment

Ready to deploy? See [Deployment Guides](../deployment/):

- **[GitHub Actions CI/CD Setup](../deployment/github-actions-setup.md)** — Automated deployment to VPS
- **[Build and Deployment](../deployment/build-and-deploy.md)** — Build process and strategies
- **[Troubleshooting](../deployment/troubleshooting.md)** — Common issues and solutions

## Quick Links

- First time setup? → [Getting Started](getting-started.md)
- CQRS pattern? → [Development Guidelines - CQRS](local-setup.md#cqrs-pattern)
- Repository usage? → [Development Guidelines - Repository Pattern](local-setup.md#repository-pattern)
- DTOs? → [Development Guidelines - DTO Organization](local-setup.md#dto-organization)
- Validation? → [Development Guidelines - Validation](local-setup.md#validation)
- Exception handling? → [Development Guidelines - Exceptions](local-setup.md#exception-handling)
- **Writing tests?** → [Unit Testing Guide](testing/)
- **Test quick start?** → [Testing README](testing/README.md)
- **Advanced patterns?** → [Complete Testing Guide](testing/testing-guide.md)

## Development Workflow

### 1. Set Up Environment

```bash
# Clone repository
git clone https://github.com/mduy26100/qr-dine-app.git

# Install dependencies
dotnet restore

# Set up database
dotnet ef database update --project src/QRDine.Infrastructure --startup-project src/QRDine.API

# Run application
dotnet run --project src/QRDine.API
```

See [Getting Started](getting-started.md) for detailed setup steps.

### 2. Understand the Architecture

- Read [Architecture Overview](../architecture/)
- Review [Project Structure](../architecture/project-structure.md)

### 3. Learn the Patterns

All code follows established patterns:

- Use CQRS (Commands for write, Queries for read)
- Use Repository pattern for data access
- Use Specifications for query logic
- Use DTOs for request/response
- Use FluentValidation for input validation

See [Development Guidelines](local-setup.md) for code examples.

### 4. Build Features

1. Create domain entity
2. Create DTOs (request/response)
3. Create command/query and handler
4. Add FluentValidation validator
5. Add AutoMapper profile
6. Create controller endpoint
7. Create unit tests

See [Features Overview](../features/) for module examples.

## Code Organization

```
Features/
├── Catalog/
│   ├── Commands/
│   │   ├── CreateCategory.cs
│   │   ├── CreateCategoryHandler.cs
│   │   └── CreateCategoryValidator.cs
│   ├── Queries/
│   │   ├── GetCategories.cs
│   │   ├── GetCategoriesHandler.cs
│   │   └── GetCategoryById.cs
│   ├── DTOs/
│   │   ├── CreateCategoryRequest.cs
│   │   └── CategoryDto.cs
│   ├── Specifications/
│   │   └── CategoriesByMerchantSpec.cs
│   └── Mappings/
│       └── CategoryMappingProfile.cs
```

## Coding Standards

- **Naming:** PascalCase for classes/methods, camelCase for properties/fields
- **Async:** Always use async/await, never use `.Result` or `.Wait()`
- **Null coalescing:** Use `??` and `??=`
- **LINQ:** Use method syntax, avoid query syntax
- **Error handling:** Throw appropriate custom exceptions
- **Logging:** Use dependency injected ILogger
- **Comments:** Document "why", not "what"

See [Development Guidelines](local-setup.md) for complete coding standards and examples.

---

**Reference:** See also [Architecture Overview](../architecture/) for design patterns and [Features Overview](../features/) for implementation examples.
