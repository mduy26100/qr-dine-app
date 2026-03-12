using QRDine.Application.Features.Dashboards.DTOs;

namespace QRDine.Application.Features.Dashboards.Queries.GetDashboardSummary
{
    public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
}
