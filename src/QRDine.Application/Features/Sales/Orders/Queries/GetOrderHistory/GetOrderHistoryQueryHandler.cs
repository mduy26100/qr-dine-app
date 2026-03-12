using QRDine.Application.Common.Models;
using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetOrderHistory
{
    public class GetOrderHistoryQueryHandler : IRequestHandler<GetOrderHistoryQuery, PagedResult<OrderListDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderHistoryQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<PagedResult<OrderListDto>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
        {
            var countSpec = new OrderHistoryCountSpec(request.SearchTerm);
            var totalCount = await _orderRepository.CountAsync(countSpec, cancellationToken);

            var pagedSpec = new OrderHistoryByPageSpec(request.SearchTerm, request.PageNumber, request.PageSize);
            var orders = await _orderRepository.ListAsync(pagedSpec, cancellationToken);

            return new PagedResult<OrderListDto>(orders, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
