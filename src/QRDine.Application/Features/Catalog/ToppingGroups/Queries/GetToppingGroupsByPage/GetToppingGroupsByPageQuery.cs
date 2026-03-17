using QRDine.Application.Common.Models;
using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Queries.GetToppingGroupsByPage
{
    public class GetToppingGroupsByPageQuery : PaginationRequest, IRequest<PagedResult<ToppingGroupDto>>
    {
        public string? Keyword { get; set; }
    }
}
