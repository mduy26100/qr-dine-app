using QRDine.Application.Features.Catalog.Categories.DTOs;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetMyCategories
{
    public record GetMyCategoriesQuery : IRequest<List<CategoryTreeDto>>;
}
