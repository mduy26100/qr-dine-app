using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetOrderDetail
{
    public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, ManagementOrderDto>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderDetailQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ManagementOrderDto> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        {
            var spec = new OrderDetailSpec(request.OrderId);

            var order = await _orderRepository.SingleOrDefaultAsync(spec, cancellationToken);

            if (order == null)
            {
                throw new NotFoundException($"Không tìm thấy đơn hàng với ID: {request.OrderId}");
            }

            return order;
        }
    }
}
