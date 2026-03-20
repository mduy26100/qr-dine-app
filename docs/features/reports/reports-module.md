# Reports Module - Complete Documentation

The Reports module provides 5 business intelligence queries that enable merchants to analyze sales performance, customer behavior, and operational metrics with intelligent caching.

---

## Overview

The Reports feature is built on the CQRS pattern with read-only queries that fetch data from Order and OrderItem entities. All reports:

- Require `AdvancedReports` feature limit
- Use 30-minute intelligent caching
- Support date range filtering
- Enforce merchant isolation
- Return projection DTOs optimized for JSON response

---

## CQRS Queries

### Query 1: GetRevenueSummaryQuery

**File:** `src/QRDine.Application/Features/Reports/Queries/GetRevenueSummary/`

Provides KPI dashboard with trend comparison against previous period.

**Request:**

```csharp
public class GetRevenueSummaryQuery : IRequest<RevenueSummaryDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
```

**Response:**

```csharp
public class RevenueSummaryDto
{
    public TrendValueDto TotalRevenue { get; set; }       // Total VND, trend % change
    public TrendValueDto TotalOrders { get; set; }        // Count, trend % change
    public TrendValueDto AverageOrderValue { get; set; }  // VND, trend % change
    public TrendValueDto CancelRate { get; set; }         // %, trend % change
}
```

**Trend Calculation:**

- Compares current period aggregate against previous equal-duration period
- Trend formula: `(Current - Previous) / Previous * 100`
- Example: Jan 1-31 vs Dec 1-31 (both 31 days)

**Business Logic:**

- Sums `TotalAmount` for orders with status Paid or Open
- Counts only Paid or Open orders
- Calculates cancel rate: `CancelledOrders / TotalOrders * 100`
- Fills missing periods with zero values for accurate trend calculation

**Authentication:** Requires `Merchant` role + `AdvancedReports` feature limit  
**Caching:** 30 minutes with key `RevenueSummary_{yyyy-MM-dd}_{yyyy-MM-dd}`

**Validation:**

- `StartDate` and `EndDate` not empty
- `EndDate >= StartDate`

---

### Query 2: GetRevenueChartQuery

**File:** `src/QRDine.Application/Features/Reports/Queries/GetRevenueChart/`

Visualizes revenue progression over time with flexible grouping.

**Request:**

```csharp
public class GetRevenueChartQuery : IRequest<IEnumerable<RevenueChartItemDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public RevenueChartGrouping Grouping { get; set; }  // ByHour=1, ByDay=2, ByMonth=3
}
```

**Response Item:**

```csharp
public class RevenueChartItemDto
{
    public string Label { get; set; }          // Formatted per grouping type
    public decimal Revenue { get; set; }       // Total VND for period
    public int OrderCount { get; set; }        // Number of orders
}
```

**Grouping Types:**

| Enum          | Label Format       | Example            | Use Case                   |
| ------------- | ------------------ | ------------------ | -------------------------- |
| `ByHour` (1)  | `HH:mm dd/MM/yyyy` | "14:00 25/01/2024" | Today's intraday trends    |
| `ByDay` (2)   | `dd/MM/yyyy`       | "25/01/2024"       | Weekly/monthly progression |
| `ByMonth` (3) | `MM/yyyy`          | "01/2024"          | Yearly trends              |

**Business Logic:**

- Groups Paid and Open orders by grouping type
- Fills missing time slots with zero revenue (no gaps in chart)
- Sorts chronologically
- Sums revenue and counts orders per slot

**Authentication:** Requires `Merchant` role + `AdvancedReports` feature limit  
**Caching:** 30 minutes with key `RevenueChart_{yyyy-MM-dd}_{yyyy-MM-dd}_{grouping}`

**Validation:**

- `StartDate` and `EndDate` not empty
- `EndDate >= StartDate`
- `Grouping` is valid enum

---

### Query 3: GetProductPerformanceQuery

**File:** `src/QRDine.Application/Features/Reports/Queries/GetProductPerformance/`

Ranks products by revenue or volume to identify best sellers.

**Request:**

```csharp
public class GetProductPerformanceQuery : IRequest<IEnumerable<ProductPerformanceDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ProductPerformanceSortBy SortBy { get; set; }  // Revenue=1, Volume=2
    public int Top { get; set; } = 10;                   // Limit 1-10000
}
```

**Response Item:**

