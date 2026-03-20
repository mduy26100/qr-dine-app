using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Extensions;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Reports.Specifications
{
    public class TrafficHeatmapOrdersSpec : Specification<Order, OrderForTrafficHeatmapDto>
    {
        public TrafficHeatmapOrdersSpec(DateTime startDate, DateTime endDate)
        {
            Query.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
            Query.Select(ReportExtensions.ToOrderForTrafficHeatmapDto);
        }
    }
}
