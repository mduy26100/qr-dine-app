using QRDine.Application.Common.Models;
using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Products.Queries.GetMyProducts
{
    public class GetMyProductsQueryHandler : IRequestHandler<GetMyProductsQuery, PagedResult<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetMyProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetMyProductsQuery request, CancellationToken cancellationToken)
        {
            var countSpec = new ProductsFilterCountSpec(request.SearchTerm, request.CategoryId, request.IsAvailable);
            var totalCount = await _productRepository.CountAsync(countSpec, cancellationToken);

            var pagedSpec = new ProductsFilterPagedSpec(
                request.SearchTerm,
                request.CategoryId,
                request.IsAvailable,
                request.PageNumber,
                request.PageSize,
                request.CursorCreatedAt,
                request.CursorId);

            var productDtos = await _productRepository.ListAsync(pagedSpec, cancellationToken);

            return new PagedResult<ProductDto>(productDtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
