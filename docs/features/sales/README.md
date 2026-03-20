# Sales Module

Order management and real-time tracking.

## Quick Overview

The Sales module manages the order lifecycle, covering order creation, item tracking, and status management with real-time updates via SignalR.

## Current Status

- ✅ Domain entities fully implemented
- ✅ EF Core configurations complete
- ✅ Database schema implemented
- ✅ CQRS commands & queries complete
- ✅ API endpoints complete (Management & Storefront)
- ✅ Business logic and validations complete
- ✅ SignalR real-time updates integrated

## Key Features

- ✅ Order creation with multiple items and toppings
- ✅ Order lifecycle management (Pending → Cooking → Served → Paid/Cancelled)
- ✅ Order item status transitions with state machine validation
- ✅ Real-time order updates via SignalR notifications
- ✅ Table-based order association with occupancy tracking
- ✅ Session isolation (multiple orders per table in different sessions)
- ✅ Product & topping snapshot at order time for accurate history
- ✅ Batch item status updates for kitchen workflows
- ✅ Order history pagination with search
- ✅ Both Management (Staff) and Storefront (Customer) APIs

## Entities

| Entity        | Purpose                                              |
| ------------- | ---------------------------------------------------- |
| **Order**     | Container for all items in a single ordering session |
| **OrderItem** | Individual item within an order                      |

## Order Statuses

- **Pending** — Order placed, awaiting kitchen
- **Cooking** — Kitchen preparing items
- **Served** — Items delivered to table
- **Paid** — Payment completed
- **Cancelled** — Order cancelled

## Use Cases

1. **Customer** opens QR menu and creates order from storefront, selecting items and toppings
2. **Customer** can add more items to same order during active session
3. **Kitchen Staff** receives order notification via SignalR in real-time
4. **Kitchen Staff** updates individual items to "Preparing" or mark as "Served"
5. **Cashier/Owner** can close order as "Paid" (all items served), transitioning to payment
6. **Cashier/Owner** can close order as "Cancelled" (void the order)
7. **Customer** sees live order status updates (Pending → Cooking → Served) via real-time SignalR
8. **Owner** views order history with pagination and search by order code or table name

## API Endpoints

### Management API (Staff)

**Create Order** (Create or append to existing session)

```http
POST /api/v1.0/management/orders
Auth: Merchant, Staff
```

**Get Active Order by Table**

```http
GET /api/v1.0/management/tables/{tableId:guid}/active-order
Auth: Merchant, Staff
```

**Get Order Details**

```http
GET /api/v1.0/management/orders/{orderId:guid}
Auth: Merchant, Staff
```

**Get Order History** (Paginated & searchable)

```http
GET /api/v1.0/management/orders/history
Auth: Merchant, Staff
QueryParams: searchTerm?, pageNumber=1, pageSize=10
```

**Close Order** (Mark as Paid or Cancelled)

```http
PUT /api/v1.0/management/orders/{orderId:guid}/close
Auth: Merchant, Staff
```

**Update Order Items Status** (Batch update for kitchen)

```http
PUT /api/v1.0/management/order-items/status
Auth: Merchant, Staff
```

### Storefront API (Public)

**Create Order** (Customer places order)

```http
POST /api/v1.0/storefront/merchants/{merchantId:guid}/orders
Auth: Public
```

**Get Active Order by Table** (Customer views order status)

```http
GET /api/v1.0/storefront/merchants/{merchantId:guid}/tables/{tableId:guid}/active-order?sessionId={sessionId}
Auth: Public
```

## Documentation

## API Endpoints (In Development)

| Method | Path                                    | Auth     | Purpose             |
| ------ | --------------------------------------- | -------- | ------------------- |
| `POST` | `/api/v1/storefront/orders`             | Public   | Create order        |
| `POST` | `/api/v1/storefront/orders/{id}/items`  | Public   | Add item to order   |
| `GET`  | `/api/v1/storefront/orders/{id}`        | Public   | Get order status    |
| `PUT`  | `/api/v1/management/orders/{id}/status` | Merchant | Update order status |

## Real-Time Setup

Orders use SignalR `/hubs/order` hub for real-time updates:

- New orders created
- Order item status changes
- Order completion
- All updates specific to merchant (via merchant group)

## Documentation

- **[Complete Sales Module Documentation](sales-module.md)** — Full documentation with all entities, configurations, and status transitions

---

**Reference:** See also [Features Overview](../) for other modules and [Development Guidelines](../../development/) for implementation patterns.
