# Dashboards Module

Merchant dashboard summary and analytics at a glance.

## Quick Overview

The Dashboards module provides merchant owners with a real-time summary dashboard displaying key metrics, revenue charts, and product performance snapshots. All data is efficiently fetched and formatted for dashboard display.

## Current Status

- ✅ Dashboard query fully implemented
- ✅ Multi-metric aggregation complete
- ✅ 7-day revenue chart integration
- ✅ Product and order metrics
- ✅ API endpoint complete

## Key Features

- ✅ Revenue summary (today, this month, 7-day trend)
- ✅ 7-day revenue chart visualization
- ✅ Top performing products
- ✅ Recent orders tracking
- ✅ Monthly order statistics
- ✅ Real-time metrics aggregation
- ✅ Dashboard optimization (efficient queries)

## Data Metrics

| Metric                 | Purpose                                   |
| ---------------------- | ----------------------------------------- |
| **Today's Revenue**    | Total revenue for current day             |
| **This Month Revenue** | Cumulative revenue for current month      |
| **Orders Count**       | Total orders placed                       |
| **Top Products**       | Best-selling products                     |
| **7-Day Chart**        | Daily revenue progression for last 7 days |
| **Recent Orders**      | Latest orders in system                   |

##Use Cases

1. **Owner** opens dashboard on login
2. **Owner** sees sales snapshot at a glance
3. **Owner** reviews 7-day trend to spot patterns
4. **Owner** identifies top-performing products
5. **Owner** tracks order volume trends
6. **Manager** uses data for daily decisions
7. **Owner** compares daily performance metrics

## API Endpoints

### Management API (Merchant Owner)

**Get Dashboard Summary**

```http
GET /api/v1.0/management/dashboard/summary
Auth: Merchant
```

**Response (200 OK):**

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
    { "date": "2024-03-18", "revenue": 2500000, "orderCount": 85 }
  ],
  "topProducts": [
    {
      "productId": "550e8400...",
      "productName": "Cappuccino",
      "revenue": 8000000
    },
    {
      "productId": "550e8400...",
      "productName": "Espresso",
      "revenue": 6500000
    }
  ]
}
```

## Documentation

- **[Complete Dashboards Module Documentation](dashboards-module.md)** — Full CQRS implementation details, DTOs, metrics calculation, specifications

---

**See also:** [Features Overview](../) · [Reports Module](../reports/) · [Sales Module](../sales/)
