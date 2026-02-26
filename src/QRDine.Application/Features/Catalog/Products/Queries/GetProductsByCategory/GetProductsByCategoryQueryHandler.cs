using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Products.Queries.GetProductsByCategory
{
    public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, List<ProductForStorefrontDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductsByCategoryQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductForStorefrontDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var spec = new ProductsForStorefrontSpec(request.MerchantId, request.CategoryId);

            return await _productRepository.ListAsync(spec, cancellationToken);
        }
    }
}
