using QRDine.Application.Features.Reports.DTOs;

namespace QRDine.Application.Features.Reports.Queries.GetRevenueSummary
{
    public record GetRevenueSummaryQuery(DateTime StartDate, DateTime EndDate) : IRequest<RevenueSummaryDto>;
}