```csharp
public class ProductPerformanceDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public long QuantitySold { get; set; }      // Total units
    public decimal TotalRevenue { get; set; }   // Total VND
}
```

**Sorting:**

| Sort Type     | Formula                                 | Output                |
| ------------- | --------------------------------------- | --------------------- |
| `Revenue` (1) | Sum of `OrderItem.Amount` per product   | Highest revenue first |
| `Volume` (2)  | Sum of `OrderItem.Quantity` per product | Most units sold first |

**Business Logic:**

- Filters OrderItems for date range
- Excludes deleted items (`IsDeleted = true`)
- Excludes cancelled items (`Status = Cancelled`)
- Groups by `ProductId`, `ProductName`
- Sorts and limits to top N (1-10000)

**Authentication:** Requires `Merchant` role + `AdvancedReports` feature limit  
**Caching:** 30 minutes with key `ProductPerformance_{yyyy-MM-dd}_{yyyy-MM-dd}_{sortBy}_{top}`

**Validation:**

- `StartDate` and `EndDate` not empty
- `EndDate >= StartDate`
- `SortBy` is valid enum
- `Top` > 0 and <= 10000

---

### Query 4: GetToppingAnalyticsQuery

**File:** `src/QRDine.Application/Features/Reports/Queries/GetToppingAnalytics/`

Identifies most popular topping combinations and their revenue contribution.

**Request:**

```csharp
public class GetToppingAnalyticsQuery : IRequest<IEnumerable<ToppingAnalyticsDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
```

**Response Item:**

```csharp
public class ToppingAnalyticsDto
{
    public Guid ToppingId { get; set; }
    public string ToppingName { get; set; }
    public long NumberOfTimes { get; set; }     // Times selected
    public decimal TotalRevenue { get; set; }   // VND from this topping
}
```

**Topping Snapshot Format:**

Toppings are stored as JSON in `OrderItem.ToppingsSnapshot`:

```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Extra Shot",
    "price": 50000
  },
  {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "name": "Caramel Drizzle",
    "price": 30000
  }
]
```

**Business Logic:**

- For each OrderItem in date range:
  - Deserialize `ToppingsSnapshot` JSON
  - Extract individual topping objects
  - Multiply topping price by item quantity
- Aggregate by topping ID and name
- Sort by total revenue (descending)
- Include number of times topping was selected

**Authentication:** Requires `Merchant` role + `AdvancedReports` feature limit  
**Caching:** 30 minutes with key `ToppingAnalytics_{yyyy-MM-dd}_{yyyy-MM-dd}`

**Validation:**

- `StartDate` and `EndDate` not empty
- `EndDate >= StartDate`

---

### Query 5: GetTrafficHeatmapQuery

**File:** `src/QRDine.Application/Features/Reports/Queries/GetTrafficHeatmap/`

Visualizes order traffic patterns by day of week and hour for staffing and promotion planning.

**Request:**

```csharp
public class GetTrafficHeatmapQuery : IRequest<IEnumerable<TrafficHeatmapDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
```

**Response Item:**

```csharp
public class TrafficHeatmapDto
{
    public int DayOfWeek { get; set; }      // 1=Monday, 2=Tuesday, ... 7=Sunday
    public int Hour { get; set; }           // 0-23
    public long OrderCount { get; set; }    // Orders placed in this slot
}
```

**Business Logic:**

- Groups orders by `DayOfWeek` (1-7) and `Hour` (0-23)
- Counts all orders (all statuses)
- Sorts by DayOfWeek (Monday to Sunday), then by Hour (0 to 23)
- Fills missing day/hour combinations with zero count

**Use Cases:**

- Identify peak hours (dinner time, lunch time, etc.)
- Optimize staff scheduling
- Plan promotional campaigns for slow periods
- Understand day-of-week patterns (weekend vs weekday)

**Authentication:** Requires `Merchant` role + `AdvancedReports` feature limit  
**Caching:** 30 minutes with key `TrafficHeatmap_{yyyy-MM-dd}_{yyyy-MM-dd}`

**Validation:**

- `StartDate` and `EndDate` not empty
- `EndDate >= StartDate`

---

## Data Transfer Objects

| DTO                     | Purpose                          |
| ----------------------- | -------------------------------- |
| `RevenueSummaryDto`     | KPI dashboard with trends        |
| `TrendValueDto`         | Single KPI with trend percentage |
| `RevenueChartItemDto`   | Time-bucketed revenue with count |
| `ProductPerformanceDto` | Ranked product metrics           |
| `ToppingAnalyticsDto`   | Topping aggregation metrics      |
| `TrafficHeatmapDto`     | Order count by day and hour      |

