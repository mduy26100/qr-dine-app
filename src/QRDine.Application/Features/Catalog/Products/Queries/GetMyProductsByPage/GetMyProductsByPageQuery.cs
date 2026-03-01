using QRDine.Application.Common.Models;
using QRDine.Application.Features.Catalog.Products.DTOs;

namespace QRDine.Application.Features.Catalog.Products.Queries.GetMyProductsByPage
{
    public class GetMyProductsByPageQuery : PaginationRequest, IRequest<PagedResult<ProductDto>>
    {
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
