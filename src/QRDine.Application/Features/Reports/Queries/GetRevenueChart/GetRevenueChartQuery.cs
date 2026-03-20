using QRDine.Application.Features.Reports.DTOs;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Reports.Queries.GetRevenueChart
{
    public record GetRevenueChartQuery(
        DateTime StartDate,
        DateTime EndDate,
        RevenueChartGrouping Grouping) : IRequest<IEnumerable<RevenueChartItemDto>>;
}
