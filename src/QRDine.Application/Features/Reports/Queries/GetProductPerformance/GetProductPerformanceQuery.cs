using QRDine.Application.Features.Reports.DTOs;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Reports.Queries.GetProductPerformance
{
    public record GetProductPerformanceQuery(
        DateTime StartDate,
        DateTime EndDate,
        ProductPerformanceSortBy SortBy,
        int Top) : IRequest<IEnumerable<ProductPerformanceDto>>;
}
