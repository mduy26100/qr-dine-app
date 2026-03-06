using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Commands.ManagementCreateOrder
{
    public record ManagementCreateOrderCommand(ManagementCreateOrderDto Dto) : IRequest<OrderResponseDto>;
}
