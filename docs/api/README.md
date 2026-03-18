# API Reference

Complete API documentation including versioning, response formats, authentication, and endpoints.

## Contents

- **[API Overview](overview.md)** — Core API concepts:
  - API versioning and groups (Identity, Admin, Management, Storefront)
  - Response envelope format (data, meta, errors)
  - HTTP status codes and meanings
  - Swagger/OpenAPI documentation

- **[Authentication & Authorization](authentication.md)** — JWT implementation and security:
  - JWT token structure and lifecycle
  - Login and refresh token flows
  - Password hashing (PBKDF2)
  - Token rotation strategy
  - Account lockout protection
  - Session management
  - RBAC and authorization patterns

- **[API Conventions](conventions.md)** — Standard practices and protocols:
  - Pagination (limit/offset/has-next)
  - Filtering and sorting
  - Search operations
  - Request validation
  - Rate limiting policies
  - CORS configuration
  - Content negotiation
  - Client best practices

## Quick Navigation

| Need                      | Resource                                                                            |
| ------------------------- | ----------------------------------------------------------------------------------- |
| Overall API design?       | [API Overview](overview.md)                                                         |
| How authentication works? | [Authentication & Authorization](authentication.md)                                 |
| Login flow?               | [Authentication - Login Process](authentication.md#login-flow)                      |
| How to refresh tokens?    | [Authentication - Refresh Token Rotation](authentication.md#refresh-token-rotation) |
| Password security?        | [Authentication - Password Security](authentication.md#password-security)           |
| Pagination?               | [Conventions - Pagination](conventions.md#pagination)                               |
| Rate limits?              | [Conventions - Rate Limiting](conventions.md#rate-limiting)                         |
| CORS setup?               | [Conventions - CORS Configuration](conventions.md#cross-origin-resource-sharing)    |

## API Groups

| Group              | Route                           | Access         | Purpose                   |
| ------------------ | ------------------------------- | -------------- | ------------------------- |
| **Identity API**   | `/api/v1/auth`, `/api/v1/users` | Public         | Login, registration       |
| **Admin API**      | `/api/v1/admin`                 | SuperAdmin     | Platform administration   |
| **Management API** | `/api/v1/management`            | Merchant/Staff | Store management CRUD     |
| **Storefront API** | `/api/v1/storefront`            | Public         | Customer-facing endpoints |

## Key Patterns

**Authentication:**

- JWT Bearer tokens (stateless, 10-min access + 7-day refresh)
- Refresh token rotation prevents replay attacks
- Account lockout after 5 failed attempts

**Authorization:**

- 4 roles: SuperAdmin, Merchant, Staff, Guest
- Resource-level ownership verification
- Multi-tenant data isolation

**API Design:**

- Unified response envelope (success/error format)
- HTTP status codes follow REST conventions
- Pagination via optional query parameters
- Rate limiting per- endpoint and per IP

---

**Related:** [Features Overview](../features/) • [Security Overview](../security/) • [Development Guidelines](../development/)
