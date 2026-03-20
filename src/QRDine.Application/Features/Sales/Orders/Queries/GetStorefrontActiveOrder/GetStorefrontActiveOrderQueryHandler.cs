using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Extensions;
using QRDine.Application.Features.Sales.Orders.Services;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetStorefrontActiveOrder
{
    public class GetStorefrontActiveOrderQueryHandler : IRequestHandler<GetStorefrontActiveOrderQuery, StorefrontOrderDto?>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderFormattingService _formattingService;

        public GetStorefrontActiveOrderQueryHandler(
            IOrderRepository orderRepository,
            IOrderFormattingService formattingService)
        {
            _orderRepository = orderRepository;
            _formattingService = formattingService;
        }

        public async Task<StorefrontOrderDto?> Handle(GetStorefrontActiveOrderQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetActiveOrderByTableSpec<StorefrontOrderDto>(request.TableId, OrderExtensions.ToStorefrontOrderDto, request.MerchantId, request.SessionId);

            var orderDto = await _orderRepository.SingleOrDefaultAsync(spec, cancellationToken);

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
