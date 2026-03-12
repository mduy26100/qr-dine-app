using QRDine.Application.Features.Dashboards.DTOs;
using QRDine.Application.Features.Dashboards.Extensions;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Dashboards.Specifications
{
    public class MonthlyOrdersSummarySpec : Specification<Order, OrderMonthlySummaryDto>
    {
        public MonthlyOrdersSummarySpec(Guid merchantId, DateTime startDate, DateTime endDate)
        {
            Query.Where(o => o.MerchantId == merchantId
                          && o.CreatedAt >= startDate
                          && o.CreatedAt <= endDate);

            Query.Select(DashboardExpressions.ToMonthlySummary);
        }
    }
}
