using QRDine.Application.Features.Catalog.Categories.DTOs;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetCategoriesByMerchant
{
    public record GetCategoriesByMerchantQuery(Guid MerchantId) : IRequest<List<CategoryTreeDto>>;
}
