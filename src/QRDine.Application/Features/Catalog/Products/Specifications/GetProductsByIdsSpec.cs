using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Extensions;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Specifications
{
    public class GetProductsByIdsSpec : Specification<Product, ProductPriceDto>, ISingleResultSpecification<Product, ProductPriceDto>
    {
        public GetProductsByIdsSpec(Guid merchantId, IEnumerable<Guid> productIds)
        {
            Query.Where(p => p.MerchantId == merchantId
                          && productIds.Contains(p.Id)
                          && p.IsAvailable
                          && !p.IsDeleted);

            Query.Select(ProductExtensions.ToProductPriceDto);
        }
    }
}
