using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Extensions;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Reports.Specifications
{
    public class ProductPerformanceOrdersSpec : Specification<Order, OrderWithItemsForPerformanceDto>
    {
        public ProductPerformanceOrdersSpec(DateTime startDate, DateTime endDate)
        {
            Query.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
            Query.Select(ReportExtensions.ToOrderWithItemsForPerformanceDto);
        }
    }
}
