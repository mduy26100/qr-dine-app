# Security

Authentication, authorization, data protection, and security best practices.

## Contents

- **[Security Overview](overview.md)** — Security architecture and threat model:
  - Defense-in-depth architecture (6 layers)
  - Threat model with severity levels
  - Data classification (Public, Internal, Sensitive, Critical)
  - Security checklist for developers
  - Incident response procedures
  - Compliance standards

- **[Authentication & Passwords](authentication.md)** — User verification and credential protection:
  - JWT token structure and lifecycle
  - Login flow (5-step process)
  - Password hashing with PBKDF2
  - Password requirements and validation
  - Refresh token rotation (prevent replay attacks)
  - Account lockout after failed attempts
  - Session management and forced termination
  - Token generation algorithms

- **[Authorization & Permissions](authorization.md)** — Access control and RBAC:
  - 4 roles: SuperAdmin, Merchant, Staff, Guest
  - Controller-level and method-level authorization
  - Role-based access patterns
  - Resource ownership verification (critical!)
  - Triple-layer defense (auth + authz + isolation)
  - Permission policies and requirements
  - Common authorization patterns

- **[Data Protection](data-protection.md)** — Input validation, encryption, and compliance:
  - FluentValidation and custom validators
  - SQL injection prevention (parameterized queries)
  - Encryption at rest (AES-256)
  - Transport security (TLS/HTTPS)
  - Audit logging with full change tracking
  - Soft deletes (logical deletion)
  - Concurrency control via RowVersion
  - Secrets management best practices

## Quick Navigation

| Question                         | Resource                                                                            |
| -------------------------------- | ----------------------------------------------------------------------------------- |
| How does authentication work?    | [Authentication - JWT](authentication.md#jwt-bearer-tokens)                         |
| How is the login flow?           | [Authentication - Login Process](authentication.md#login-flow)                      |
| How do tokens refresh?           | [Authentication - Refresh Token Rotation](authentication.md#refresh-token-rotation) |
| Are passwords secure?            | [Authentication - Password Security](authentication.md#password-security)           |
| Who can do what?                 | [Authorization - RBAC](authorization.md#role-based-access-control)                  |
| How are resources protected?     | [Authorization - Resource-Level](authorization.md#resource-level-authorization)     |
| How is data isolated per tenant? | [Data Protection - Input Validation](data-protection.md#input-validation)           |
| How is data encrypted?           | [Data Protection - Encryption](data-protection.md#encryption-at-rest)               |
| What about data in transit?      | [Data Protection - TLS/HTTPS](data-protection.md#transport-security-tlshttps)       |

## Security Architecture

**6-Layer Defense-in-Depth:**

```
1. Authentication → User is who they claim (JWT valid)
   ↓
2. Authorization → User has required role
   ↓
3. Data Isolation → Multi-tenant row-level filtering
   ↓
4. Ownership Check → User owns resource being accessed
   ↓
5. Input Validation → Sanitize and validate all inputs
   ↓
6. Audit Logging → Log all sensitive operations
```

## Trust Models

| User Type      | Auth             | Authorization              | Data Access               |
| -------------- | ---------------- | -------------------------- | ------------------------- |
| **SuperAdmin** | JWT              | All operations             | All merchants' data       |
| **Merchant**   | JWT              | Resource management        | Own merchant data only    |
| **Staff**      | JWT              | Order/inventory management | Own merchant's data only  |
| **Guest**      | None (anonymous) | Public endpoints           | QR menu + storefront only |

## Key Security Features

✅ **Authentication:**

- Stateless JWT (10-min access + 7-day refresh)
- Refresh token rotation (prevents replay attacks)
- Account lockout protection (5 failures → 15-min lockout)
- Secure password hashing (PBKDF2-SHA256, 10k iterations)

✅ **Authorization:**

- Role-Based Access Control (4 roles)
- Resource-level ownership verification
- Triple-layer defense pattern

✅ **Data Protection:**

- Parameterized queries (SQL injection prevention)
- Input validation via FluentValidation
- Encryption at rest (sensitive fields)
- TLS/HTTPS for transport security
- Soft deletes for recovery

✅ **Operations:**

- Audit logging (all changes tracked)
- Concurrency control (optimistic locking)
- Rate limiting (prevent brute force/DoS)

---

**Related:** [API Authentication](../api/authentication.md) • [Database Multi-Tenancy](../database/multi-tenancy.md) • [Configuration Secrets](../configuration/secrets-management.md)

---

**Reference:** See also [API Reference](../api/) for authentication flow and [Development Guidelines](../development/) for secure coding patterns.