### Internal DTOs (used in specifications)

| DTO                               | Purpose                               |
| --------------------------------- | ------------------------------------- |
| `OrderForTrafficHeatmapDto`       | Minimal order data - just `CreatedAt` |
| `RevenueOrderDto`                 | Core order financial data             |
| `OrderWithItemsForPerformanceDto` | Order with product item details       |
| `OrderWithItemsForToppingDto`     | Order with topping snapshots          |
| `OrderItemForPerformanceDto`      | Product item metrics                  |
| `OrderItemForToppingDto`          | Item topping data                     |

---

## Specifications (Query Patterns)

| Specification                  | Purpose                                   | DTO Projection                    |
| ------------------------------ | ----------------------------------------- | --------------------------------- |
| `TrafficHeatmapOrdersSpec`     | Orders filtered by date, minimal fields   | `OrderForTrafficHeatmapDto`       |
| `RevenueOrdersSpec`            | Orders filtered by date, financial fields | `RevenueOrderDto`                 |
| `ProductPerformanceOrdersSpec` | Orders with items for product analysis    | `OrderWithItemsForPerformanceDto` |
| `ToppingAnalyticsOrdersSpec`   | Orders with topping snapshots             | `OrderWithItemsForToppingDto`     |

All specifications:

- Filter by `CreatedAt` between StartDate and EndDate
- Enforce merchant isolation via global query filter
- Use expression-based projections for efficient database queries

---

## Extensions

**File:** `src/QRDine.Application/Features/Reports/Extensions/ReportExtensions.cs`

Contains EF Core projection expressions:

```csharp
// Maps order to minimal traffic data
public static Expression<Func<Order, OrderForTrafficHeatmapDto>> ToOrderForTrafficHeatmapDto()

// Maps order to financial summary
public static Expression<Func<Order, RevenueOrderDto>> ToRevenueOrderDto()

// Maps order with item details for product metrics
public static Expression<Func<Order, OrderWithItemsForPerformanceDto>>
    ToOrderWithItemsForPerformanceDto()

// Maps order with item topping data
public static Expression<Func<Order, OrderWithItemsForToppingDto>>
    ToOrderWithItemsForToppingDto()
```

These expressions are used in Ardalis.Specification for efficient database projections.

---

## Caching Strategy

All reports implement 30-minute intelligent caching via `ICacheService`.

**Cache Process:**

1. Query handler checks cache using specific key
2. If found: return cached result immediately
3. If not found: fetch from database, store in cache for 30 minutes, return

**Cache Key Patterns:**

```
ProductPerformance_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{sortBy}_{top}
RevenueChart_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{grouping}
RevenueSummary_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}
ToppingAnalytics_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}
TrafficHeatmap_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}
```

**Benefits:**

- Reduces database queries for popular date ranges
- Improves dashboard load times
- Merchant-specific (MerchantId in isolation context)
- Automatic expiration ensures fresh data

---

## Enums

### RevenueChartGrouping

```csharp
public enum RevenueChartGrouping
{
    ByHour = 1,    // Hourly progression with HH:mm dd/MM/yyyy format
    ByDay = 2,     // Daily progression with dd/MM/yyyy format
    ByMonth = 3    // Monthly progression with MM/yyyy format
}
```

### ProductPerformanceSortBy

```csharp
public enum ProductPerformanceSortBy
{
    Revenue = 1,   // Sort by total revenue (highest first)
    Volume = 2     // Sort by quantity sold (highest first)
}
```

---

## API Endpoints

**Base Route:** `[Route("api/v{version:apiVersion}/management/reports")]`  
**Controller File:** `src/QRDine.API/Controllers/Management/Reports/ReportsController.cs`  
**Authorization:** `[Authorize(Roles = "Merchant")]`  
**Feature Limit:** All endpoints require `[CheckFeatureLimit(FeatureType.AdvancedReports)]`

### Endpoint Details

