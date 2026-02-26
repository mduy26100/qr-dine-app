using QRDine.Application.Features.Catalog.Products.DTOs;

namespace QRDine.Application.Features.Catalog.Products.Queries.GetProductsByCategory
{
    public class GetProductsByCategoryQuery : IRequest<List<ProductForStorefrontDto>>
    {
        public Guid MerchantId { get; set; }
        public Guid CategoryId { get; set; }
    }
}
