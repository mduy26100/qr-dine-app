using QRDine.Application.Features.Dashboards.DTOs;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Dashboards.Extensions
{
    public static class DashboardExpressions
    {
        public static Expression<Func<OrderItem, OrderItemRevenueDto>> ToOrderItemRevenue =>
            oi => new OrderItemRevenueDto
            {
                CreatedAt = oi.CreatedAt,
                Amount = oi.Amount
            };
    }
}
