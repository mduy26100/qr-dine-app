# Features Overview

High-level overview of QRDine's domain modules and their capabilities.

## Catalog Module

Manages restaurant menu structure and physical table management.

### Entities

- **Category** — Hierarchical menu categories (e.g., Appetizers > Soups)
- **Product** — Menu items with pricing, images, descriptions
- **Topping** — Customization options (e.g., Extra Cheese, No Onions)
- **ToppingGroup** — Logical grouping of toppings (e.g., "Extras", "Sauce Style")
- **ProductToppingGroup** — Links products to applicable topping groups
- **Table** — Physical restaurant tables with QR code identifiers

### Key Features

- Hierarchical category organization with parent-child relationships
- Product images uploaded to Cloudinary
- QR code generation for each table
- Topping customization with pricing
- Soft delete support (IsDeleted flag)
- Real-time table occupancy tracking

### Use Cases

1. **Restaurant Owner** creates category hierarchy and manages products
2. **Customer** scans table QR code to view menu
3. **Customer** selects product with customization options
4. **System** generates product variations based on topping selections
5. **Staff** manages table status and QR codes

### API Endpoints

**Management (Merchant only):**

- `POST /api/v1/management/categories` — Create category
- `PUT /api/v1/management/categories/{id}` — Update category
- `DELETE /api/v1/management/categories/{id}` — Delete category
- `GET /api/v1/management/products` — List merchant's products
- `POST /api/v1/management/tables` — Create table

**Storefront (Public):**

- `GET /api/v1/storefront/categories?merchantId={id}` — List merchant's categories
- `GET /api/v1/storefront/products?merchantId={id}` — Browse menu
- `GET /api/v1/storefront/tables/{merchantId}` — Get table info and QR

## Sales Module

Manages customer orders and order items with real-time tracking.

### Entities

- **Order** — Order header with status, total, customer info
- **OrderItem** — Individual items in an order with status

### Key Features

- Order lifecycle management (Open → Paid → Completed/Cancelled)
- Real-time order updates via SignalR
- Order items with individual status tracking
- Order code generation for kitchen displays
- Table-based order association
- Customer name and phone optional
- Session isolation (multiple orders per table in different sessions)

### Order Statuses

- **Open** — Customer adding items, not yet paid
- **Paid** — Payment confirmed, ready for preparation
- **Cancelled** — Order cancelled by customer or staff

### Order Item Statuses

- **Pending** — Received, waiting for kitchen
- **Processing** — Kitchen preparing
- **Completed** — Ready for pickup
- **Cancelled** — Item cancelled

### Use Cases

1. **Customer** creates order and adds items
2. **Customer** pays for order
3. **Kitchen** receives order notification via SignalR
4. **Kitchen** updates item status as prepared
5. **Staff** marks order as served
6. **Customer** views live order status

### API Endpoints

**Management (Merchant/Staff):**

- `GET /api/v1/management/orders` — List orders
- `PUT /api/v1/management/orders/{id}/status` — Update order status
- `GET /api/v1/management/orders/{id}/items` — Get order items

**Storefront (Customer):**

- `POST /api/v1/storefront/orders` — Create order
- `POST /api/v1/storefront/orders/{id}/items` — Add item to order
- `GET /api/v1/storefront/orders/{id}` — Get current order status

### Real-Time Updates

SignalR hub at `/hubs/order` broadcasts:

- New orders created
- Order item status changes
- Order completion
- All updates specific to merchant (via merchant group)

## Identity Module

User authentication, registration, and role management.

### Entities (from ASP.NET Core Identity)

- **ApplicationUser** — User account with email, password hash
- **ApplicationRole** — Role definition (Merchant, Staff, Guest)
- **RefreshToken** — Token refresh tracking (prevent reuse)

### Key Features

- Merchant registration with email verification
- Staff registration by merchant
- JWT Bearer authentication
- Refresh token rotation
- Role-based access control
- Email confirmation flow
- Password hashing with ASP.NET Core Identity

### User Roles

- **SuperAdmin** — Platform administrator (all access)
- **Merchant** — Restaurant owner (owns stores)
- **Staff** — Restaurant employee (limited access)
- **Guest** — Customer (read-only, public endpoints)

### Use Cases

1. **Restaurant Owner** registers new merchant account
2. **System** sends confirmation email
3. **Owner** confirms email
4. **Owner** logs in, receives JWT token
5. **Owner** adds staff members
6. **Staff** logs in with their credentials
7. **System** restricts access based on role

