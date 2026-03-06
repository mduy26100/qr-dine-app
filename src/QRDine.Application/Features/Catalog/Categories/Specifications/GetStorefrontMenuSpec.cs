using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Extensions;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Specifications
{
    public class GetStorefrontMenuSpec : Specification<Category, StorefrontMenuCategoryDto>
    {
        public GetStorefrontMenuSpec(Guid merchantId)
        {
            Query.Where(c => c.MerchantId == merchantId
                          && c.IsActive
                          && c.Products.Any(p => p.IsAvailable));

            Query.OrderBy(c => c.Parent != null ? c.Parent.DisplayOrder : c.DisplayOrder)
                 .ThenBy(c => c.DisplayOrder);

            Query.Select(CategoryExpressions.ToStorefrontMenuDto);
        }
    }
}
