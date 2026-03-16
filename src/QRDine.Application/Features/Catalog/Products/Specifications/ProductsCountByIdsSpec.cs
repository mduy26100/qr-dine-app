using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Specifications
{
    public class ProductsCountByIdsSpec : Specification<Product>
    {
        public ProductsCountByIdsSpec(IEnumerable<Guid> productIds)
        {
            Query.Where(p => productIds.Contains(p.Id));
        }
    }
}
