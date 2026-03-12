using QRDine.Application.Features.Sales.OrderItems.Specifications;
using QRDine.Application.Features.Sales.Repositories;
using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.OrderItems.Commands.UpdateOrderItemsStatus
{
    public class UpdateOrderItemsStatusCommandHandler : IRequestHandler<UpdateOrderItemsStatusCommand, bool>
    {
        private readonly IOrderItemRepository _orderItemRepository;

        public UpdateOrderItemsStatusCommandHandler(IOrderItemRepository orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
        }

        public async Task<bool> Handle(UpdateOrderItemsStatusCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderItemIds == null || !request.OrderItemIds.Any())
                return false;

            var spec = new OrderItemsByIdsSpec(request.OrderItemIds);
            var items = await _orderItemRepository.ListAsync(spec, cancellationToken);

            if (!items.Any()) return false;

            var validItemsToUpdate = new List<OrderItem>();

            foreach (var item in items)
            {
                if (item.Order.Status != OrderStatus.Open)
                    continue;

                if (item.Status == request.TargetStatus)
                    continue;

                if (item.Status == OrderItemStatus.Served || item.Status == OrderItemStatus.Cancelled)
                    continue;

                bool isValidTransition = false;

                if (item.Status == OrderItemStatus.Pending)
                {
                    isValidTransition = request.TargetStatus == OrderItemStatus.Preparing ||
                                        request.TargetStatus == OrderItemStatus.Served ||
                                        request.TargetStatus == OrderItemStatus.Cancelled;
                }
                else if (item.Status == OrderItemStatus.Preparing)
                {
                    isValidTransition = request.TargetStatus == OrderItemStatus.Served ||
                                        request.TargetStatus == OrderItemStatus.Cancelled;
                }

                if (isValidTransition)
                {
                    if (request.TargetStatus == OrderItemStatus.Cancelled)
                    {
                        item.Order.TotalAmount -= item.Amount;
                    }

                    item.Status = request.TargetStatus;
                    validItemsToUpdate.Add(item);
                }
            }

            if (validItemsToUpdate.Any())
            {
                await _orderItemRepository.UpdateRangeAsync(validItemsToUpdate, cancellationToken);
            }

            return true;
        }
    }
}
