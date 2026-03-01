using QRDine.Application.Common.Models;
using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Products.Queries.GetMyProductsByPage
{
    public class GetMyProductsByPageQueryHandler : IRequestHandler<GetMyProductsByPageQuery, PagedResult<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetMyProductsByPageQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetMyProductsByPageQuery request, CancellationToken cancellationToken)
        {
            var countSpec = new ProductsFilterCountSpec(request.SearchTerm, request.CategoryId, request.IsAvailable);
            var totalCount = await _productRepository.CountAsync(countSpec, cancellationToken);

            var pagedSpec = new ProductsByPageSpec(request.SearchTerm, request.CategoryId, request.IsAvailable, request.PageNumber, request.PageSize);
            var productDtos = await _productRepository.ListAsync(pagedSpec, cancellationToken);

            return new PagedResult<ProductDto>(productDtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
