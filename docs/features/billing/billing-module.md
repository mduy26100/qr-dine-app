# Billing Module - Complete Documentation

Subscription management and feature limiting.

---

## Domain Entities

### Plan

A pricing tier with specific feature limits.

| Property      | Type      | Description                              |
| ------------- | --------- | ---------------------------------------- |
| `Id`          | `Guid`    | Plan identifier                          |
| `Name`        | `string`  | Plan name (Free, Basic, Pro, Enterprise) |
| `Description` | `string?` | Plan description                         |
| `Price`       | `decimal` | Monthly price in USD                     |
| `IsActive`    | `bool`    | Active toggle                            |

**Navigation properties:** `Subscriptions`, `FeatureLimits`

### Subscription

A merchant's active subscription to a plan.

| Property     | Type                 | Description                                 |
| ------------ | -------------------- | ------------------------------------------- |
| `Id`         | `Guid`               | Subscription identifier                     |
| `MerchantId` | `Guid`               | Associated merchant                         |
| `PlanId`     | `Guid`               | Subscribed plan                             |
| `Status`     | `SubscriptionStatus` | Current status (Active, Expired, Cancelled) |
| `StartDate`  | `DateTime`           | Subscription start date                     |
| `EndDate`    | `DateTime`           | Subscription end date                       |
| `AutoRenew`  | `bool`               | Auto-renewal toggle                         |

**Navigation properties:** `Merchant`, `Plan`

### FeatureLimit

Feature availability and limits per plan.

| Property      | Type          | Description                                 |
| ------------- | ------------- | ------------------------------------------- |
| `Id`          | `Guid`        | FeatureLimit identifier                     |
| `PlanId`      | `Guid`        | Plan reference                              |
| `FeatureType` | `FeatureType` | Feature identifier (Products, Orders, etc.) |
| `Limit`       | `int?`        | Maximum usage (`null` = unlimited)          |

**Navigation properties:** `Plan`

### Transaction

Payment and transaction history.

| Property          | Type            | Description                                  |
| ----------------- | --------------- | -------------------------------------------- |
| `Id`              | `Guid`          | Transaction identifier                       |
| `MerchantId`      | `Guid`          | Associated merchant                          |
| `Amount`          | `decimal`       | Transaction amount                           |
| `Currency`        | `string`        | Currency code (USD, VND, etc.)               |
| `Status`          | `PaymentStatus` | Payment status (Pending, Completed, Failed)  |
| `PaymentMethod`   | `PaymentMethod` | Method used (CreditCard, BankTransfer, etc.) |
| `TransactionDate` | `DateTime`      | Transaction timestamp                        |

---

## Feature Types

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

---

## Subscription Status Enum

```csharp
public enum SubscriptionStatus
{
    Active = 1,
    Expired = 2,
    Cancelled = 3
}
```

---

## Payment Status Enum

```csharp
public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}
```

---

## Plan Example Configuration

**Free Plan:**

- Products limit: 10
- Orders/month limit: 50
- Tables limit: 1
- Staff members limit: 1
- Price: $0

**Basic Plan:**

- Products limit: 50
- Orders/month limit: 500
- Tables limit: 5
- Staff members limit: 3
- Price: $9.99/month

**Pro Plan:**

- Products limit: Unlimited
- Orders/month limit: Unlimited
- Tables limit: Unlimited
- Staff members limit: Unlimited
- Price: $29.99/month

---

## Feature Enforcement

The system checks feature availability using `CheckFeatureLimitAttribute`:

### 1. Endpoint-Level Enforcement

```csharp
[HttpPost("products")]
[Authorize]
[CheckFeatureLimit(FeatureType.Products)]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest req)
{
    // Handler only executes if feature limit not exceeded
    var result = await _mediator.Send(new CreateProductCommand(req));
    return Ok(result);
}
```

### 2. Middleware-Level Enforcement

`SubscriptionEnforcementMiddleware` validates subscription status for all requests:

- Checks if merchant's subscription is active
- Blocks requests if subscription expired
- Returns 403 Forbidden with clear message

### 3. Handler-Level Enforcement

CQRS handlers can include additional checks:

```csharp
// Get current usage
var currentCount = await _repository.CountAsync(spec);

// Get feature limit
var featureLimit = await _featureLimitService.GetLimit(merchantId, FeatureType.Products);

// Enforce limit
if (featureLimit.HasValue && currentCount >= featureLimit.Value)
{
    throw new ForbiddenException("Product limit reached for your plan");
}
```

---

## API Endpoints (Admin Only)

### Get All Plans

**Endpoint:** `GET /api/v1/admin/plans`  
**Auth:** `SuperAdmin` role required  
**Output:** `List<PlanResponseDto>`

Lists all available plans with their feature limits.

### Create Plan

**Endpoint:** `POST /api/v1/admin/plans`  
**Auth:** `SuperAdmin` role required  
**Input:** `CreatePlanDto`  
**Output:** `PlanResponseDto`

Creates a new custom plan with feature limits.

### Get Merchant Subscription

**Endpoint:** `GET /api/v1/admin/merchants/{id}/subscription`  
**Auth:** `SuperAdmin` role required  
**Output:** `SubscriptionResponseDto`

Gets the current subscription and feature limits for a specific merchant.

---

## Subscription Lifecycle

```
1. Merchant registers
   ↓
2. System assigns Free plan
   ↓
3. Merchant upgrades to Basic plan
   ↓
4. Subscription becomes Active
   ↓
5. Each month, renewal payment processed
   ↓
6. If payment fails, subscription expires
   ↓
7. Merchant can cancel anytime
```

---

## Usage Tracking

The system tracks feature usage per merchant:

```csharp
// Get current product count
var spec = ProductsByMerchantSpec(merchantId);
var currentCount = await _repository.CountAsync(spec);

// Compare against plan limit
var limit = await _featureLimitService.GetLimit(merchantId, FeatureType.Products);

// Return usage info
return new {
    Used = currentCount,
    Limit = limit,
    Remaining = limit.HasValue ? limit.Value - currentCount : null
};
```

---

## Default Plans (Seeded)

On startup, `PlanSeeder` creates default plans:

1. **Free Plan** — No cost, limited features
2. **Basic Plan** — $9.99/month, moderate features
3. **Pro Plan** — $29.99/month, unlimited features
4. **Enterprise Plan** — Custom pricing, all features + support

---

**Reference:** See also [Billing Module Overview](README.md) and [Features Overview](../) for other modules.
