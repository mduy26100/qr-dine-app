using QRDine.Application.Features.Catalog.Categories.DTOs;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetCategoryProductLookup
{
    public record GetCategoryProductLookupQuery() : IRequest<List<CategoryLookupDto>>;
}
