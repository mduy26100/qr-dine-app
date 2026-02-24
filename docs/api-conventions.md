# API Conventions

This document describes the HTTP API conventions, response envelope, error handling, versioning, authentication, and Swagger configuration used in QRDine.

---

## API Versioning

All endpoints are versioned using **URL segment versioning**:

```
/api/v{version:apiVersion}/...
```

Currently, all controllers target **v1.0** via the `[ApiVersion("1.0")]` attribute.

Configured in `src/QRDine.API/DependencyInjection/CrossCutting/ApiVersioningRegistration.cs`.

---

## API Groups

The API is split into two logical groups, each with its own Swagger document:

| Group | Route Prefix | Access | Purpose |
|-------|-------------|--------|---------|
| `management` | `/api/v1/management/...` | Authenticated (Merchant role) | Store management CRUD operations |
| `storefront` | `/api/v1/storefront/...` | Public | Customer-facing endpoints |

Additionally, identity-related endpoints (`/api/v1/auth/...`, `/api/v1/users/...`) are grouped under `management`.

Swagger groups are defined as constants in `src/QRDine.API/Constants/SwaggerGroups.cs` and configured in `src/QRDine.API/Program.cs`.

---

## Unified Response Envelope

All successful API responses are automatically wrapped in a standardized `ApiResponse` envelope by the `ApiResponseFilter` action filter.

### Success Response

```json
{
  "data": { /* response payload */ },
  "meta": {
    "timestamp": "2026-02-24T12:00:00Z",
    "path": "/api/v1/management/categories",
    "method": "POST",
    "statusCode": 201,
    "traceId": "00-abc123...",
    "requestId": null,
    "clientIp": "::1"
  },
  "error": null
}
```

### Error Response

```json
{
  "data": null,
  "meta": {
    "timestamp": "2026-02-24T12:00:00Z",
    "path": "/api/v1/management/categories",
    "method": "POST",
    "statusCode": 400,
    "traceId": "00-abc123...",
    "requestId": null,
    "clientIp": "::1"
  },
  "error": {
    "type": "business-rule-violation",
    "message": "Cannot assign a parent that is already a child. Only 1 level of hierarchy allowed.",
    "details": null
  }
}
```

### Validation Error Response

```json
{
  "data": null,
  "meta": { /* ... */ },
  "error": {
    "type": "validation-error",
    "message": "Validation failed",
    "details": {
      "Dto.Name": ["Category name is required."],
      "Dto.DisplayOrder": ["Display order must be greater than or equal to 0."]
    }
  }
}
```

### Response Models

Defined in `src/QRDine.API/Responses/`:

| Class | Properties |
|-------|-----------|
| `ApiResponse` | `Data`, `Meta`, `Error` |
| `Meta` | `Timestamp`, `Path`, `Method`, `StatusCode`, `TraceId`, `RequestId`, `ClientIp` |
| `ApiError` | `Type`, `Message`, `Details` |

---

## Automatic Response Wrapping

The `ApiResponseFilter` (`src/QRDine.API/Filters/ApiResponseFilter.cs`) intercepts all `ObjectResult` responses and wraps them in `ApiResponse.Success()`. The following responses are **excluded** from wrapping:

- `204 No Content` responses
- `FileResult` responses
- Responses already of type `ApiResponse` or `ProblemDetails`

---

## Global Exception Handling

The `ExceptionHandlingMiddleware` (`src/QRDine.API/Middlewares/ExceptionHandlingMiddleware.cs`) catches all unhandled exceptions and maps them to structured error responses:

| Exception Type | HTTP Status | Error Type |
|---------------|------------|------------|
| `ValidationException` | 400 Bad Request | `validation-error` |
| `BusinessRuleException` | 400 Bad Request | `business-rule-violation` |
| `NotFoundException` | 404 Not Found | `not-found` |
| `ConflictException` | 409 Conflict | `conflict` |
| `ConcurrencyException` | 409 Conflict | `concurrency-error` |
| `ForbiddenException` | 403 Forbidden | `forbidden` |
| `UnauthorizedAccessException` | 401 Unauthorized | `unauthorized` |
| `ApplicationExceptionBase` | 400 Bad Request | `application-error` |
| Any other exception | 500 Internal Server Error | `internal-server-error` |

