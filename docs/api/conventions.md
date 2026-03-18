# API Conventions

Request/response conventions, pagination, filtering, validation, and best practices.

## Pagination

List endpoints support pagination for large result sets:

```bash
GET /api/v1/management/products?pageNumber=1&pageSize=20
```

**Query Parameters:**

| Parameter    | Type | Default | Purpose                  |
| ------------ | ---- | ------- | ------------------------ |
| `pageNumber` | int  | 1       | Current page (1-indexed) |
| `pageSize`   | int  | 20      | Items per page (max 100) |

**Response:**

```json
{
  "data": [
    { "id": "...", "name": "Product 1" },
    { "id": "...", "name": "Product 2" }
  ],
  "meta": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

**Metadata Fields:**

- `pageNumber` — Current page requested
- `pageSize` — Items returned per page
- `totalCount` — Total items matching filter
- `totalPages` — Total pages available
- `hasNextPage` — True if page + 1 exists
- `hasPreviousPage` — True if page - 1 exists

### Pagination Best Practices

```javascript
// Client-side example
async function loadProducts(pageNumber = 1) {
  const response = await fetch(
    `/api/v1/management/products?pageNumber=${pageNumber}&pageSize=20`,
  );
  const json = await response.json();

  displayProducts(json.data);
  updatePaginationButtons(json.meta);
}

function updatePaginationButtons(meta) {
  document.querySelector("#next").disabled = !meta.hasNextPage;
  document.querySelector("#prev").disabled = !meta.hasPreviousPage;
  document.querySelector("#info").textContent =
    `Page ${meta.pageNumber} of ${meta.totalPages}`;
}
```

## Filtering

List endpoints support filtering via query parameters:

```bash
GET /api/v1/management/products?isActive=true&categoryId=550e8400-e29b-41d4-a716-446655440000&pageNumber=1&pageSize=20
```

**Common Filters:**

| Endpoint                 | Filters                                          |
| ------------------------ | ------------------------------------------------ |
| `/management/products`   | `categoryId`, `isActive`, `priceMin`, `priceMax` |
| `/management/orders`     | `status`, `tableId`, `dateFrom`, `dateTo`        |
| `/management/categories` | `isActive`, `parentId`                           |
| `/storefront/categories` | `isActive`                                       |

Supported filters documented in Swagger per endpoint.

## Sorting

Results sorted by default (usually by `createdAt` descending). Optional sort parameters:

```bash
GET /api/v1/management/products?orderBy=name&sortDescending=false
```

**Parameters:**

| Parameter        | Type    | Default | Purpose           |
| ---------------- | ------- | ------- | ----------------- |
| `orderBy`        | string  | Varies  | Column to sort by |
| `sortDescending` | boolean | false   | Sort order        |

**Example sorting for different types:**

```bash
# Sort categories by display order (ascending)
GET /api/v1/management/categories?orderBy=displayOrder&sortDescending=false

# Sort products by price (descending)
GET /api/v1/management/products?orderBy=price&sortDescending=true

# Sort orders by creation date (newest first)
GET /api/v1/management/orders?orderBy=createdAt&sortDescending=true
```

## Search

Full-text search on string fields:

```bash
GET /api/v1/management/products?search=pasta
GET /api/v1/storefront/categories?search=appetizer
```

Search is case-insensitive and matches partial strings:

```
search="pasta" matches:
- "Spaghetti Pasta"
- "Pasta Carbonara"
- "Fettuccine Pasta Bolognese"
```

## Request Validation

**All command inputs are validated automatically** via MediatR `ValidationBehavior`:

### Example: Invalid Product Create

**Request:**

```bash
POST /api/v1/management/products
Content-Type: application/json
Authorization: Bearer <token>

{
  "name": "",
  "price": -5,
  "categoryId": "invalid-guid"
}
```

**Response (400 Bad Request):**

```json
{
  "error": {
    "type": "validation-error",
    "message": "The following validation errors occurred.",
    "errors": [
      {
        "field": "name",
        "message": "Product name is required"
      },
      {
        "field": "price",
        "message": "Price must be greater than 0"
      },
      {
        "field": "categoryId",
        "message": "Invalid category ID format"
      }
    ]
  },
  "meta": {
    "timestamp": "2026-02-24T12:34:56.789Z",
    "statusCode": 400
  }
}
```

### Common Validation Rules

| Field Type  | Rule                        |
| ----------- | --------------------------- |
| Name        | Required, max 255 chars     |
| Price       | Required, > 0               |
| Email       | Required, valid format      |
| GUID        | Required, valid UUID format |
| Description | Max 1000 chars              |
| URL         | Valid URI format            |

Specific rules per endpoint documented in Swagger.

## Rate Limiting

Protects API from abuse and brute force attacks.

### Rate Limit Policies

| Endpoint                  | Limit       | Window     | Purpose                |
| ------------------------- | ----------- | ---------- | ---------------------- |
| `/auth/login`             | 5 requests  | 15 minutes | Brute force protection |
| `/auth/register-merchant` | 3 requests  | 1 hour     | Spam prevention        |
| `/users/register-staff`   | 10 requests | 1 hour     | Abuse prevention       |
| Other endpoints           | Unlimited   | -          | No rate limit          |

### Rate Limit Response

When limit exceeded:

**Response (429 Too Many Requests):**

```json
{
  "error": {
    "type": "rate-limit-exceeded",
    "message": "Too many requests. Try again after 15 minutes."
  },
  "meta": {
    "statusCode": 429,
    "retryAfter": "900"
  }
}
```

### Client Handling

Implement exponential backoff:

```javascript
async function apiRequestWithRetry(endpoint, options, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    const response = await fetch(endpoint, options);

    if (response.status === 429) {
      const retryAfter = response.headers.get("Retry-After") || 900 * (i + 1);
      const delay = parseInt(retryAfter) * 1000;

      console.warn(`Rate limited. Retrying after ${delay}ms`);
      await new Promise((resolve) => setTimeout(resolve, delay));
      continue;
    }

    return response;
  }
}
```

## CORS Policy

Cross-Origin Resource Sharing is configured per environment to prevent unauthorized cross-origin requests.

### Development Configuration

**appsettings.Development.json:**

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
  }
}
```

