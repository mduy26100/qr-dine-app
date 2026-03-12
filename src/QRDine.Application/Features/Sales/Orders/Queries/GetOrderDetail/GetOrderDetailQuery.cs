using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetOrderDetail
{
    public record GetOrderDetailQuery(Guid OrderId) : IRequest<ManagementOrderDto>;
}
