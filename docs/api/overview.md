# API Overview

Complete guide to QRDine's REST API conventions, response formats, and authentication.

## API Versioning

All endpoints use URL segment versioning:

```
/api/v{version:apiVersion}/...
```

**Current version:** `v1.0`

Version parameter in route:

```csharp
[Route("api/v{version:apiVersion}/management/categories")]
[ApiVersion("1.0")]
public class CategoriesController : ControllerBase { }
```

## API Groups

The API is organized into four logical groups, each with dedicated Swagger documentation:

### 1. Identity API

**Route prefix:** `/api/v1/auth`, `/api/v1/users`  
**Access:** Public (no authentication required)  
**Purpose:** User authentication and registration

### 2. Admin API

**Route prefix:** `/api/v1/admin`  
**Access:** SuperAdmin role only  
**Purpose:** Platform administration (merchants, plans, system settings)

### 3. Management API

**Route prefix:** `/api/v1/management`  
**Access:** Authenticated merchant or staff member  
**Purpose:** Store management CRUD operations (products, categories, orders)

### 4. Storefront API

**Route prefix:** `/api/v1/storefront`  
**Access:** Public (no authentication required)  
**Purpose:** Customer-facing endpoints (browse menu, create orders)

## Unified Response Envelope

All successful and error responses follow a standardized structure managed by `ApiResponseFilter`.

### Success Response

```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Appetizers",
    "description": "Starters and small bites"
  },
  "meta": {
    "timestamp": "2026-02-24T12:34:56.789Z",
    "path": "/api/v1/management/categories",
    "method": "POST",
    "statusCode": 201,
    "traceId": "00-abc123def456ghi789jkl-monopqrst-00",
    "requestId": "req-12345",
    "clientIp": "192.168.1.100"
  }
}
```

**Fields:**

- **data** — Response payload (DTO, array, or null for 204 No Content)
- **meta.timestamp** — UTC timestamp of response
- **meta.path** — Requested URL path
- **meta.method** — HTTP method (GET, POST, etc.)
- **meta.statusCode** — HTTP status code
- **meta.traceId** — Correlation ID for request tracing
- **meta.requestId** — Unique request identifier
- **meta.clientIp** — Client IP address (for audit logging)

### Error Response

```json
{
  "error": {
    "type": "validation-error",
    "message": "The following validation errors occurred.",
    "errors": [
      {
        "field": "name",
        "message": "Name must not be empty"
      },
      {
        "field": "price",
        "message": "Price must be greater than 0"
      }
    ]
  },
  "meta": {
    "timestamp": "2026-02-24T12:34:56.789Z",
    "path": "/api/v1/management/products",
    "method": "POST",
    "statusCode": 400,
    "traceId": "..."
  }
}
```

### HTTP Status Codes

| Code | Error Type              | Meaning                            |
| ---- | ----------------------- | ---------------------------------- |
| 200  | success                 | Request successful                 |
| 201  | created                 | Resource created successfully      |
| 204  | no-content              | Success with no response body      |
| 400  | validation-error        | Input validation failed            |
| 400  | bad-request             | Malformed request                  |
| 400  | business-rule-violation | Business logic constraint violated |
| 401  | unauthorized            | Authentication required            |
| 403  | forbidden               | Access denied                      |
| 404  | not-found               | Resource not found                 |
| 409  | conflict                | Resource conflict (duplicate)      |
| 409  | concurrency-error       | Optimistic concurrency violation   |
| 500  | internal-error          | Unexpected server error            |

## Authentication

QRDine uses JWT (JSON Web Token) for stateless authentication.

### Login Flow

**Request:**

```bash
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "merchant@example.com",
  "password": "SecurePassword!123"
}
```

**Response (200 OK):**

```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "50e8400-e29b-41d4-a716-446655440000",
    "expiresIn": 600,
    "tokenType": "Bearer"
  },
  "meta": { ... }
}
```

**Token Claims:**

```json
{
  "sub": "user-id",
  "email": "merchant@example.com",
  "merchant_id": "550e8400-e29b-41d4-a716-446655440000",
  "role": "Merchant",
  "iat": 1708770896,
  "exp": 1708771496,
  "iss": "https://qrdine.com",
  "aud": "https://qrdine.com"
}
```

**Token Lifetime:**

- Access token: 10 minutes (configurable)
- Refresh token: 7 days (configurable)

### Using Authentication

Include token in `Authorization` header:

