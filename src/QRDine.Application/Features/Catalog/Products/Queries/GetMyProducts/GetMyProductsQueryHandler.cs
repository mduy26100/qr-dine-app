using QRDine.Application.Common.Models;
using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Products.Queries.GetMyProducts
{
    public class GetMyProductsQueryHandler : IRequestHandler<GetMyProductsQuery, PagedResult<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetMyProductsQueryHandler(
            IProductRepository productRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
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

            var products = await _productRepository.ListAsync(pagedSpec, cancellationToken);

            var productDtos = _mapper.Map<List<ProductDto>>(products);

            return new PagedResult<ProductDto>(productDtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
