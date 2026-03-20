using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Extensions;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Reports.Specifications
{
    public class ToppingAnalyticsOrdersSpec : Specification<Order, OrderWithItemsForToppingDto>
    {
        public ToppingAnalyticsOrdersSpec(DateTime startDate, DateTime endDate)
        {
            Query.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
            Query.Select(ReportExtensions.ToOrderWithItemsForToppingDto);
        }
    }
}
