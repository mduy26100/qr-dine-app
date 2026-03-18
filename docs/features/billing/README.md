# Billing Module

Subscription plans and feature limiting.

## Quick Overview

The Billing module manages subscription management and feature availability per plan, controlling what features each merchant can access.

## Key Features

- ✅ Multiple subscription plans (Free, Basic, Pro, Enterprise)
- ✅ Feature-based access control
- ✅ Usage tracking and enforcement
- ✅ Automatic feature limit enforcement
- ✅ Subscription lifecycle management

## Pricing Plans

| Plan           | Products  | Orders/Month | Tables    | Staff     | Price  |
| -------------- | --------- | ------------ | --------- | --------- | ------ |
| **Free**       | 10        | 50           | 1         | 1         | $0     |
| **Basic**      | 50        | 500          | 5         | 3         | $9.99  |
| **Pro**        | Unlimited | Unlimited    | Unlimited | Unlimited | $29.99 |
| **Enterprise** | Unlimited | Unlimited    | Unlimited | Unlimited | Custom |

## Entities

| Entity           | Purpose                        |
| ---------------- | ------------------------------ |
| **Plan**         | Pricing tier definition        |
| **Subscription** | Merchant's active subscription |
| **FeatureLimit** | Feature availability per plan  |
| **Transaction**  | Payment history tracking       |

## Use Cases

1. **Merchant** chooses pricing plan on registration
2. **System** assigns free plan by default
3. **Merchant** adds products (limited by plan)
4. **System** checks feature limits in middleware
5. **Merchant** upgrades plan
6. **System** recalculates feature limits
7. **Admin** creates custom plans

## Feature Limiting

The system enforces feature limits at multiple levels:

```csharp
[Authorize]
[CheckFeatureLimit(FeatureType.Products)]
public async Task<IActionResult> CreateProduct(...)
{
    // Only executed if limit not exceeded
}
```

**Enforcement mechanisms:**

1. `CheckFeatureLimitAttribute` — Endpoint-level validation
2. `SubscriptionEnforcementMiddleware` — Middleware-level validation
3. Handler-level checks — Business logic validation

## API Endpoints

| Method | Path                                        | Auth  | Purpose                   |
| ------ | ------------------------------------------- | ----- | ------------------------- |
| `GET`  | `/api/v1/admin/plans`                       | Admin | List all plans            |
| `POST` | `/api/v1/admin/plans`                       | Admin | Create plan               |
| `GET`  | `/api/v1/admin/merchants/{id}/subscription` | Admin | Get merchant subscription |

## Documentation

- **[Complete Billing Module Documentation](billing-module.md)** — Full documentation with all entities and subscription management

---

**Reference:** See also [Features Overview](../) for other modules and [Configuration](../../configuration/) for billing setup.
