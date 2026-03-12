using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetOrderHistory
{
    public record GetOrderHistoryQuery : IRequest<List<OrderListDto>>;
}
