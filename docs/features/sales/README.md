# Sales Module

Order management and real-time tracking.

## Quick Overview

The Sales module manages the order lifecycle, covering order creation, item tracking, and status management with real-time updates via SignalR.

## Current Status

- ✅ Domain entities defined
- ✅ EF Core configurations complete
- ✅ Database schema implemented
- 🟡 CQRS commands/queries partially implemented
- 🟡 API endpoints in development
- 🟡 SignalR real-time updates in development

## Key Features

- ✅ Order creation with multiple items
- ✅ Order lifecycle management (Pending → Cooking → Served → Paid)
- ✅ Order item status tracking
- ✅ Real-time order updates via SignalR
- ✅ Table-based order association
- ✅ Session isolation (multiple orders per table in different sessions)

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

1. **Customer** creates order and adds items
2. **Customer** submits order for payment
3. **Kitchen** receives order notification via SignalR
4. **Kitchen** updates item status as prepared
5. **Staff** marks order as served
6. **Customer** sees live order status updates (SignalR)

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
