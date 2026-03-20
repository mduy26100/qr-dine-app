# Real-Time Module - Complete Documentation

The Real-Time Notifications system enables merchants, staff, and customers to receive live order updates and system events via WebSocket connections using ASP.NET Core SignalR. This eliminates polling and provides true push-based communication.

---

## Overview

Real-time notifications leverage SignalR hub for efficient WebSocket communication with automatic reconnection, merchant-based message routing, and JWT-based security.

**Key Design:**

- Hub-based messaging (not persistent connections)
- Merchant group isolation
- JWT token authentication
- Efficient broadcasting

---

## SignalR Hub: OrderHub

**File:** `src/QRDine.Infrastructure/SignalR/Hubs/OrderHub.cs`

### Connection Management

#### OnConnectedAsync

```csharp
public override async Task OnConnectedAsync()
{
    var user = Context.User;
    var merchantId = user?.FindFirst("MerchantId")?.Value;

    if (string.IsNullOrEmpty(merchantId))
    {
        Context.Abort();  // Close connection if no merchant
        return;
    }

    // Add connection to merchant group
    await Groups.AddToGroupAsync(ConnectionId, $"merchant-{merchantId}");
    await base.OnConnectedAsync();
}
```

**Logic:**

1. Extract user from Context.User (JWT validated by middleware)
2. Extract MerchantId from JWT claims
3. Abort if no merchant (unauthenticated)
4. Add connection to merchant-specific group: `merchant-{merchantId}`

**Group Name Format:** `merchant-[guid]`  
**Example:** `merchant-550e8400-e29b-41d4-a716-446655440000`

#### OnDisconnectedAsync

```csharp
public override async Task OnDisconnectedAsync(Exception? exception)
{
    var merchantId = Context.User?.FindFirst("MerchantId")?.Value;
    if (!string.IsNullOrEmpty(merchantId))
    {
        await Groups.RemoveFromGroupAsync(ConnectionId, $"merchant-{merchantId}");
    }
    await base.OnDisconnectedAsync(exception);
}
```

**Logic:**

1. Extract MerchantId from JWT
2. Remove connection from merchant group
3. Cleanup resources

---

## Notification Service

### IOrderNotificationService

**File:** `src/QRDine.Application/Features/Sales/Services/IOrderNotificationService.cs`

```csharp
public interface IOrderNotificationService
{
    Task NotifyOrderUpdatedAsync(Guid merchantId, Order order);
    Task NotifyOrderItemStatusUpdatedAsync(Guid merchantId, OrderItem item);
    Task NotifyOrderClosedAsync(Guid merchantId, Order order);
}
```

### OrderNotificationService Implementation

**File:** `src/QRDine.Infrastructure/SignalR/Services/OrderNotificationService.cs`

**Methods:**

#### NotifyOrderUpdatedAsync

```csharp
public async Task NotifyOrderUpdatedAsync(Guid merchantId, Order order)
{
    var orderDto = order.AsOrderResponseDto();  // Map to DTO
    await _hubContext.Clients
        .Group($"merchant-{merchantId}")
        .SendAsync("ReceiveOrderUpdate", orderDto);
}
```

**Sends:**

- Event: `ReceiveOrderUpdate`
- Target: All clients in `merchant-{merchantId}` group
- Payload: `OrderResponseDto` (Order summary)

#### NotifyOrderItemStatusUpdatedAsync

```csharp
public async Task NotifyOrderItemStatusUpdatedAsync(Guid merchantId, OrderItem item)
{
    var itemDto = new {
        item.Id,
        item.OrderId,
        item.Status,
        item.ProductName,
        item.Quantity
    };
    await _hubContext.Clients
        .Group($"merchant-{merchantId}")
        .SendAsync("ReceiveOrderItemStatusUpdate", itemDto);
}
```

**Sends:**

- Event: `ReceiveOrderItemStatusUpdate`
- Target: All clients in merchant group
- Payload: OrderItem summary

#### NotifyOrderClosedAsync

```csharp
public async Task NotifyOrderClosedAsync(Guid merchantId, Order order)
{
    var notification = new {
        OrderId = order.Id,
        Status = order.Status,
        TotalAmount = order.TotalAmount
    };
    await _hubContext.Clients
        .Group($"merchant-{merchantId}")
        .SendAsync("ReceiveOrderClosed", notification);
}
```

**Sends:**

- Event: `ReceiveOrderClosed`
- Target: Merchant group
- Payload: Order closure details

---

## Hub Client Interface

### IOrderHubClient

**File:** `src/QRDine.Infrastructure/SignalR/Hubs/IOrderHubClient.cs`

```csharp
public interface IOrderHubClient
{
    Task ReceiveOrderUpdate(OrderResponseDto order);
    Task ReceiveOrderItemStatusUpdate(OrderItemUpdateDto item);
    Task ReceiveOrderClosed(OrderClosureDto closure);
    Task ReceiveNotification(string message);
}
```

**Client-side Implementation:**

- Clients override these methods to handle received messages
- SignalR automatically deserializes JSON to DTOs
- Methods are async and awaitable

---

## Authentication

### JWT Token in WebSocket URL