```bash
GET /api/v1/management/products
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Refresh Token Flow

When access token expires, use refresh token:

**Request:**

```bash
POST /api/v1/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "50e8400-e29b-41d4-a716-446655440000"
}
```

**Response (200 OK):**

```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new-refresh-token-guid",
    "expiresIn": 600,
    "tokenType": "Bearer"
  },
  "meta": { ... }
}
```

Refresh tokens are rotated (old token invalidated, new token issued) to prevent token replay attacks.

### Authorization

Endpoints specify required roles with `[Authorize]` attribute:

```csharp
[Authorize]                        // Any authenticated user
[Authorize(Roles = "Merchant")]    // Only merchants
[Authorize(Roles = "Merchant,SuperAdmin")] // Multiple roles
[AllowAnonymous]                   // Public endpoint
```

## Multi-Tenancy in API

Merchants are identified by automatic resolution through:

1. **JWT Token Claim** — `merchant_id` in token (primary)
2. **Header** — `X-Merchant-Id` header
3. **Route Parameter** — `{merchantId}` in URL
4. **Query Parameter** — `?merchantId=...`

**Automatic Resolution Example:**

```csharp
// All three requests return the same merchant's data

// 1. Via JWT token
GET /api/v1/management/products
Authorization: Bearer <token-with-merchant-id>

// 2. Via header
GET /api/v1/storefront/categories
X-Merchant-Id: 550e8400-e29b-41d4-a716-446655440000

// 3. Via query parameter
GET /api/v1/storefront/categories?merchantId=550e8400-e29b-41d4-a716-446655440000
```

The system resolves merchant automatically through `TenantResolutionMiddleware`.

## Pagination

List endpoints support pagination via query parameters:

```bash
GET /api/v1/management/products?pageNumber=1&pageSize=20
```

**Response includes pagination metadata:**

```json
{
  "data": [
    /* items */
  ],
  "meta": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

## Rate Limiting

The API applies rate limiting to sensitive endpoints:

| Endpoint                  | Limit | Window     |
| ------------------------- | ----- | ---------- |
| `/auth/login`             | 5     | 15 minutes |
| `/auth/register-merchant` | 3     | 1 hour     |
| `/users/register-staff`   | 10    | 1 hour     |

**Rate Limit Response (429 Too Many Requests):**

```json
{
  "error": {
    "type": "rate-limit-exceeded",
    "message": "Too many requests. Please try again later."
  }
}
```

## CORS Policy

Cross-Origin Resource Sharing is configured per environment:

**Development:**

```json
"Cors": {
  "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
}
```

**Production:**

```json
"Cors": {
  "AllowedOrigins": ["https://qrdine.com", "https://admin.qrdine.com"]
}
```

Frontend origins must be whitelisted in `appsettings.json`.

## Request Validation

All command inputs are automatically validated via `ValidationBehavior` MediatR pipeline:

```bash
POST /api/v1/management/products
Content-Type: application/json

{
  "name": "",
  "price": -5
}
```

**Response (400 Bad Request):**

```json
{
  "error": {
    "type": "validation-error",
    "message": "The following validation errors occurred.",
    "errors": [
      { "field": "name", "message": "Product name is required" },
      { "field": "price", "message": "Price must be greater than 0" }
    ]
  },
  "meta": { ... }
}
```

## Swagger Documentation

Four Swagger documents available at `/swagger`:

```
Identity API → /swagger/Identity/swagger.json
Admin API → /swagger/Admin/swagger.json
Management API → /swagger/Management/swagger.json
Storefront API → /swagger/Storefront/swagger.json
```

**Access:** https://localhost:7288/swagger

Features:

- Try-it-out functionality
- Request/response examples
- Authentication token input
- Easy endpoint discovery

## Content Negotiation

All endpoints support JSON (default):

```bash
GET /api/v1/management/categories
Accept: application/json
```

Accept header is optional; JSON is assumed.

## Filtering & Sorting

List endpoints support filtering and sorting:

```bash
GET /api/v1/management/products?isActive=true&orderBy=name&sortDescending=false
```

Supported filters vary by endpoint; check Swagger docs for specifics.

## Best Practices for API Consumers

1. **Use access token** until expiry, then refresh
2. **Handle 401 Unauthorized** by refreshing token
3. **Implement exponential backoff** for rate limit (429) responses
4. **Log traceId** from meta for debugging issues
5. **Validate responses** — always check status code before using data
6. **Use HTTPS** in production (enforce via settings)
7. **Send User-Agent header** to identify your client
8. **Cache GET responses** appropriately based on data volatility

## Related Documentation

- [Authentication Details](authentication.md)
- [Endpoint Reference](endpoints-catalog.md)
- [Error Handling](../error-handling.md)
