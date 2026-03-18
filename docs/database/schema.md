# Database Overview

High-level database architecture, schema organization, and key concepts.

## Database Provider

- **DBMS:** SQL Server (2019+)
- **ORM:** Entity Framework Core 8
- **Approach:** Code-First with EF Core migrations
- **Deployment:** Automated via CI/CD pipeline

## Schema Organization

Database organized into schemas by domain (logical separation):

| Schema     | Purpose                 | Key Entities                           |
| ---------- | ----------------------- | -------------------------------------- |
| `identity` | Authentication          | Users, Roles, RefreshTokens            |
| `tenant`   | Multi-tenancy           | Merchants, Subscriptions, AuditLogs    |
| `catalog`  | Menu management         | Categories, Products, Toppings, Tables |
| `sales`    | Order management        | Orders, OrderItems                     |
| `billing`  | Billing & subscriptions | SubscriptionPlans, Subscriptions       |

**Configuration:** Schema names in `src/QRDine.Infrastructure/Persistence/Constants/SchemaNames.cs`

## Core Concepts

### Tables & Relationships

Fifteen+ tables organized across 5 schemas with proper relationships:

```
Merchants (Tenant Root)
├─ Categories → Products → OrderItems ← Orders ← Tables
├─ RefreshTokens ← Users
├─ Subscriptions → SubscriptionPlans
└─ AuditLogs ← (all entities)
```

**Primary Patterns:**

- All tables have soft-delete (`IsDeleted` flag)
- All tables track timestamps (`CreatedAt`, `UpdatedAt`)
- Multi-tenant tables stamped with `MerchantId`

<details>
<summary>See: Complete Entity Definitions →</summary>

All table definitions, column details, indexes, and SQL DDL statements are in [Database Entities](entities.md).

</details>

### Multi-Tenancy (Row-Level Isolation)

Complete data isolation ensures merchants cannot access other merchants' data through three protective layers:

1. **Layer 1: Global Query Filters** — Automatic row-level filtering at query execution
2. **Layer 2: Automatic Stamping** — New entities automatically assigned to current merchant
3. **Layer 3: Explicit Ownership Checks** — Handlers verify resource ownership

Example: Query products

```csharp
// Business logic
var products = await _context.Products.ToListAsync();

// Executed SQL (filter automatically applied)
SELECT * FROM [catalog].[Products]
WHERE [IsDeleted] = 0
  AND [MerchantId] = 'current-merchant-uuid'
```

<details>
<summary>See: Multi-Tenancy Architecture →</summary>

Detailed explanation of isolation strategy, filter configuration, ownership verification, and security testing is in [Multi-Tenancy Design](multi-tenancy.md).

</details>

## Migrations

Database migrations stored in `src/QRDine.Infrastructure/Persistence/Migrations/`.

**Key migrations:**

| #   | Name                | Purpose                                         |
| --- | ------------------- | ----------------------------------------------- |
| 1   | `InitialCreate`     | Core identity and tenant tables                 |
| 2   | `InitCatalogSchema` | Catalog schema (categories, products, toppings) |
| 3   | `InitSalesSchema`   | Sales schema (orders, order items)              |
| 4   | `InitBillingSchema` | Billing schema (plans, subscriptions)           |
| 5   | `AddMultiTenancy`   | MerchantId columns and global filters           |
| 6   | `AddAuditLogging`   | Audit table for compliance                      |

**Apply migrations:**

```bash
cd src/QRDine.API
dotnet ef database update
```

## Data Features

### Soft Deletes

All entities have `IsDeleted` flag (logical deletion):

```csharp
// Delete (soft)
category.IsDeleted = true;
category.DeletedAt = DateTime.UtcNow;
await _context.SaveChangesAsync();

// Auto-filtered in queries (Layer 1)
WHERE [IsDeleted] = 0
```

**Benefits:**

- Data recovery enabled
- Audit trails preserved
- Compliance with data retention policies

### Audit Logging

All changes tracked via `AuditLog` table:

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? MerchantId { get; set; }
    public string Action { get; set; }              // "Category.Created"
    public Guid EntityId { get; set; }
    public string? OldValues { get; set; }          // JSON
    public string NewValues { get; set; }           // JSON
    public string IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Automatically logged on entity changes via `SaveChangesAsync()`.

### Optimistic Concurrency

`Table` entity uses `RowVersion` for concurrency control:

```csharp
public byte[] RowVersion { get; set; }  // [Timestamp]
```

EF Core validates version on updates; throws if modified by another user.

## Related Documentation

- [Database Entities](entities.md) — Complete table definitions, SQL, indexes
- [Multi-Tenancy Design](multi-tenancy.md) — Isolation strategy, global filters, verification
- [Security - Authorization](../security/authorization.md) — Resource ownership checks
- [Configuration - Connection Strings](../configuration/connection-strings.md) — Database connection setup
