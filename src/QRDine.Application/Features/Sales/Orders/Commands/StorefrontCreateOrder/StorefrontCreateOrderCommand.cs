using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Commands.StorefrontCreateOrder
{
    public record StorefrontCreateOrderCommand(Guid MerchantId, StorefrontCreateOrderDto Dto) : IRequest<OrderResponseDto>;
}
