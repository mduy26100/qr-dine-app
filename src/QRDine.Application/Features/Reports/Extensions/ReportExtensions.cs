using QRDine.Application.Features.Reports.DTOs;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Reports.Extensions
{
    public static class ReportExtensions
    {
        public static Expression<Func<Order, RevenueOrderDto>> ToRevenueOrderDto =>
            o => new RevenueOrderDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            };

        public static Expression<Func<Order, OrderWithItemsForPerformanceDto>> ToOrderWithItemsForPerformanceDto =>
            o => new OrderWithItemsForPerformanceDto
            {
                OrderItems = o.OrderItems
                    .Select(oi => new OrderItemForPerformanceDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Quantity = oi.Quantity,
                        Amount = oi.Amount,
                        Status = oi.Status.ToString(),
                        IsDeleted = oi.IsDeleted
                    })
                    .ToList()
            };

        public static Expression<Func<Order, OrderWithItemsForToppingDto>> ToOrderWithItemsForToppingDto =>
            o => new OrderWithItemsForToppingDto
            {
                OrderItems = o.OrderItems
                    .Select(oi => new OrderItemForToppingDto
                    {
                        Quantity = oi.Quantity,
                        ToppingsSnapshot = oi.ToppingsSnapshot
                    })
                    .ToList()
            };

        public static Expression<Func<Order, OrderForTrafficHeatmapDto>> ToOrderForTrafficHeatmapDto =>
            o => new OrderForTrafficHeatmapDto
            {
                CreatedAt = o.CreatedAt
            };
    }
}

