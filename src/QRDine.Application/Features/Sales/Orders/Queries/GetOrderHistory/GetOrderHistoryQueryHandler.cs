using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetOrderHistory
{
    public class GetOrderHistoryQueryHandler : IRequestHandler<GetOrderHistoryQuery, List<OrderListDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderHistoryQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<OrderListDto>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
        {

            var spec = new OrderHistorySpec();
            var orders = await _orderRepository.ListAsync(spec, cancellationToken);

            return orders;
        }
    }
}
