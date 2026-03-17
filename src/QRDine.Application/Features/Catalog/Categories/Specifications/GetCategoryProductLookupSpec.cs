using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Extensions;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Specifications
{
    public class GetCategoryProductLookupSpec : Specification<Category, CategoryLookupDto>
    {
        public GetCategoryProductLookupSpec()
        {
            Query.Where(c => c.Products.Any(p => !p.IsDeleted));

            Query.OrderBy(c => c.Parent != null ? c.Parent.DisplayOrder : c.DisplayOrder)
                 .ThenBy(c => c.DisplayOrder);

            Query.Select(CategoryExpressions.ToLookupDto);
        }
    }
}
