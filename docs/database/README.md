# Database

Database schema, multi-tenancy architecture, and data management patterns.

## Contents

- **[Database Overview](schema.md)** — High-level schema design:
  - 5 logical schemas (identity, tenant, catalog, sales, billing)
  - Schema organization and concepts
  - Core patterns and design decisions
  - Migrations timeline
  - Soft deletes and audit logging
  - Optimistic concurrency control

- **[Database Entities](entities.md)** — Complete table definitions:
  - All table DDL (CREATE TABLE statements)
  - Column types, constraints, defaults
  - Foreign key relationships
  - Composite indexes for performance
  - SQL views for optimization
  - Detailed explanations of each entity

- **[Multi-Tenancy Design](multi-tenancy.md)** — Row-level isolation strategy:
  - Multi-tenancy architecture (row-level model)
  - Current user context resolution
  - Layer 1: Global query filters
  - Layer 2: Automatic MerchantId stamping
  - Layer 3: Explicit ownership verification
  - Shared entities (not filtered)
  - SuperAdmin access patterns
  - Testing multi-tenancy isolation
  - Migration checklist for new entities

## Quick Navigation

| Question                          | Resource                                                                                     |
| --------------------------------- | -------------------------------------------------------------------------------------------- |
| How is the database organized?    | [Overview - Schema Organization](schema.md#schema-organization)                              |
| What tables exist?                | [Entities - Core Tables](entities.md#core-tables)                                            |
| What do the columns mean?         | [Entities](entities.md) (detailed SQL DDL)                                                   |
| How is multi-tenancy implemented? | [Multi-Tenancy - Architecture](multi-tenancy.md#multi-tenancy-architecture)                  |
| How does data isolation work?     | [Multi-Tenancy - Triple-Layer Defense](multi-tenancy.md#layer-1-global-query-filters)        |
| How are new merchants protected?  | [Multi-Tenancy - Automatic Stamping](multi-tenancy.md#layer-2-automatic-merchantid-stamping) |
| What about soft deletes?          | [Overview - Data Features](schema.md#data-features)                                          |
| How are migrations applied?       | [Overview - Migrations](schema.md#migrations)                                                |

## Entity Relationships

```
Merchants (root)
├─ Categories
│  └─ Products
│     ├─ OrderItems ← Orders ← Tables
│     └─ ProductToppings ← Toppings
├─ Subscriptions → SubscriptionPlans
├─ RefreshTokens ← Users
└─ AuditLogs ← (all entities)
```

## Key Design Patterns

| Pattern                    | Purpose                     | Implementation                                           |
| -------------------------- | --------------------------- | -------------------------------------------------------- |
| **Global Query Filters**   | Automatic tenant isolation  | EF Core `HasQueryFilter(e => e.MerchantId == CurrentId)` |
| **Soft Deletes**           | Safe data retention         | `IsDeleted` flag + filter `WHERE IsDeleted = 0`          |
| **Auto Stamping**          | Automatic tenant assignment | `SaveChangesAsync` intercepts new entities               |
| **Optimistic Concurrency** | Conflict detection          | `RowVersion` (TIMESTAMP column)                          |
| **Composite Indexes**      | Query performance           | `(MerchantId, IsDeleted, ...)`                           |

## 5 Database Schemas

| Schema       | Purpose            | Key Tables                             |
| ------------ | ------------------ | -------------------------------------- |
| **identity** | Authentication     | Users, Roles, RefreshTokens            |
| **tenant**   | Multi-tenancy root | Merchants, AuditLogs                   |
| **catalog**  | Menu/inventory     | Categories, Products, Toppings, Tables |
| **sales**    | Orders             | Orders, OrderItems                     |
| **billing**  | Subscriptions      | SubscriptionPlans, Subscriptions       |

## Multi-Tenancy Security (3 Layers)

Every request protected by three independent checks:

1. **Layer 1: Global Query Filters**
   - Automatic `WHERE MerchantId = @currentMerchant`
   - Applied to ALL queries (even bugs can't bypass)

2. **Layer 2: Automatic MerchantId Stamping**
   - New entities auto-assigned current merchant
   - Prevents manual override to wrong tenant

3. **Layer 3: Explicit Ownership Verification**
   - Handler double-checks: `if (entity.MerchantId != current)`
   - Final defensive validation

**Defense Depth:** Even if Layer 1 has a bug, Layers 2+3 catch it.

## Performance Considerations

**Indexes for Filtering:**

All multi-tenant queries include `MerchantId` in WHERE clause:

```sql
-- Fast (uses composite index)
SELECT * FROM Orders
WHERE MerchantId = @merchantId AND Status = 'Open'

-- Slow (full table scan)
SELECT * FROM Orders
WHERE Status = 'Open'  -- Missing MerchantId!
```

**Index Strategy:** Every index on filtered queries leads with `MerchantId`.

---

**Related:** [Security - Authorization](../security/authorization.md) • [API - Authentication](../api/authentication.md) • [Configuration - Connection Strings](../configuration/connection-strings.md)

┌──────────────────────────────────────┐
│ catalog │
├──────────────────────────────────────┤
│ Categories │
│ Products │
│ Tables │
│ Toppings │
│ ToppingGroups │
│ ProductToppingGroups │
└──────────────────────────────────────┘

┌──────────────────────────────────────┐
│ sales │
├──────────────────────────────────────┤
│ Orders │
│ OrderItems │
└──────────────────────────────────────┘

┌──────────────────────────────────────┐
│ billing │
├──────────────────────────────────────┤
│ Plans │
│ Subscriptions │
│ FeatureLimits │
│ Transactions │
└──────────────────────────────────────┘

```

See [Database Schema](schema.md) for complete documentation, table definitions, and SQL examples.

---

**Reference:** See also [Configuration](../configuration/) for connection string setup and [Getting Started](../getting-started.md) for database migration commands.
```
