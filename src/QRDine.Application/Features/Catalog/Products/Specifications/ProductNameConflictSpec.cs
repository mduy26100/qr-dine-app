using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Specifications
{
    public class ProductNameConflictSpec : Specification<Product>
    {
        public ProductNameConflictSpec(Guid categoryId, string name, Guid? excludeId = null, bool includeDeleted = false)
        {
            Query.Where(x => x.CategoryId == categoryId && x.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                Query.Where(x => x.Id != excludeId.Value);
            }

            if (includeDeleted)
            {
                Query.IgnoreQueryFilters();
            }
        }
    }
}
