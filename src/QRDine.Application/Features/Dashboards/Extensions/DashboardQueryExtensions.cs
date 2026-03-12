using QRDine.Application.Features.Dashboards.DTOs;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Dashboards.Extensions
{
    public static class DashboardExpressions
    {
        public static Expression<Func<Order, OrderMonthlySummaryDto>> ToMonthlySummary =>
            o => new OrderMonthlySummaryDto
            {
                Status = o.Status,
                TotalAmount = o.TotalAmount
            };

        public static Expression<Func<Order, OrderRevenueDto>> ToRevenueChart =>
            o => new OrderRevenueDto
            {
                CreatedAt = o.CreatedAt,
                TotalAmount = o.TotalAmount
            };
    }
}
