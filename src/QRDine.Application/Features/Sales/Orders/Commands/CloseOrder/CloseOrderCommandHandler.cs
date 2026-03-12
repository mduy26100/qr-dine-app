using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Sales.Orders.Commands.CloseOrder
{
    public class CloseOrderCommandHandler : IRequestHandler<CloseOrderCommand, bool>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITableRepository _tableRepository;

        public CloseOrderCommandHandler(
            IOrderRepository orderRepository,
            ITableRepository tableRepository)
        {
            _orderRepository = orderRepository;
            _tableRepository = tableRepository;
        }

        public async Task<bool> Handle(CloseOrderCommand request, CancellationToken cancellationToken)
        {
            var spec = new OrderWithItemsSpec(request.OrderId);
            var order = await _orderRepository.SingleOrDefaultAsync(spec, cancellationToken);

            if (order == null)
                throw new NotFoundException("Đơn hàng không tồn tại hoặc không thuộc quyền quản lý của bạn.");

            if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Cancelled)
                throw new ConflictException("Đơn hàng này đã được đóng trước đó.");

            if (request.TargetStatus == OrderStatus.Paid)
            {
                var unservedItems = order.OrderItems
                    .Where(i => i.Status == OrderItemStatus.Pending || i.Status == OrderItemStatus.Preparing)
                    .ToList();

                foreach (var item in unservedItems)
                {
                    item.Status = OrderItemStatus.Cancelled;
                    order.TotalAmount -= item.Amount;
                }
            }
            else if (request.TargetStatus == OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems.Where(i => i.Status != OrderItemStatus.Cancelled))
                {
                    item.Status = OrderItemStatus.Cancelled;
                }
                order.TotalAmount = 0;
            }

            order.Status = request.TargetStatus;

            await _orderRepository.UpdateAsync(order, cancellationToken);

            var table = await _tableRepository.GetByIdAsync(order.TableId, cancellationToken);
            if (table != null && table.CurrentSessionId == order.SessionId)
            {
                table.IsOccupied = false;
                table.CurrentSessionId = null;
                await _tableRepository.UpdateAsync(table, cancellationToken);
            }

            return true;
        }
    }
}
