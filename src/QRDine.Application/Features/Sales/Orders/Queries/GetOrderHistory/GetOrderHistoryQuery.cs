using QRDine.Application.Common.Models;
using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetOrderHistory
{
    public class GetOrderHistoryQuery : PaginationRequest, IRequest<PagedResult<OrderListDto>>
    {
        public string? SearchTerm { get; set; }
    }
}
