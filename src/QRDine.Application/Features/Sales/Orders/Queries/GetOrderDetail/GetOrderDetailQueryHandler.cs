using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Services;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetOrderDetail
{
    public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, ManagementOrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderFormattingService _formattingService;

        public GetOrderDetailQueryHandler(
            IOrderRepository orderRepository,
            IOrderFormattingService formattingService)
        {
            _orderRepository = orderRepository;
            _formattingService = formattingService;
        }

        public async Task<ManagementOrderDto> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        {
            var spec = new OrderDetailSpec(request.OrderId);

            var order = await _orderRepository.SingleOrDefaultAsync(spec, cancellationToken);

            if (order == null)
            {
                throw new NotFoundException($"Không tìm thấy đơn hàng với ID: {request.OrderId}");
            }

            if (order.Items != null && order.Items.Any())
            {
                foreach (var item in order.Items)
                {
                    item.ToppingsSnapshot = _formattingService.FormatToppingSnapshot(item.ToppingsSnapshot);
                }
            }

            return order;
        }
    }
}