### Production Configuration

**appsettings.Production.json:**

```json
{
  "Cors": {
    "AllowedOrigins": ["https://qrdine.com", "https://admin.qrdine.com"]
  }
}
```

### CORS Errors

If frontend makes cross-origin request to non-whitelisted origin:

**Browser console error:**

```
Access to XMLHttpRequest at 'https://api.qrdine.com/api/v1/...'
from origin 'https://unauthorized.com' has been blocked by CORS policy
```

**Solution:** Add origin to `Cors.AllowedOrigins` in appsettings.

**Never use wildcard:**

```javascript
// ❌ Bad - Allows any origin
"AllowedOrigins": ["*"]

// ✅ Good - Whitelist specific origins
"AllowedOrigins": ["https://qrdine.com", "https://admin.qrdine.com"]
```

## Content Negotiation

Currently all endpoints return **JSON** only.

**Request:**

```bash
GET /api/v1/management/products
Accept: application/json
```

**Response:**

```json
{
  "data": [
    /* ... */
  ],
  "meta": {
    /* ... */
  }
}
```

Accept header is optional; JSON is assumed for all endpoints.

## Error Response Format

Consistent error format across all error types:

```json
{
  "error": {
    "type": "error-type",
    "message": "Human-readable message"
  },
  "meta": {
    "timestamp": "2026-02-24T12:34:56.789Z",
    "path": "/api/v1/management/products",
    "method": "POST",
    "statusCode": 400,
    "traceId": "00-abc123-def456-00"
  }
}
```

**Error Types:**

| Type                  | Status | Meaning                          |
| --------------------- | ------ | -------------------------------- |
| `validation-error`    | 400    | Input validation failed          |
| `bad-request`         | 400    | Malformed request                |
| `business-rule-error` | 400    | Business logic constraint broken |
| `unauthorized`        | 401    | Authentication required          |
| `forbidden`           | 403    | Access denied / Authorization    |
| `not-found`           | 404    | Resource not found               |
| `conflict`            | 409    | Resource conflict (duplicate)    |
| `concurrency-error`   | 409    | Optimistic lock failure          |
| `rate-limit-exceeded` | 429    | Too many requests                |
| `internal-error`      | 500    | Unexpected server error          |

No sensitive details (stack traces, SQL) returned in error responses.

## Multi-Tenancy Headers

For operations outside of JWT scope, override merchant context:

```bash
GET /api/v1/storefront/categories
X-Merchant-Id: 550e8400-e29b-41d4-a716-446655440000
```

**Supported headers:**

| Header          | Purpose                 | Example                 |
| --------------- | ----------------------- | ----------------------- |
| `X-Merchant-Id` | Override merchant ID    | UUID of target merchant |
| `X-Request-Id`  | Custom request tracking | Any correlation ID      |

## Client Best Practices

### 1. Always Check Status Code

```javascript
const response = await fetch("/api/v1/management/products", options);

if (!response.ok) {
  const error = await response.json();
  console.error(`API Error [${response.status}]:`, error.error.message);
  return;
}

const data = await response.json();
```

### 2. Handle Token Expiration

```javascript
async function apiCall(endpoint, options) {
  let response = await fetch(endpoint, {
    ...options,
    headers: {
      ...options.headers,
      Authorization: `Bearer ${getAccessToken()}`,
    },
  });

  if (response.status === 401) {
    // Token expired, refresh
    await refreshAccessToken();
    response = await fetch(endpoint, {
      ...options,
      headers: {
        ...options.headers,
        Authorization: `Bearer ${getAccessToken()}`,
      },
    });
  }

  return response;
}
```

### 3. Implement Pagination

```javascript
async function* getAllProducts() {
  let pageNumber = 1;
  let hasMore = true;

  while (hasMore) {
    const response = await fetch(
      `/api/v1/management/products?pageNumber=${pageNumber}`,
    );
    const json = await response.json();

    for (const product of json.data) {
      yield product;
    }

    hasMore = json.meta.hasNextPage;
    pageNumber++;
  }
}
```

### 4. Cache Responses Appropriately

```javascript
// Cache GET responses for 5 minutes
const cache = new Map();
const CACHE_DURATION = 5 * 60 * 1000;

async function getCachedProducts() {
  const cacheKey = "/api/v1/management/products";
  const cached = cache.get(cacheKey);

  if (cached && Date.now() - cached.timestamp < CACHE_DURATION) {
    return cached.data;
  }

  const response = await fetch(cacheKey);
  const data = await response.json();

  cache.set(cacheKey, { data, timestamp: Date.now() });
  return data;
}
```

## Related Documentation

- [API Overview](overview.md) — Response envelope, API structure
- [Authentication](authentication.md) — JWT tokens, login flow
