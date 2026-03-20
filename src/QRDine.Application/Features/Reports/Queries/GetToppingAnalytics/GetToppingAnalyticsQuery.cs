using QRDine.Application.Features.Reports.DTOs;

namespace QRDine.Application.Features.Reports.Queries.GetToppingAnalytics
{
    public record GetToppingAnalyticsQuery(DateTime StartDate, DateTime EndDate) : IRequest<IEnumerable<ToppingAnalyticsDto>>;
}
