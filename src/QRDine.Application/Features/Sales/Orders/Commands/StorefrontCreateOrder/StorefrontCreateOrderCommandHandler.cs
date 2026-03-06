using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Services;

namespace QRDine.Application.Features.Sales.Orders.Commands.StorefrontCreateOrder
{
    public class StorefrontCreateOrderCommandHandler : IRequestHandler<StorefrontCreateOrderCommand, OrderResponseDto>
    {
        private readonly IOrderCreationService _orderCreationService;
        private readonly IMapper _mapper;

        public StorefrontCreateOrderCommandHandler(
            IOrderCreationService orderCreationService,
            IMapper mapper)
        {
            _orderCreationService = orderCreationService;
            _mapper = mapper;
        }

        public async Task<OrderResponseDto> Handle(StorefrontCreateOrderCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var orderModel = new OrderCreationDto
            {
                MerchantId = request.MerchantId,
                TableId = dto.TableId,
                SessionId = dto.SessionId,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                Note = dto.Note,
                Items = dto.Items.Select(i => new OrderItemCreationDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    ToppingsSnapshot = i.ToppingsSnapshot,
                    ToppingSurcharge = i.ToppingSurcharge,
                    Note = i.Note
                }).ToList()
            };

            var order = await _orderCreationService.CreateOrAppendOrderAsync(orderModel, cancellationToken);

            return _mapper.Map<OrderResponseDto>(order);
        }
    }
}
