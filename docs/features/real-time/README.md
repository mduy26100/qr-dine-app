# Real-Time Notifications & SignalR

Real-time order updates and notifications for staff and customers.

## Quick Overview

The Real-Time Notifications system enables merchants and customers to receive live order updates via WebSocket connections using ASP.NET Core SignalR. Orders are broadcasted to connected clients as status changes occur, eliminating the need for polling.

## Current Status

- ✅ SignalR hub implementation complete
- ✅ JWT authentication for WebSocket
- ✅ Merchant-scoped messaging groups
- ✅ Order notification service complete
- ✅ Real-time order status updates

## Key Features

- ✅ Real-time order status updates
- ✅ WebSocket connection management (connect/disconnect)
- ✅ Merchant isolation - staff only see their merchant's orders
- ✅ JWT authentication for secure connections
- ✅ Automatic client-side receiving
- ✅ Low-latency message delivery
- ✅ Connection pooling and scaling support

## Architecture

### SignalR Hub: OrderHub

**Endpoint:** `wss://[domain]/hubs/order`

**Features:**

- JWT authentication via query parameter: `?access_token=<token>`
- Automatic group assignment by merchant ID
- Connection lifecycle management
- Message broadcasting to merchant groups

### Message Types

| Event                          | Direction | Payload                 | Use Case                  |
| ------------------------------ | --------- | ----------------------- | ------------------------- |
| `ReceiveOrderUpdate`           | → Client  | Order details + status  | Order created/updated     |
| `ReceiveOrderItemStatusUpdate` | → Client  | OrderItem + new status  | Item status changed       |
| `ReceiveOrderClosed`           | → Client  | Order ID + final status | Order completed/cancelled |
| `ReceiveOrderNotification`     | → Client  | Notification message    | General alerts            |

## Use Cases

1. **Customer** receives live order status (Pending → Cooking → Served)
2. **Kitchen staff** sees new orders appear in real-time
3. **Cashier** sees order updates without page refresh
4. **Owner** sees all merchant activity live on dashboard
5. **System** broadcasts event to all connected staff/customers

## API/WebSocket Connection

### Connect to OrderHub

**WebSocket URL:**

```
wss://[domain]/hubs/order?access_token=<jwt_token>
```

**Requirements:**

- Valid JWT token in query parameter
- CORS configured for WebSocket origin
- HTTPS/WSS for production

### Client-Side Example (JavaScript)

```javascript
// Connect to SignalR hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`/hubs/order?access_token=${token}`)
  .withAutomaticReconnect([0, 0, 10000])
  .build();

// Listen for order updates
connection.on("ReceiveOrderUpdate", (order) => {
  console.log("Order updated:", order);
  updateOrderUI(order);
});

// Listen for item status
connection.on("ReceiveOrderItemStatusUpdate", (item) => {
  console.log("Item status changed:", item.status);
});

// Start connection
connection.start().catch((err) => console.error(err));
```

## Documentation

- **[Complete Real-Time Documentation](real-time-module.md)** — Full SignalR implementation, hub methods, message protocols

---

**See also:** [Sales Module](../sales/) · [Features Overview](../)
