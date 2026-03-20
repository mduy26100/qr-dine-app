# Middleware and Filters Implementation Documentation

**QRDine Project** | Comprehensive Reference Guide

---

## Table of Contents

1. [Middleware Components](#middleware-components)
2. [Filters](#filters)
3. [Integration & Execution Flow](#integration--execution-flow)
4. [Error Handling](#error-handling)
5. [Response Envelope Structure](#response-envelope-structure)
6. [Complete Execution Sequence](#complete-execution-sequence)

---

## Middleware Components

### Overview

Middleware components are registered in [Program.cs](../src/QRDine.API/Program.cs) and execute in a specific order during request processing. All middlewares are located in `src/QRDine.API/Middlewares/`.

---

### 1. Exception Handling Middleware

**File:** `src/QRDine.API/Middlewares/ExceptionHandlingMiddleware.cs`

**Registration (Program.cs - Line 52):**

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

**Position:** Applied first in the middleware pipeline (line 52)

#### Purpose

- Wraps entire request processing in try-catch
- Handles all thrown exceptions and formats them consistently
- Handles implicit HTTP status codes (401, 403, 404, 405) even when no exception occurs
- Prevents unhandled exceptions from leaking to clients
- Logs errors for debugging

#### Exception Type Mapping

The middleware maps custom exceptions to HTTP status codes and error types:

| Exception Type                      | HTTP Status | Error Type                | Message                       | Details Field                  |
| ----------------------------------- | ----------- | ------------------------- | ----------------------------- | ------------------------------ |
| `BadRequestException`               | 400         | "bad-request"             | Custom message                | N/A                            |
| `ValidationException`               | 400         | "validation-error"        | Custom message                | ✓ Includes validation failures |
| `BusinessRuleException`             | 400         | "business-rule-violation" | Custom message                | N/A                            |
| `NotFoundException`                 | 404         | "not-found"               | Custom message                | N/A                            |
| `ConflictException`                 | 409         | "conflict"                | Custom message                | N/A                            |
| `ConcurrencyException`              | 409         | "concurrency-error"       | Custom message                | N/A                            |
| `ForbiddenException`                | 403         | "forbidden"               | Custom message                | N/A                            |
| `UnauthorizedAccessException`       | 401         | "unauthorized"            | Custom message                | N/A                            |
| `ApplicationExceptionBase`          | 400         | "application-error"       | Custom message                | N/A                            |
| **Unhandled Exception** (any other) | 500         | "internal-server-error"   | "An internal error occurred." | N/A                            |

#### Error Response Example

```json
{
  "data": null,
  "error": {
    "type": "not-found",
    "message": "Product with ID 12345 was not found.",
    "details": null
  },
  "meta": {
    "timestamp": "2026-03-21T10:30:45.123Z",
    "path": "/api/v1/management/products/12345",
    "method": "GET",
    "statusCode": 404,
    "traceId": "0HN1GKNF64QCE:00000001",
    "requestId": null,
    "clientIp": "192.168.1.100"
  }
}
```

#### Validation Error Response Example

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
  "meta": {
    "timestamp": "2026-03-21T10:30:45.123Z",
    "path": "/api/v1/management/products",
    "method": "POST",
    "statusCode": 400,
    "traceId": "0HN1GKNF64QCE:00000002",
    "requestId": null,
    "clientIp": "192.168.1.100"
  }
}
```

---

### 2. Tenant Resolution Middleware

**File:** `src/QRDine.API/Middlewares/TenantResolutionMiddleware.cs`

**Registration (Program.cs - Line 59):**

```csharp
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
```

**Position:** After Authentication middleware (line 59)

#### Purpose

- Resolves merchant/tenant ID from multiple sources
- Stores resolved merchant ID in `HttpContext.Items` for downstream consumption
- Enables multi-tenancy for public (storefront) endpoints without authentication
- Skips webhook requests

#### Merchant ID Resolution Order

1. **JWT Claim** (highest priority)
   - For authenticated Merchant and Staff users
   - Extracted from `merchant_id` claim in JWT token
   - Claim added during login by `LoginService`
   - Location: `context.User.FindFirst(AppClaimTypes.MerchantId)`

2. **Route Parameter**
   - For public endpoints: `/api/v1/storefront/merchants/{merchantId}/categories`
   - Parameter name: `merchantId`
   - Used for storefront (unauthenticated) access

3. **Query Parameter** (lowest priority)
   - For public endpoints: `GET /api/v1/storefront/categories?merchantId=550e8400-e29b-41d4-a716-446655440000`
   - Parameter name: `merchantId`

#### Storage

- Resolved merchant ID stored in `context.Items["ResolvedMerchantId"]`

---

### 3. Storefront Subscription Middleware

**File:** `src/QRDine.API/Middlewares/StorefrontSubscriptionMiddleware.cs`

**Registration (Program.cs - Line 60):**

```csharp
app.UseMiddleware<StorefrontSubscriptionMiddleware>();
```

**Position:** After TenantResolutionMiddleware (line 60)

#### Purpose

- Enforces active subscription requirement for storefront APIs
- Validates that merchant has active subscription before allowing customer access
- Prevents access to store menu/ordering when store doesn't exist or subscription expired
- Applies only to storefront endpoints (`/api/v1/storefront/...`)
- Skips webhooks

#### Subscription Check Flow

```
Request to /api/v1/storefront/...
         ↓
TenantResolutionMiddleware resolves merchantId
         ↓
StorefrontSubscriptionMiddleware
         ↓
Call ISubscriptionService.IsSubscriptionActiveAsync(merchantId)
         ↓
Check cache key: "MerchantActiveStatus_{merchantId}"
         ↓
If cached:
  - Use cached value (with TTL)
Else:
  - Query GetLatestSubscriptionInfoAsync(merchantId)
  - Check if Status == Active AND EndDate >= Now
  - Cache result
         ↓
If subscription INACTIVE:
  - Write 403 Forbidden response
  - Return early (don't call _next)
Else:
  - Continue to next middleware
```

#### Response When Subscription Inactive

```json
{
  "data": null,
  "error": {
    "type": "store-closed",
    "message": "Cửa hàng hiện đang tạm ngưng phục vụ. Vui lòng quay lại sau!"
  },
  "meta": {
    "timestamp": "2026-03-21T10:30:45.123Z",
    "path": "/api/v1/storefront/merchants/abc-123/categories",
    "method": "GET",
    "statusCode": 403,
    "traceId": "0HN1GKNF64QCE:00000003",
    "clientIp": "192.168.1.100"
  }
}
```

---

### 4. Subscription Enforcement Middleware

**File:** `src/QRDine.API/Middlewares/SubscriptionEnforcementMiddleware.cs`

**Registration (Program.cs - Line 68):**

```csharp
app.UseMiddleware<SubscriptionEnforcementMiddleware>();
```

**Position:** Last in middleware chain (line 68), after MapHub and all routing

#### Purpose

- Enforces active subscription for merchant and staff users
- Prevents Merchant and Staff users from accessing system when subscription expired
- Returns 402 Payment Required when subscription is expired or missing
- Skips anonymous endpoints and specific endpoints decorated with `[SkipSubscriptionCheck]`
- Validates subscription status claim in JWT token
- Logs access attempts with expired subscriptions

#### Subscription Status Check Flow

```
Authenticated Request
         ↓
Get Endpoint metadata
         ↓
Is endpoint [AllowAnonymous]? → YES → Skip to next middleware
         ↓ NO
Is endpoint [SkipSubscriptionCheck]? → YES → Skip to next middleware
         ↓ NO
Is user authenticated? → NO → Skip to next middleware
         ↓ YES
Is user Merchant or Staff role? → NO → Skip to next middleware
         ↓ YES
Extract SubscriptionStatus claim from JWT
         ↓
Is claim empty OR equals "Expired"? → YES → Return 402 Payment Required
         ↓ NO
Continue to endpoint
```

#### JWT Claims Checked

The middleware extracts these claims from the JWT token:

- **AppClaimTypes.SubscriptionStatus:** "subscription_status"
  - Values: "Active", "Trialing", "Expired", "Cancelled"
  - Set during login by `LoginService`

#### Response When Subscription Expired

```json
{
  "data": null,
  "error": {
    "type": "payment-required",
    "message": "Gói cước của cửa hàng đã hết hạn hoặc chưa được đăng ký. Vui lòng gia hạn để tiếp tục sử dụng hệ thống."
  },
  "meta": {
    "timestamp": "2026-03-21T10:30:45.123Z",
    "path": "/api/v1/management/products",
    "method": "GET",
    "statusCode": 402,
    "traceId": "0HN1GKNF64QCE:00000004",
    "requestId": null,
    "clientIp": "192.168.1.100"
  }
}
```

#### How to Skip Subscription Check

Use the `[SkipSubscriptionCheckAttribute]` decorator:

```csharp
[ApiController]
[Route("api/v1/management")]
public class ProductsController : ControllerBase
{
    [HttpGet("subscription-status")]
    [SkipSubscriptionCheck]  // ← Skips the middleware check
    public async Task<IActionResult> GetSubscriptionStatus()
    {
        // This endpoint is called even if subscription expired
        // Useful for allowing users to check their subscription status and renew
    }
}
```

---

## Filters

### Overview

Filters are registered globally at the controller level in [Program.cs](../src/QRDine.API/Program.cs) and execute during the action result phase. Located in `src/QRDine.API/Filters/`.

---

### 1. API Response Filter

**File:** `src/QRDine.API/Filters/ApiResponseFilter.cs`

**Registration (Program.cs - Line 16):**

```csharp
builder.Services
    .AddControllers(options => options.Filters.Add<ApiResponseFilter>())
```

**Type:** `IAsyncResultFilter`

**Scope:** Global (applied to all controllers)

#### Purpose

- Wraps successful action results in standardized `ApiResponse` envelope
- Adds metadata to every response (timestamp, trace ID, IP, etc.)
- Skips 204 No Content responses (no body)
- Skips FileResult responses (downloads)
- Skips responses already wrapped in `ApiResponse`
- Converts camelCase property names in JSON

#### Response Wrapping Example

**Original Response (from controller):**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Pho Bo",
  "price": 50000
}
```

**Wrapped Response (after filter):**

```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Pho Bo",
    "price": 50000
  },
  "error": null,
  "meta": {
    "timestamp": "2026-03-21T10:30:45.123Z",
    "path": "/api/v1/management/products/550e8400-e29b-41d4-a716-446655440000",
    "method": "GET",
    "statusCode": 200,
    "traceId": "0HN1GKNF64QCE:00000005",
    "requestId": null,
    "clientIp": "192.168.1.100"
  }
}
```

#### Responses Skipped

1. 204 No Content - No response body needed
2. File downloads - Send files as-is without JSON wrapping
3. Already wrapped ApiResponse - Prevent double-wrapping
4. ProblemDetails (RFC 7807) - Standard error format

---

### 2. Feature Limit Filter

**File:** `src/QRDine.API/Filters/FeatureLimitFilter.cs`

**Type:** `IAsyncActionFilter`

**Scope:** Per-endpoint (via `[CheckFeatureLimit]` attribute)

#### Purpose

- Validates that merchant has not exceeded feature limits for specific actions
- Prevents operations when plan limit reached (e.g., max 100 products in Basic plan)
- Checks merchant's current plan and feature usage
- Returns 402 Payment Required when limit exceeded
- Allows selective protection of features requiring higher plans

#### Feature Types

```csharp
public enum FeatureType
{
    Products = 1,
    Orders = 2,
    Tables = 3,
    StaffMembers = 4,
    Categories = 5,
    Customizations = 6
}
```

#### Usage Example

Protect an endpoint that creates products:

```csharp
[ApiController]
[Route("api/v1/management")]
[Authorize(Roles = SystemRoles.Merchant)]
public class ProductsController : ControllerBase
{
    [HttpPost("products")]
    [CheckFeatureLimit(FeatureType.Products)]  // ← Protect with feature limit check
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        // This endpoint checks if merchant has limit for Products feature
        // If exceeded, returns 402 Payment Required before entering this method
    }
}
```

#### Check Limit Flow

```
Request to protected endpoint
         ↓
FeatureLimitFilter.OnActionExecutionAsync()
         ↓
Is user authenticated? → NO → Skip check, continue
         ↓ YES
Extract MerchantId and PlanCode claims from JWT
         ↓
Are both present? → NO → Skip check, continue
         ↓ YES
Call IFeatureLimitService.CheckLimitAsync(merchantId, planCode, featureType)
         ↓
Service checks plan limits from database/cache
         ↓
Is current usage >= limit? → YES → Throw exception
         ↓ NO
Return normally
         ↓
Catch exception block:
  - Write 402 Payment Required response
  - Include error message from service
  - Return early (don't continue to action)
         ↓
If no exception:
  - Continue to action method
```

#### Response When Limit Exceeded

```json
{
  "data": null,
  "error": {
    "type": "feature-limit-exceeded",
    "message": "Your Basic plan allows maximum 100 products. You have already created 100 products."
  },
  "meta": {
    "timestamp": "2026-03-21T10:30:45.123Z",
    "path": "/api/v1/management/products",
    "method": "POST",
    "statusCode": 402,
    "traceId": "0HN1GKNF64QCE:00000006",
    "requestId": null,
    "clientIp": "192.168.1.100"
  }
}
```

---

## Integration & Execution Flow

### Middleware Registration Order (Program.cs)

```csharp
var app = builder.Build();

app.UseForwardedHeaders();                                    // Line 51
app.UseMiddleware<ExceptionHandlingMiddleware>();            // Line 53 - FIRST
app.UseHttpsRedirection();                                    // Line 55
app.UseCors("AllowSpecificOrigins");                          // Line 57
app.UseRateLimiter();                                         // Line 59
app.UseAuthentication();                                      // Line 61
app.UseMiddleware<TenantResolutionMiddleware>();             // Line 62 - SECOND
app.UseMiddleware<StorefrontSubscriptionMiddleware>();       // Line 63 - THIRD
app.UseAuthorization();                                       // Line 65
app.MapHub<OrderHub>("/hubs/order");                          // Line 67
app.UseMiddleware<SubscriptionEnforcementMiddleware>();      // Line 69 - FOURTH
app.MapControllers();                                         // Line 77
```

### Complete Request Processing Pipeline

```
HTTP Request
    ↓
ExceptionHandlingMiddleware (wraps entire pipeline in try-catch)
    ↓
ForwardedHeaders
    ↓
HTTPS Redirection
    ↓
CORS
    ↓
RateLimiter
    ↓
Authentication Middleware (validates JWT, extracts claims)
    ↓
TenantResolutionMiddleware (resolve merchantId from route/query)
    ↓
StorefrontSubscriptionMiddleware (validate storefront access)
    ↓
Authorization Middleware (check roles)
    ↓
SignalR Hub Mapping (/hubs/order)
    ↓
SubscriptionEnforcementMiddleware (check subscription status)
    ↓
Routing & Controller Selection
    ↓
ACTION FILTERS (CheckFeatureLimitFilter)
    ↓
Controller Action Execution
    ↓
RESULT FILTERS (ApiResponseFilter wraps response)
    ↓
Response sent to client
```

---

## Error Handling

### Exception Type Mapping

| Exception Type                | HTTP Status | Error Type                | Use Case                         |
| ----------------------------- | ----------- | ------------------------- | -------------------------------- |
| `ValidationException`         | 400         | "validation-error"        | Input validation failed          |
| `BadRequestException`         | 400         | "bad-request"             | Malformed request                |
| `BusinessRuleException`       | 400         | "business-rule-violation" | Business logic constraint broken |
| `UnauthorizedAccessException` | 401         | "unauthorized"            | Authentication failed            |
| `ForbiddenException`          | 403         | "forbidden"               | Access denied (authorization)    |
| `NotFoundException`           | 404         | "not-found"               | Resource not found               |
| `ConflictException`           | 409         | "conflict"                | Resource conflict (duplicate)    |
| `ConcurrencyException`        | 409         | "concurrency-error"       | Optimistic concurrency violation |
| Implicit 401                  | 401         | "unauthorized"            | Authentication required          |
| Implicit 403                  | 403         | "forbidden"               | Access denied                    |
| Implicit 404                  | 404         | "not-found"               | Endpoint not found               |
| Implicit 405                  | 405         | "method-not-allowed"      | HTTP method not allowed          |
| Subscription Expired          | 402         | "payment-required"        | Subscription enforcement failed  |
| Feature Limit                 | 402         | "feature-limit-exceeded"  | Feature limit exceeded           |
| Any other                     | 500         | "internal-server-error"   | Unhandled exception              |

---

## Response Envelope Structure

### Standard Success Response

```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Product Name",
    ...
  },
  "error": null,
  "meta": {
    "timestamp": "2026-03-21T10:30:45.123Z",
    "path": "/api/v1/management/products",
    "method": "GET",
    "statusCode": 200,
    "traceId": "0HN1GKNF64QCE:00000001",
    "requestId": null,
    "clientIp": "192.168.1.100"
  }
}
```

### Standard Error Response

```json
{
  "data": null,
  "error": {
    "type": "error-type-code",
    "message": "Human readable error message",
    "details": null
  },
  "meta": {
    "timestamp": "2026-03-21T10:30:45.123Z",
    "path": "/api/v1/management/products",
    "method": "POST",
    "statusCode": 400,
    "traceId": "0HN1GKNF64QCE:00000002",
    "requestId": null,
    "clientIp": "192.168.1.100"
  }
}
```

### Validation Error Response

```json
{
  "data": null,
  "error": {
    "type": "validation-error",
    "message": "The following validation errors occurred.",
    "details": [
      {
        "propertyName": "Name",
        "errorMessage": "Product name is required"
      },
      {
        "propertyName": "Price",
        "errorMessage": "Price must be greater than 0"
      }
    ]
  },
  "meta": {
    "timestamp": "2026-03-21T10:30:45.123Z",
    "path": "/api/v1/management/products",
    "method": "POST",
    "statusCode": 400,
    "traceId": "0HN1GKNF64QCE:00000003",
    "requestId": null,
    "clientIp": "192.168.1.100"
  }
}
```

---

## Complete Execution Sequence

### Example 1: Successful Request with Authentication

```
Request: GET /api/v1/management/products
Authorization: Bearer <valid-jwt-token>

↓ ExceptionHandlingMiddleware (Try block enters)
↓ Authentication ~ JWT validated, claims extracted
↓ TenantResolutionMiddleware ~ Has merchant_id claim, skip resolution
↓ StorefrontSubscriptionMiddleware ~ Not /storefront/*, skip
↓ Authorization ~ User authorized
↓ SubscriptionEnforcementMiddleware ~ subscription_status = "Active", continue
↓ Controller Action ~ Query database, return product list
↓ ApiResponseFilter ~ Wrap in ApiResponse.Success()
↓
Response Status 200:
{
  "data": [ { "id": "...", "name": "...", "price": 50000 } ],
  "error": null,
  "meta": { ... }
}
```

### Example 2: Validation Error

```
Request: POST /api/v1/management/products
Body: { "name": "", "price": -5 }

↓ Validation ~ FluentValidation runs
↓ Throws ValidationException with failures
↓ ExceptionHandlingMiddleware (catch block)
↓ Maps to 400 "validation-error"
↓
Response Status 400:
{
  "data": null,
  "error": {
    "type": "validation-error",
    "message": "...",
    "details": [
      { "propertyName": "Name", "errorMessage": "Name required" },
      { "propertyName": "Price", "errorMessage": "Price > 0" }
    ]
  },
  "meta": { ... }
}
```

### Example 3: Subscription Expired

```
Request: GET /api/v1/management/orders
Authorization: Bearer <jwt-with-subscription-expired>

↓ Authentication ~ Extract subscription_status claim: "Expired"
↓ Authorization ~ Pass
↓ SubscriptionEnforcementMiddleware
  - Is Merchant/Staff? YES
  - subscription_status = "Expired"? YES
  - Return early with 402
↓
Response Status 402:
{
  "data": null,
  "error": {
    "type": "payment-required",
    "message": "Gói cước của cửa hàng đã hết hạn..."
  },
  "meta": { ... }
}
```

---

## Best Practices

✅ **Always throw typed exceptions** - Use specific exception types (NotFoundException, ValidationException, etc.) for consistent error responses

✅ **Validate input early** - Run FluentValidation in handlers before business logic

✅ **Check subscription status** - Use `[SkipSubscriptionCheck]` only for endpoints that should work expired subscriptions (e.g., renewal pages)

✅ **Use feature limit checks** - Protect operations that consume quota with `[CheckFeatureLimit]`

✅ **Include context** - Log MerchantId and UserId for debugging issues

✅ **Cache subscription status** - Avoid repeated database queries for frequently accessed merchants

✅ **Handle cancellation** - Use `CancellationToken` throughout for graceful shutdown
