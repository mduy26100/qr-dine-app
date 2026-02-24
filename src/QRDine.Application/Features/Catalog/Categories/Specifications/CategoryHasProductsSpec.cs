using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Specifications
{
    public class CategoryHasProductsSpec : Specification<Category>
    {
        public CategoryHasProductsSpec(Guid categoryId)
        {
            Query.Where(x => x.Id == categoryId && x.Products.Any());
        }
    }
}
