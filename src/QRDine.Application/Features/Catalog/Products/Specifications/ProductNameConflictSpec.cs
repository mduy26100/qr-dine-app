using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Specifications
{
    public class ProductNameConflictSpec : Specification<Product>
    {
        public ProductNameConflictSpec(Guid categoryId, string name)
        {
            Query.Where(x => x.CategoryId == categoryId && x.Name == name);
        }
    }
}
