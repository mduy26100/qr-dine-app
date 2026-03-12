using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Sales.OrderItems.Commands.UpdateOrderItemsStatus
{
    public record UpdateOrderItemsStatusCommand(List<Guid> OrderItemIds, OrderItemStatus TargetStatus) : IRequest<bool>;
}
