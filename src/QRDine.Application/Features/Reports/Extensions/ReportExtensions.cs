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
    }
}
