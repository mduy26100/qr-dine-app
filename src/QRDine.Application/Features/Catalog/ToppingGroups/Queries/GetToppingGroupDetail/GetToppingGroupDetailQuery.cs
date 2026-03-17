using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Queries.GetToppingGroupDetail
{
    public record GetToppingGroupDetailQuery(Guid Id) : IRequest<ToppingGroupDetailDto>;
}
