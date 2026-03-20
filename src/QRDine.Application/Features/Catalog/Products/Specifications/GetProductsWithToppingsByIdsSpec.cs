using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Extensions;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Specifications
{
    public class GetProductsWithToppingsByIdsSpec : Specification<Product, ProductWithToppingsDto>
    {
        public GetProductsWithToppingsByIdsSpec(Guid merchantId, IEnumerable<Guid> productIds)
        {
            Query.Where(p => p.MerchantId == merchantId
                          && productIds.Contains(p.Id)
                          && !p.IsDeleted);

            Query.Select(ProductWithToppingsExtensions.ToProductWithToppingsDto);
        }
    }
}
