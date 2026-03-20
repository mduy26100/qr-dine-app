using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Extensions;
using QRDine.Application.Features.Sales.Orders.Services;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetActiveOrderByTable
{
    public class GetActiveOrderByTableQueryHandler : IRequestHandler<GetActiveOrderByTableQuery, ManagementOrderDto?>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderFormattingService _formattingService;

        public GetActiveOrderByTableQueryHandler(
            IOrderRepository orderRepository,
            IOrderFormattingService formattingService)
        {
            _orderRepository = orderRepository;
            _formattingService = formattingService;
        }

        public async Task<ManagementOrderDto?> Handle(GetActiveOrderByTableQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetActiveOrderByTableSpec<ManagementOrderDto>(request.TableId, OrderExtensions.ToManagementOrderDto);

            var orderDto = await _orderRepository.FirstOrDefaultAsync(spec, cancellationToken);

            if (orderDto != null && orderDto.Items.Any())
            {
                foreach (var item in orderDto.Items)
                {
                    item.ToppingsSnapshot = _formattingService.FormatToppingSnapshot(item.ToppingsSnapshot);
                }
            }

            return orderDto;
        }
    }
}
