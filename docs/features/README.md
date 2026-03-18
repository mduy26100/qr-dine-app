# Features

Domain modules, feature documentation, and use cases.

## Available Modules

### [Catalog Module](catalog/)

Menu and table management for restaurants.

- Categories (hierarchical organization)
- Products (menu items with pricing, images)
- Tables (physical restaurant tables with QR codes)
- Toppings (customization options)

**Key entities:** Category, Product, Table, Topping, ToppingGroup, ProductToppingGroup  
**Use case:** Restaurant owner manages menu structure and table setup  
[Learn more →](catalog/)

---

### [Identity Module](identity/)

User authentication, registration, and role management.

- User accounts with email verification
- JWT Bearer authentication with refresh tokens
- Role-based access control (4 roles)
- Staff registration and management

**Key entities:** ApplicationUser, ApplicationRole, RefreshToken  
**Use case:** Secure user authentication and authorization  
[Learn more →](identity/)

---

### [Sales Module](sales/)

Order management with real-time tracking.

- Order creation and lifecycle
- Order items with individual status tracking
- Real-time updates via SignalR
- Order code generation for kitchen displays

**Key entities:** Order, OrderItem  
**Use case:** Customers create orders, kitchen prepares, staff tracks progress  
[Learn more →](sales/)

---

### [Billing Module](billing/)

Subscription plans and feature limiting.

- Multiple pricing plans (Free, Basic, Pro, Enterprise)
- Feature availability per plan
- Usage tracking and enforcement
- Subscription lifecycle management

**Key entities:** Plan, Subscription, FeatureLimit, Transaction  
**Use case:** Merchants subscribe to plans, system enforces feature limits  
[Learn more →](billing/)

---

### [Tenant Module](tenant/)

Multi-tenancy management and merchant data.

- Merchant (restaurant) profile and settings
- Complete data isolation per merchant
- Subscription association

**Key entities:** Merchant  
**Use case:** Each restaurant operates independently within system  
[Learn more →](tenant/)

---

### [Staffs Module](staffs/)

Staff member management within merchant accounts.

- Staff registration and role assignment
- Limited permissions within merchant scope
- Staff performance tracking

**Key entities:** ApplicationUser (with Staff role)  
**Use case:** Restaurant owners manage staff members and permissions  
[Learn more →](staffs/)

---

## Module Organization

```
Features/
├── Catalog/           → Categories, Products, Tables, Toppings
├── Sales/             → Orders, OrderItems, Real-time tracking
├── Identity/          → Login, Register, Token management
├── Billing/           → Plans, Subscriptions, Feature limits
├── Tenant/            → Merchant data
└── Staffs/            → Staff management
```

Each module is self-contained and contains:

- **Commands** — Write operations (create, update, delete)
- **Queries** — Read operations (get, list, search)
- **DTOs** — Request/response models
- **Specifications** — Reusable query filters
- **Repositories** — Data access abstraction
- **Mappings** — Entity to DTO conversion
- **Validators** — Input validation rules

## Module Dependencies

```
┌─────────────┐
│   Catalog   │─────────────────┐
└─────────────┘                 │
                                ├──────────▶ ┌──────────┐
┌─────────────┐                 │            │  Sales   │
│  Identity   │─────────────────┤            └──────────┘
└─────────────┘                 │
                                ├──────────▶ ┌──────────┐
┌─────────────┐                 │            │  Tenant  │
│  Billing    │─────────────────┘            └──────────┘
└─────────────┘
                    ┌──────────────────┐
                    │  Dashboard       │
                    │  Staffs          │
                    └──────────────────┘
```

## Feature Limiting

Different subscription plans have different feature limits:

```
Free Plan:
  - 10 products
  - 50 orders/month
  - 1 table
  - 1 staff

Basic Plan:
  - 50 products
  - 500 orders/month
  - 5 tables
  - 3 staff

Pro Plan:
  - Unlimited products
  - Unlimited orders
  - Unlimited tables
  - Unlimited staff
```

The system enforces these limits via:

1. **CheckFeatureLimitAttribute** — Validates at endpoint level
2. **SubscriptionEnforcementMiddleware** — Validates in middleware
3. **Handler-level checks** — Final validation in business logic

## Future Modules

Potential modules for expansion:

- **Payments** — Payment gateway integrations (Stripe, PayPal)
- **Analytics** — Detailed business intelligence
- **Notifications** — Push notifications, SMS
- **Inventory** — Stock management and alerts
- **Reviews** — Customer ratings and reviews
- **Loyalty** — Rewards program
- **Deliveries** — Delivery management

---

## Quick Links

| I want to...          | See                          |
| --------------------- | ---------------------------- |
| Manage menu items     | [Catalog Module](catalog/)   |
| Handle authentication | [Identity Module](identity/) |
| Process orders        | [Sales Module](sales/)       |
| Manage subscriptions  | [Billing Module](billing/)   |
| Handle multi-tenancy  | [Tenant Module](tenant/)     |
| Manage staff          | [Staffs Module](staffs/)     |

---

**Reference:** See also [Architecture Overview](../architecture/) for system design and [Development Guidelines](../development/) for implementation patterns.
