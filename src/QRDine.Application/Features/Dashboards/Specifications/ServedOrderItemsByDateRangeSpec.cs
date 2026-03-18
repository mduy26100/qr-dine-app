using QRDine.Application.Features.Dashboards.DTOs;
using QRDine.Application.Features.Dashboards.Extensions;
using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Dashboards.Specifications
{
    public class ServedOrderItemsByDateRangeSpec : Specification<OrderItem, OrderItemRevenueDto>
    {
        public ServedOrderItemsByDateRangeSpec(Guid merchantId, DateTime startDate, DateTime endDate)
        {
            Query.Where(oi => oi.Order.MerchantId == merchantId
                           && oi.Status == OrderItemStatus.Served
                           && oi.CreatedAt >= startDate
                           && oi.CreatedAt <= endDate);

            Query.Select(DashboardExpressions.ToOrderItemRevenue);
        }
    }
}
