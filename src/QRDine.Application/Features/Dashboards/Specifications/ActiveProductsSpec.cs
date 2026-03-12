using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Dashboards.Specifications
{
    public class ActiveProductsSpec : Specification<Product>
    {
        public ActiveProductsSpec()
        {
            Query.Where(p => p.IsAvailable);
        }
    }
}
