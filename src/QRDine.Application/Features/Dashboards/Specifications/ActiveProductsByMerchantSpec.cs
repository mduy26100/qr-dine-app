using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Dashboards.Specifications
{
    public class ActiveProductsByMerchantSpec : Specification<Product>
    {
        public ActiveProductsByMerchantSpec(Guid merchantId)
        {
            Query.Where(p => p.MerchantId == merchantId && p.IsAvailable);
        }
    }
}
