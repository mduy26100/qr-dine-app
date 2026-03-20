using QRDine.Application.Features.Reports.DTOs;

namespace QRDine.Application.Features.Reports.Queries.GetTrafficHeatmap
{
    public record GetTrafficHeatmapQuery(DateTime StartDate, DateTime EndDate) : IRequest<IEnumerable<TrafficHeatmapDto>>;
}
