using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetActiveOrderByTable
{
    public class GetActiveOrderByTableQueryHandler : IRequestHandler<GetActiveOrderByTableQuery, ManagementOrderDto?>
    {
        private readonly IOrderRepository _orderRepository;

        public GetActiveOrderByTableQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ManagementOrderDto?> Handle(GetActiveOrderByTableQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetActiveOrderByTableSpec(request.TableId);

            var orderDto = await _orderRepository.SingleOrDefaultAsync(spec, cancellationToken);

            return orderDto;
        }
    }
}