| Method | Path                   | Query Params                            | Response                             | Handler                    |
| ------ | ---------------------- | --------------------------------------- | ------------------------------------ | -------------------------- |
| `GET`  | `/revenue-summary`     | `startDate`, `endDate`                  | `RevenueSummaryDto`                  | GetRevenueSummaryQuery     |
| `GET`  | `/revenue-chart`       | `startDate`, `endDate`, `grouping`      | `IEnumerable<RevenueChartItemDto>`   | GetRevenueChartQuery       |
| `GET`  | `/product-performance` | `startDate`, `endDate`, `sortBy`, `top` | `IEnumerable<ProductPerformanceDto>` | GetProductPerformanceQuery |
| `GET`  | `/topping-analytics`   | `startDate`, `endDate`                  | `IEnumerable<ToppingAnalyticsDto>`   | GetToppingAnalyticsQuery   |
| `GET`  | `/traffic-heatmap`     | `startDate`, `endDate`                  | `IEnumerable<TrafficHeatmapDto>`     | GetTrafficHeatmapQuery     |

**Date Parameter Format:** `yyyy-MM-dd` (e.g., `2024-01-31`)

---

## Validation

All queries include FluentValidation validators:

**Common Validators:**

- ✓ `StartDate` not empty (Vietnamese error message)
- ✓ `EndDate` not empty
- ✓ `EndDate >= StartDate`

**Query-Specific Validators:**

- **GetRevenueChartQuery**: Validates `Grouping` is valid enum
- **GetProductPerformanceQuery**: Validates `SortBy` is valid enum, `Top` between 1-10000

**Error Handling:**

- `ValidationException` returns 400 with field-level errors
- Validators return Vietnamese localized error messages

---

## Business Rules

✅ Reports require `AdvancedReports` feature limit (enforced by attribute)  
✅ All data filtered by date range (inclusive on both ends)  
✅ Only Paid/Open orders count toward revenue metrics  
✅ Cancelled and deleted items excluded from product metrics  
✅ Trend calculations use equal-duration previous period  
✅ Revenue chart fills missing time slots with zero  
✅ Traffic heatmap includes all order statuses  
✅ Merchant isolation enforced via global query filter  
✅ All results cached for 30 minutes  
✅ Null values in JSON parsing handled gracefully

---

## Multi-Tenancy

- **Query Filtering:** Global query filter on Order entity: `e.MerchantId == CurrentMerchantId`
- **Merchant Extraction:** Derived from current user context in app service
- **Cache Scoping:** Cache keys don't include MerchantId (relies on context isolation)
- **Data Isolation:** No cross-tenant data leakage possible

---

## Error Scenarios

| Scenario                           | Response                               | Status |
| ---------------------------------- | -------------------------------------- | ------ |
| Invalid StartDate/EndDate format   | ValidationException with error details | 400    |
| EndDate before StartDate           | ValidationException                    | 400    |
| Invalid Grouping/SortBy enum       | ValidationException                    | 400    |
| Top outside 1-10000 range          | ValidationException                    | 400    |
| User lacks AdvancedReports feature | FeatureLimitException                  | 403    |
| User not Merchant role             | Unauthorized                           | 401    |
| No orders in date range            | Empty collection or zero metrics       | 200    |

---

## Example Responses

### Revenue Summary Response

```json
{
  "totalRevenue": {
    "value": 10500000,
    "trend": 15.5
  },
  "totalOrders": {
    "value": 156,
    "trend": 8.2
  },
  "averageOrderValue": {
    "value": 67307,
    "trend": 6.9
  },
  "cancelRate": {
    "value": 2.5,
    "trend": -0.8
  }
}
```

### Revenue Chart Response

```json
[
  {
    "label": "01/2024",
    "revenue": 5250000,
    "orderCount": 78
  },
  {
    "label": "02/2024",
    "revenue": 6100000,
    "orderCount": 91
  }
]
```

### Product Performance Response

```json
[
  {
    "productId": "550e8400-e29b-41d4-a716-446655440000",
    "productName": "Cappuccino",
    "quantitySold": 450,
    "totalRevenue": 3375000
  },
  {
    "productId": "550e8400-e29b-41d4-a716-446655440001",
    "productName": "Espresso",
    "quantitySold": 380,
    "totalRevenue": 2850000
  }
]
```

---

## Testing

Reports feature should be tested for:

- ✓ Accurate revenue/order calculations
- ✓ Correct trend % calculations
- ✓ Date range filtering
- ✓ Topping JSON parsing
- ✓ Product aggregation with filtering
- ✓ Traffic heatmap grouping and gap filling
- ✓ Caching behavior
- ✓ Merchant isolation
- ✓ Validation rules
