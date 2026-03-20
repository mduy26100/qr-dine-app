# Dashboards Module - Complete Documentation

The Dashboards module provides merchant owners with a comprehensive real-time dashboard displaying key KPIs, revenue charts, sales trends, and product performance metrics. All dashboard data is efficiently aggregated from Order and OrderItem entities.

---

## Overview

Dashboard provides a single entry point (query) for dashboard summary data, aggregating multiple metrics into a single optimized response for merchant dashboards.

**Key Design:**

- Efficient multi-metric query with single database call
- 7-day historical trending
- Product performance aggregation
- Real-time metrics (no caching for freshness)

---

## CQRS Query

### GetDashboardSummaryQuery

**File:** `src/QRDine.Application/Features/Dashboards/Queries/GetDashboardSummary/`

Retrieves comprehensive dashboard summary with all key metrics.

**Request:**

```csharp
public class GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>
{
    // No parameters - uses current user's merchant context
}
```

**Response:**

```csharp
public class DashboardSummaryDto
{
    public decimal TodayRevenue { get; set; }        // Today's total revenue (VND)
    public decimal ThisMonthRevenue { get; set; }    // Current month total (VND)
    public int OrdersCount { get; set; }             // Total orders (all time)
    public List<ChartDataDto> ChartData { get; set; } // 7-day revenue chart
    public List<OrderItemRevenueDto> TopProducts { get; set; } // Top 5 products
}
```

**Handler Logic:**

1. Extract current user's `MerchantId` from `ICurrentUserService`
2. Get today's date and month date range
3. Execute 3 specifications in parallel (if optimized):
   - `ActiveProductsSpec` - top products
   - `OrdersByDateRangeSpec` - 7-day chart
   - `ServedOrderItemsByDateRangeSpec` - monthly metrics
4. Aggregate and format results into `DashboardSummaryDto`
5. Return dashboard summary

**Authentication:** Requires `Merchant` role  
**Authorization:** Automatically scoped to current user's merchant  
**Caching:** None (real-time dashboard)

---

## Data Transfer Objects

### DashboardSummaryDto

**File:** `src/QRDine.Application/Features/Dashboards/DTOs/DashboardSummaryDto.cs`

Main dashboard response object aggregating all metrics.

```csharp
public class DashboardSummaryDto
{
    public decimal TodayRevenue { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public int OrdersCount { get; set; }
    public List<ChartDataDto> ChartData { get; set; }
    public List<OrderItemRevenueDto> TopProducts { get; set; }
}
```

### ChartDataDto

7-day daily revenue data point.

```csharp
public class ChartDataDto
{
    public string Date { get; set; }          // Format: yyyy-MM-dd
    public decimal Revenue { get; set; }      // Daily revenue (VND)
    public int OrderCount { get; set; }       // Orders placed that day
}
```

### OrderItemRevenueDto

Product revenue aggregate for top products.

```csharp
public class OrderItemRevenueDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Revenue { get; set; }      // Total revenue from this product (VND)
}
```

---

## Specifications

### ActiveProductsSpec

**File:** `src/QRDine.Infrastructure/Dashboards/Specifications/ActiveProductsSpec.cs`

Retrieves top revenue-generating products.

**Type:** `Specification<OrderItem, OrderItemRevenueDto>`

**Filter Chain:**

- Date range: Last 30 days (configurable)
- Order status: Only Paid/Completed orders
- Merchant scope: Filter by MerchantId
- Sort: By total revenue descending
- Limit: Top 5-10 products

---

### OrdersByDateRangeSpec

**File:** `src/QRDine.Infrastructure/Dashboards/Specifications/OrdersByDateRangeSpec.cs`

7-day daily revenue aggregation.

**Type:** `Specification<Order, ChartDataDto>`

**Filter Chain:**

- Date range: Last 7 days
- Order status: Paid/Completed
- Merchant scope: Filter by MerchantId
- Grouping: Group by date
- Calculation: Sum revenue, count orders per day
- Sort: By date ascending

---

### ServedOrderItemsByDateRangeSpec

**File:** `src/QRDine.Infrastructure/Dashboards/Specifications/ServedOrderItemsByDateRangeSpec.cs`

Monthly and today metrics.

**Type:** `Specification<OrderItem, AggregateMetricsDto>`

**Filter Chain:**

- Date range: Current month + today
- Order status: Served/Completed
- Merchant scope: Filter by MerchantId
- Calculation: Sum amount, count orders

---

## Extensions

### DashboardQueryExtensions

**File:** `src/QRDine.Infrastructure/Dashboards/Extensions/DashboardQueryExtensions.cs`

Expression-based DTO projections for efficient query execution.

**Methods:**

- `ToChartDataDto()` - Projects order to chart point
- `ToOrderItemRevenueDto()` - Projects product metrics
- `ToMetricsDto()` - Projects aggregated metrics

---

## API Endpoint

