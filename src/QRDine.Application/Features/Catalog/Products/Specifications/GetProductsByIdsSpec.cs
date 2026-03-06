using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Specifications
{
    public class GetProductsByIdsSpec : Specification<Product>
    {
        public GetProductsByIdsSpec(Guid merchantId, IEnumerable<Guid> productIds)
        {
            Query.Where(p => p.MerchantId == merchantId
                          && productIds.Contains(p.Id)
                          && p.IsAvailable
                          && !p.IsDeleted);
        }
    }
}
