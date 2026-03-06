using QRDine.Application.Features.Catalog.Categories.DTOs;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetStorefrontMenu
{
    public record GetStorefrontMenuQuery(Guid MerchantId) : IRequest<List<StorefrontMenuCategoryDto>>;
}
