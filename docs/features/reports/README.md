# Reports Module

Business intelligence and analytics for merchants.

## Quick Overview

The Reports module provides 5 powerful business intelligence queries that help merchants analyze sales performance, customer behavior, and operational metrics. All reports are cached for 30 minutes and require the `AdvancedReports` feature limit.

## Current Status

- ✅ Domain queries fully implemented
- ✅ EF Core specifications complete
- ✅ Caching strategy integrated (30-minute TTL)
- ✅ API endpoints complete
- ✅ Validation complete
- ✅ Multi-tenancy enforced

## Key Features

- ✅ Revenue summary with period-over-period trend analysis
- ✅ Revenue progression chart with flexible grouping (hourly, daily, monthly)
- ✅ Product performance ranking by revenue or volume
- ✅ Topping analytics - most popular customization options
- ✅ Traffic heatmap - order patterns by day and hour
- ✅ All reports filter by date range with automatic gap filling
- ✅ Automatic trend % calculation for KPIs
- ✅ Multi-tenancy with merchant isolation
- ✅ 30-minute intelligent caching

## Entities & Data Sources

| Source        | Purpose                                          |
| ------------- | ------------------------------------------------ |
| **Order**     | Revenue, status, creation time for all reports   |
| **OrderItem** | Product metrics, topping data, line item amounts |

## Report Types

| Report                  | Purpose           | Key Metrics                                                       |
| ----------------------- | ----------------- | ----------------------------------------------------------------- |
| **Revenue Summary**     | KPI dashboard     | Total Revenue, Orders, Avg Order Value, Cancel Rate (with trends) |
| **Revenue Chart**       | Sales progression | Revenue & Order Count over time                                   |
| **Product Performance** | Top products      | Product Name, Quantity Sold, Total Revenue                        |
| **Topping Analytics**   | Popular toppings  | Topping Name, Times Selected, Revenue Contribution                |
| **Traffic Heatmap**     | Order patterns    | Day of Week, Hour, Order Count                                    |

## Use Cases

1. **Owner** views revenue summary dashboard for daily KPIs with trends
2. **Owner** compares sales progression week-over-week using revenue chart
3. **Owner** identifies best-selling products to optimize menu
4. **Owner** sees which topping combinations drive revenue
5. **Manager** uses traffic heatmap to optimize staff scheduling
6. **Manager** plans promotions based on slow and busy hours
7. **Owner** analyzes seasonal trends filtering by date ranges

## API Endpoints

### Management API (Merchant only)

**Get Revenue Summary** (KPI Dashboard)

```http
GET /api/v1.0/management/reports/revenue-summary?startDate=2024-01-01&endDate=2024-01-31
Auth: Merchant
Feature Limit: AdvancedReports
```

**Get Revenue Chart** (Sales Progression)

```http
GET /api/v1.0/management/reports/revenue-chart?startDate=2024-01-01&endDate=2024-01-31&grouping=2
Auth: Merchant
Feature Limit: AdvancedReports
QueryParams: grouping (1=ByHour, 2=ByDay, 3=ByMonth)
```

**Get Product Performance** (Top Products)

```http
GET /api/v1.0/management/reports/product-performance?startDate=2024-01-01&endDate=2024-01-31&sortBy=1&top=10
Auth: Merchant
Feature Limit: AdvancedReports
QueryParams: sortBy (1=Revenue, 2=Volume), top (1-10000, default 10)
```

**Get Topping Analytics** (Popular Toppings)

```http
GET /api/v1.0/management/reports/topping-analytics?startDate=2024-01-01&endDate=2024-01-31
Auth: Merchant
Feature Limit: AdvancedReports
```

**Get Traffic Heatmap** (Order Patterns)

```http
GET /api/v1.0/management/reports/traffic-heatmap?startDate=2024-01-01&endDate=2024-01-31
Auth: Merchant
Feature Limit: AdvancedReports
```

## Documentation