### API Endpoints

- `POST /api/v1/auth/login` — Login user
- `POST /api/v1/auth/register-merchant` — Register new restaurant
- `POST /api/v1/auth/refresh-token` — Refresh access token
- `POST /api/v1/users/register-staff` — Add staff (merchant only)
- `GET /api/v1/users/profile` — Get current user profile

## Billing Module

Subscription management and feature limiting.

### Entities

- **Plan** — Pricing tier (Free, Basic, Pro, Enterprise)
- **Subscription** — Merchant's active subscription
- **FeatureLimit** — Feature availability per plan (products limit, order limit)
- **Transaction** — Payment history

### Key Features

- Multiple pricing plans with different feature limits
- Feature-based access control
- Usage tracking (products count, orders count, etc.)
- Subscription status management (Active, Expired, Cancelled)
- Payment transaction tracking
- Automatic feature enforcement via middleware

### Plan Example

```
Free Plan: 10 products, 50 orders/month
Basic Plan: 50 products, 500 orders/month
Pro Plan: Unlimited products, unlimited orders
```

### Use Cases

1. **Merchant** chooses pricing plan on registration
2. **System** assigns free plan by default
3. **Merchant** adds products (limited by plan)
4. **System** checks feature limits in middleware
5. **Merchant** upgrades plan
6. **System** recalculates limits
7. **Admin** creates custom plans with different features

### API Endpoints (Admin)

- `GET /api/v1/admin/plans` — List all plans
- `POST /api/v1/admin/plans` — Create plan
- `GET /api/v1/admin/merchants/{id}/subscription` — Get subscription

## Tenant Module

Multi-tenancy management and merchant data.

### Entities

- **Merchant** — Tenant organization (restaurant)

### Key Features

- Complete data isolation per merchant
- Merchant profile (name, logo, description, contact)
- Subscription association
- All data automatically filtered by merchant

### Use Cases

1. **Restaurant** registers as merchant
2. **System** assigns unique merchant ID
3. **Merchant** manages own data (products, orders, staff)
4. **System** isolates data from other merchants
5. **Data** filtered by merchant in all queries

## Staff Module

Staff member management within a merchant's account.

### Key Features

- Merchants can add staff members
- Staff have limited permissions within merchant's account
- Role assignment (Manager, Cashier, Kitchen)
- Staff access to orders and sales reports

### Use Cases

1. **Restaurant Owner** adds staff members
2. **Staff** can view orders and update order status
3. **Staff** cannot access merchant settings
4. **Owner** can revoke staff access

## Dashboard Module

Analytics and reporting for merchants.

### Key Features

- Sales metrics (revenue, order count, average order value)
- Best-selling products
- Order trends
- Peak hours analysis
- Staff performance metrics

### Use Cases

1. **Owner** views daily sales
2. **Owner** analyzes product performance
3. **Owner** makes data-driven decisions

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
                    │  Staff           │
                    └──────────────────┘
```

## Feature Limiting

The system enforces feature limits based on subscription plan:

```csharp
[Authorize]
[CheckFeatureLimit(FeatureType.Products)]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest req)
{
    // Only executed if merchant hasn't exceeded product limit
}
```

The `CheckFeatureLimitAttribute` (from Billing module) intercepts requests and checks:

1. Is feature available in merchant's plan?
2. Has usage limit been exceeded?

If validation fails, returns 403 Forbidden with clear message.

## Module Organization in Code

Each module is self-contained in the Application Layer:

```
Features/
├── Catalog/          → Categories, Products, Tables, Toppings
├── Sales/            → Orders, OrderItems
├── Identity/         → Login, Register, Token management
├── Billing/          → Plans, Subscriptions, Feature checks
├── Tenant/           → Merchant data
├── Dashboards/       → Analytics
└── Staffs/           → Staff management
```

Each module has:

- Commands (for write operations)
- Queries (for read operations)
- DTOs (request/response models)
- Specifications (query filters)
- Repositories (data access interfaces)
- Mappings (AutoMapper profiles)

## Future Modules

Potential modules for future expansion:

- **Payments** — Payment gateway integrations (Stripe, PayPal)
- **Analytics** — Detailed business intelligence
- **Notifications** — Push notifications, SMS
- **Inventory** — Stock management and alerts
- **Reviews** — Customer ratings and reviews
- **Loyalty** — Rewards program
- **Deliveries** — Delivery management
