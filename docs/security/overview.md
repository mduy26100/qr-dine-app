# Security Overview

Comprehensive security architecture, threat model, and best practices for QRDine.

## Security Architecture

QRDine implements a **defense-in-depth** security model with multiple protective layers:

```
Client Request
  ↓
[Authentication] → Verify identity (JWT token)
  ↓
[Authorization] → Check permissions (role + resource ownership)
  ↓
[Data Isolation] → Row-level filtering (multi-tenancy)
  ↓
[Input Validation] → Sanitize inputs (SQL injection prevention)
  ↓
[Audit Logging] → Track operation (compliance)
  ↓
[Encryption] → Protect data (TLS, at-rest)
```

## Security Pillars

### 1. Authentication & Passwords

User identity verification and credential protection:

- **JWT Bearer Tokens** — Stateless authentication with 10-minute access tokens
- **Refresh Token Rotation** — Prevents token replay attacks
- **PBKDF2 Hashing** — Industry-standard password hashing
- **Account Lockout** — 5 failed attempts → 15-minute lockout
- **Rate Limiting** — Per-IP limits on login attempts

**→ See [Authentication & Passwords](authentication.md) for implementation details**

### 2. Authorization & Permissions

Role-based access control with resource-level verification:

- **RBAC System** — 4 roles (SuperAdmin, Merchant, Staff, Guest)
- **Resource Ownership** — Explicit verification of data access rights
- **Triple-Layer Defense** — Authentication + authorization + data isolation
- **Multi-Tenancy** — Complete row-level isolation between merchants

**→ See [Authorization & Permissions](authorization.md) for implementation details**

### 3. Data Protection

Encryption, validation, and audit trails:

- **Input Validation** — FluentValidation on all DTOs
- **SQL Injection Prevention** — Parameterized queries (EF Core)
- **Encryption at Rest** — AES-256 for sensitive fields
- **Transport Security** — TLS 1.2+ (HTTPS enforcement)
- **Soft Deletes** — Data recovery via logical deletion
- **Audit Logging** — Complete operation history
- **CSRF Protection** — Token-based request verification

**→ See [Data Protection](data-protection.md) for implementation details**

## Threat Model

### Threats & Mitigations

| Threat                      | Severity | Control Layer      | Mitigation                                             |
| --------------------------- | -------- | ------------------ | ------------------------------------------------------ |
| **Credential Theft**        | Critical | Authentication     | PBKDF2 hashing, HTTPS-only tokens, short expiry        |
| **Token Replay**            | Critical | Authentication     | Refresh token rotation, token revocation               |
| **Privilege Escalation**    | High     | Authorization      | Resource ownership checks, global query filters        |
| **Cross-Tenant Access**     | Critical | Data Isolation     | Triple-layer defense (filter + check + error handling) |
| **SQL Injection**           | Critical | Input Validation   | Parameterized queries, FluentValidation                |
| **Brute Force**             | High     | Rate Limiting      | Account lockout (5 attempts), per-IP rate limits       |
| **Data Tampering**          | High     | Audit Logging      | Complete audit trail, soft deletes for recovery        |
| **Man-in-Middle**           | Critical | Transport Security | HTTPS enforcement, HSTS headers, TLS 1.2+              |
| **Unauthorized Disclosure** | High     | Encryption         | Encryption at rest & transit, PII field masking        |
| **Malicious Input**         | High     | Input Validation   | Type checking, length limits, pattern matching         |

### Security Zones

**Public Zone** (No authentication required):

- Anonymous product catalog viewing
- QR code menu display
- Public restaurant information

**Merchant Zone** (Merchant + Staff roles):

- Order management
- Inventory management
- Staff assignment

**Admin Zone** (SuperAdmin role only):

- Merchant management
- Platform configuration
- Analytics & reporting

## Data Classification

| Classification | Examples                       | Protection Level                        |
| -------------- | ------------------------------ | --------------------------------------- |
| **Public**     | Product names, menu items      | No encryption required                  |
| **Internal**   | Merchant statistics            | Role-based access, audit logged         |
| **Sensitive**  | Email addresses, phone numbers | Encrypted at rest, audit logged         |
| **Critical**   | Passwords, tokens, secrets     | PBKDF2 hashed / encrypted, never logged |

## Compliance & Standards

- **OWASP Top 10** — Mitigates all common web vulnerabilities
- **CWE-25** — Secure coding guidelines follow Microsoft recommendations
- **GDPR** — Soft deletes enable data retention compliance
- **PCI DSS** — If processing payments (external service integration)

## Security Checklist for Developers

Before committing code:

- ✅ All endpoints protected with `[Authorize]` (unless public by design)
- ✅ No hardcoded secrets in code, config files, or logs
- ✅ Input validation on all DTOs via FluentValidation
- ✅ Resource ownership verified for cross-tenant data access
- ✅ Sensitive errors logged, generic errors returned to client
- ✅ Queries use parameterized EF Core methods (no string concatenation)
- ✅ No user enumeration in error messages
- ✅ CORS whitelist configured (not `*` in production)

## Security Onboarding

New developers should review in order:

1. **This document** — High-level architecture and threat model
2. [Authentication & Passwords](authentication.md) — JWT, password security, token rotation
3. [Authorization & Permissions](authorization.md) — RBAC, resource-level checks
4. [Data Protection](data-protection.md) — Validation, encryption, audit logging
5. **Code Review** — Real examples in QRDine codebase

## Incident Response

For security issues:

1. **Immediately notify** security team
2. **Stop the attack** (revoke tokens, lock accounts)
3. **Investigate** (audit logs, error traces)
4. **Remediate** (fix vulnerability, patch, deploy)
5. **Communicate** (affected users, transparency)

## Related Documentation

- [API Conventions](../api/conventions.md) — Rate limiting, CORS configuration
- [Configuration - Secrets Management](../configuration/secrets-management.md) — Key vault setup
- [Database - Multi-Tenancy Design](../database/multi-tenancy.md) — Data isolation strategy
