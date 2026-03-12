using QRDine.Application.Features.Dashboards.DTOs;
using QRDine.Application.Features.Dashboards.Extensions;
using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Dashboards.Specifications
{
    public class Last7DaysRevenueSpec : Specification<Order, OrderRevenueDto>
    {
        public Last7DaysRevenueSpec(Guid merchantId, DateTime startDate, DateTime endDate)
        {
            Query.Where(o => o.MerchantId == merchantId
                          && o.Status == OrderStatus.Paid
                          && o.CreatedAt >= startDate
                          && o.CreatedAt <= endDate);

            Query.Select(DashboardExpressions.ToRevenueChart);
        }
    }
}