WebSocket connections include JWT in query parameter:

```
wss://localhost:7288/hubs/order?access_token=eyJhbGciOiJIUzI1NiIs...
```

**JWT Claims Extracted:**

- `"MerchantId"` - Merchant GUID
- `"sub"` - User ID
- `"email"` - User email
- `"role"` - User role

**Validation:**

- Token verified by ASP.NET Core middleware before hub connection
- Hub receives `Context.User` with validated claims
- Invalid tokens result in aborted connection

### Token Refresh

Clients should:

1. Refresh token before expiration
2. Disconnect and reconnect with new token
3. Or implement token refresh middleware

---

## Message Flow Diagram

```
Order State Change
    ↓
UpdateOrderItemsStatusCommand Handler
    ↓
Update OrderItem status in database
    ↓
Call IOrderNotificationService.NotifyOrderItemStatusUpdatedAsync()
    ↓
OrderNotificationService maps to ItemUpdateDto
    ↓
Sends to _hubContext.Clients.Group("merchant-{id}")
    ↓
All connected clients in merchant group
    ↓
SignalR broadcasts via WebSocket
    ↓
Client receives "ReceiveOrderItemStatusUpdate" event
    ↓
Client-side handler processes (updates UI, plays sound, etc.)
```

---

## Event Message DTOs

### OrderResponseDto

```csharp
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "orderCode": "240320001",
    "tableId": "550e8400-e29b-41d4-a716-446655440001",
    "tableN ame": "Table 5",
    "status": "Cooking",
    "totalAmount": 250000,
    "createdOn": "2024-03-20T10:30:00Z",
    "sessionId": "550e8400-e29b-41d4-a716-446655440002"
}
```

### OrderItemUpdateDto

```csharp
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "orderId": "550e8400-e29b-41d4-a716-446655440001",
    "status": "Prepared",
    "productName": "Cappuccino",
    "quantity": 2
}
```

### OrderClosureDto

```csharp
{
    "orderId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "Paid",
    "totalAmount": 500000
}
```

---

## Configuration

### Startup Configuration

**In Program.cs:**

```csharp
// Add SignalR
services.AddSignalR(options => {
    options.MaximumParallelInvocationsPerClient = 1;
});

// Map hub endpoint
app.MapHub<OrderHub>("/hubs/order");
```

### CORS for WebSocket

```csharp
services.AddCors(options => {
    options.AddPolicy("SignalRPolicy", builder => {
        builder
            .WithOrigins("https://qr-dine-ui.vercel.app", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();  // Required for WebSocket
    });
});

// Apply CORS
app.UseCors("SignalRPolicy");
```

---

## Client-Side Implementation

### React/TypeScript Example

```typescript
import * as signalR from "@microsoft/signalr";

class OrderHub {
  private connection: signalR.HubConnection;

  async connect(token: string) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/order?access_token=${token}`, {
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 0, 10000, 30000])
      .build();

    // Register handlers
    this.connection.on("ReceiveOrderUpdate", (order) => {
      console.log("Order updated:", order);
      // Update UI
    });

    await this.connection.start();
  }

  disconnect() {
    this.connection?.stop();
  }
}
```

---

## Event Broadcasting Integration

### When Called from Commands

**Example: CloseOrderCommand**

```csharp
// In handler after closing order
await _orderNotificationService.NotifyOrderClosedAsync(
    merchantId,
    order
);
```

**Example: UpdateOrderItemsStatusCommand**

```csharp
// After updating item statuses
foreach (var item in updatedItems)
{
    await _orderNotificationService.NotifyOrderItemStatusUpdatedAsync(
        merchantId,
        item
    );
}
```

---

## Security Considerations

✅ JWT validation occurs before hub connection  
✅ MerchantId isolation prevents cross-tenant message leakage  
✅ CORS configured for trusted origins only  
✅ No sensitive data in client messages  
✅ Connection per user (not shared)  
✅ Token expiration handled by JWT middleware

---

## Performance Optimization

**Scaling Considerations:**

- SignalR scales horizontally with backplane (Redis/Azure)
- Groups enable efficient message routing (not broadcast to all)
- Async all-the-way prevents thread pool starvation
- Connection pooling via HubContext

**Message Frequency:**

- Order creation: 1 message
- Item status update: 1 message per item
- Order closure: 1 message
- No throttling applied (orders change infrequently)

---

## Troubleshooting

| Issue                      | Solution                                           |
| -------------------------- | -------------------------------------------------- |
| **Connection fails**       | Verify JWT token is valid and not expired          |
| **Messages not received**  | Check client is listening to correct event name    |
| **Cross-merchant leakage** | Verify MerchantId extraction from JWT              |
| **CORS errors**            | Add origin to CORS policy in appsettings.json      |
| **Reconnection issues**    | Client should implement `withAutomaticReconnect()` |

---

## Testing

Real-time module should be tested for:

- ✓ Connection establishment with valid token
- ✓ Connection abort with invalid token
- ✓ Message delivery to merchant group
- ✓ No cross-tenant message leakage
- ✓ Multiple concurrent connections per merchant
- ✓ Reconnection after network failure
- ✓ Message payload correctness
