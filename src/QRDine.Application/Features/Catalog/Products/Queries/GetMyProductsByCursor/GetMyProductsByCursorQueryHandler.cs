using QRDine.Application.Common.Models;
using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Products.Queries.GetMyProductsByCursor
{
    public class GetMyProductsByCursorQueryHandler : IRequestHandler<GetMyProductsByCursorQuery, CursorPagedResult<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetMyProductsByCursorQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<CursorPagedResult<ProductDto>> Handle(GetMyProductsByCursorQuery request, CancellationToken cancellationToken)
        {
            var spec = new ProductsByCursorSpec(
                request.SearchTerm,
                request.CategoryId,
                request.IsAvailable,
                request.CursorCreatedAt,
                request.CursorId,
                request.Limit + 1);

            var productDtos = await _productRepository.ListAsync(spec, cancellationToken);

            bool hasNextPage = productDtos.Count > request.Limit;
            var data = hasNextPage ? productDtos.Take(request.Limit).ToList() : productDtos;

            DateTime? nextCreatedAt = null;
            Guid? nextId = null;

            if (data.Any())
            {
                var lastItem = data.Last();
                nextCreatedAt = lastItem.CreatedAt;
                nextId = lastItem.Id;
            }

            return new CursorPagedResult<ProductDto>(data, hasNextPage, nextCreatedAt, nextId);
        }
    }
}
