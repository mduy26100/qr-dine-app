using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Sales.Orders.Commands.CloseOrder
{
    public record CloseOrderCommand(Guid OrderId, OrderStatus TargetStatus) : IRequest<bool>;
}