**Controller:** `src/QRDine.API/Controllers/Management/Dashboards/DashboardController.cs`

**Route:** `[Route("api/v{version:apiVersion}/management/dashboard")]`  
**API Version:** 1.0  
**Swagger Group:** Management  
**Authorization:** `[Authorize(Roles = SystemRoles.Merchant)]`

### GET /api/v1.0/management/dashboard/summary

**Method:** GET  
**No Query Parameters** — Uses authenticated user's merchant context

**Status Codes:**

- `200 OK` — Successfully retrieved dashboard
- `401 Unauthorized` — Not authenticated
- `403 Forbidden` — Not Merchant role

**Response:**

```json
{
  "todayRevenue": 2500000,
  "thisMonthRevenue": 75000000,
  "ordersCount": 156,
  "chartData": [
    { "date": "2024-03-14", "revenue": 350000, "orderCount": 15 },
    { "date": "2024-03-15", "revenue": 420000, "orderCount": 18 },
    { "date": "2024-03-16", "revenue": 380000, "orderCount": 16 },
    { "date": "2024-03-17", "revenue": 500000, "orderCount": 22 },
    { "date": "2024-03-18", "revenue": 2500000, "orderCount": 85 },
    { "date": "2024-03-19", "revenue": 1800000, "orderCount": 72 },
    { "date": "2024-03-20", "revenue": 2100000, "orderCount": 68 }
  ],
  "topProducts": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440000",
      "productName": "Cappuccino",
      "revenue": 8000000
    },
    {
      "productId": "550e8400-e29b-41d4-a716-446655440001",
      "productName": "Espresso",
      "revenue": 6500000
    },
    {
      "productId": "550e8400-e29b-41d4-a716-446655440002",
      "productName": "Americano",
      "revenue": 5200000
    }
  ]
}
```

---

## Metrics Definitions

### Today's Revenue

- **Calculation:** Sum of all order amounts where OrderDate = today AND Status = Paid/Completed
- **Format:** Vietnamese Dong (VND)
- **Timezone:** Server timezone (UTC or configured)

### This Month Revenue

- **Calculation:** Sum of all order amounts where OrderDate in [1st day of month, today] AND Status = Paid/Completed
- **Format:** Vietnamese Dong (VND)

###Orders Count

- **Calculation:** Count of all orders (all statuses, all time)
- **Purpose:** Overall volume metric

### 7-Day Chart

- **Calculation:** Group last 7 days by date, sum revenue per day, count orders per day
- **Format:** Daily breakdown with ISO date (yyyy-MM-dd)
- **Use:** Visualize weekly trends, identify patterns

### Top Products

- **Calculation:** Group order items by product, sum revenue, order by revenue descending, take top N
- **Timeframe:** Last 30 days (configurable)
- **Filter:** Only completed/served items
- **Limit:** Top 5-10 products

---

## Business Rules

✅ Dashboard data filtered strictly by merchant ID  
✅ Revenue only counts Paid/Completed orders  
✅ 7-day chart always shows 7 data points (fills zeros for no-sales days)  
✅ Chart data ordered chronologically (oldest to newest)  
✅ Top products sorted by revenue descending  
✅ All currency in VND (Vietnamese Dong)  
✅ No caching (real-time metrics)  
✅ Single query optimized for speed

---

## Multi-Tenancy

- **Isolation Level:** Query-based filtering by MerchantId
- **Enforcement:** All specifications include merchant filter
- **Additional Check:** ICurrentUserService validates MerchantId exists
- **Data Leakage:** Impossible - all queries filtered by merchant context

---

## Error Scenarios

| Scenario               | Response                             | Status |
| ---------------------- | ------------------------------------ | ------ |
| User not authenticated | Unauthorized error                   | 401    |
| User not Merchant role | Forbidden error                      | 403    |
| MerchantId null        | Throws exception (should not happen) | 500    |
| No orders/items found  | Returns zeros, empty chart/products  | 200    |
| Database error         | Returns error message                | 500    |

---

## Performance Considerations

- **Single Query:** All metrics fetched with optimized query (3 specifications)
- **Projection:** Uses EF Core expressions for efficient server-side filtering
- **Aggregation:** Database does grouping, counting (not application-level)
- **Caching:** None - updated on each request for freshness
- **Query Complexity:** O(n) where n = order count for merchant

**Potential Optimizations:**

- Implement 5-minute caching with invalidation on new orders
- Pre-aggregate daily metrics in separate table
- Archive old data (older than 1 year) to separate database

---

## Testing

Dashboard module should be tested for:

- ✓ Correct metric calculations (today, month, total)
- ✓ 7-day chart data for last 7 days (including zeros for empty days)
- ✓ Top products ranking and revenue calculation
- ✓ Multi-tenant isolation (merchant A cannot see merchant B's data)
- ✓ Edge cases (new merchant with no orders, all orders cancelled)
- ✓ Performance with large order volumes
- ✓ Timezone handling (today's revenue calculation)