The middleware also handles non-exception HTTP status codes for responses that haven't started:

| HTTP Status | Error Type | Trigger |
|------------|------------|---------|
| 401 | `unauthorized` | Missing or invalid JWT |
| 403 | `forbidden` | Insufficient role |
| 404 | `not-found` | No matching endpoint |
| 405 | `method-not-allowed` | Wrong HTTP method |

---

## Authentication & Authorization

### JWT Bearer Authentication

- Configured in `src/QRDine.API/DependencyInjection/Security/JwtRegistration.cs`
- Algorithm: **HMAC-SHA256**
- Validates: Issuer, Audience, Lifetime, SigningKey
- Clock skew: **Zero** (strict expiration)

### JWT Token Claims

The access token includes the following claims:

| Claim | Source |
|-------|--------|
| `NameIdentifier` | User ID |
| `Email` | User email |
| `Name` | Full name (FirstName + LastName) |
| `Role` | Assigned roles (one claim per role) |
| `merchant_id` | Custom claim for tenant context (if applicable) |

### Token Configuration

Configured via `Jwt` section in `appsettings.json`:

| Setting | Description |
|---------|-------------|
| `Secret` | Symmetric key for HMAC-SHA256 signing |
| `ValidIssuer` | Token issuer URL |
| `ValidAudience` | Token audience URL |
| `AccessTokenExpiryMinutes` | Access token TTL (default: 15 min) |
| `RefreshTokenExpiryDays` | Refresh token TTL (default: 7 days) |

### Role-Based Access Control

Four system roles are defined in `src/QRDine.Infrastructure/Identity/Constants/SystemRoles.cs`:

| Role | Usage |
|------|-------|
| `SuperAdmin` | Can register new merchants (`POST /api/v1/users/register-merchant`) |
| `Merchant` | Manages their store's catalog and staff (`/api/v1/management/...` endpoints) |
| `Staff` | Store staff (registered by Merchant) |
| `Guest` | Public guests accessing storefront |

### Identity Configuration

Configured in `src/QRDine.API/DependencyInjection/Security/IdentityRegistration.cs`:

- **Password Policy**: Min 6 chars, requires uppercase, lowercase, digit, and special character
- **Lockout**: 5 failed attempts → 5-minute lockout
- **Email**: Must be unique across all users

---

## Request Formats

### JSON Body (`application/json`)

Most endpoints accept JSON request bodies:

```http
POST /api/v1/management/categories
Content-Type: application/json

{
  "dto": {
    "name": "Beverages",
    "description": "All drinks",
    "displayOrder": 1,
    "parentId": null
  }
}
```

### Multipart Form Data (`multipart/form-data`)

The `CreateProduct` endpoint accepts `multipart/form-data` to support image file uploads:

```http
POST /api/v1/management/products
Content-Type: multipart/form-data

name=Espresso
description=Strong coffee
price=3.50
isAvailable=true
categoryId=<guid>
imageFile=@espresso.jpg
```

The `CreateProductForm` class (`src/QRDine.API/Requests/Catalog/CreateProductForm.cs`) converts `IFormFile` to a DTO with raw `Stream`:

```csharp
public CreateProductDto ToDto()
{
    return new CreateProductDto
    {
        Name = Name,
        Description = Description,
        Price = Price,
        IsAvailable = IsAvailable,
        CategoryId = CategoryId,
        ImgContent = ImageFile?.OpenReadStream(),
        ImgFileName = ImageFile?.FileName,
        ImgContentType = ImageFile?.ContentType
    };
}
```

---

## Validation Pipeline

All MediatR requests pass through the `ValidationBehavior<TRequest, TResponse>` pipeline behavior before reaching their handler:

1. All registered `IValidator<TRequest>` validators are executed in parallel.
2. If any validation failures exist, they are grouped by property name and thrown as a `ValidationException`.
3. If validation passes, the request proceeds to the handler.

Validators use **FluentValidation** syntax. Example from `src/QRDine.Application/Features/Catalog/Categories/Commands/CreateCategory/CreateCategoryCommandValidator.cs`:

```csharp
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Category data must be provided.");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Dto.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
    }
}
```
