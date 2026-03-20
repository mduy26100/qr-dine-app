using QRDine.Application.Common.Abstractions.Notifications;
using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Services;

namespace QRDine.Application.Features.Sales.Orders.Commands.StorefrontCreateOrder
{
    public class StorefrontCreateOrderCommandHandler : IRequestHandler<StorefrontCreateOrderCommand, OrderResponseDto>
    {
        private readonly IOrderCreationService _orderCreationService;
        private readonly IOrderNotificationService _notificationService;
        private readonly IMapper _mapper;

        public StorefrontCreateOrderCommandHandler(
            IOrderCreationService orderCreationService,
            IOrderNotificationService notificationService,
            IMapper mapper)
        {
            _orderCreationService = orderCreationService;
            _notificationService = notificationService;
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
                    SelectedToppingIds = i.SelectedToppingIds ?? new List<Guid>(),
                    Note = i.Note
                }).ToList()
            };

            var order = await _orderCreationService.CreateOrAppendOrderAsync(orderModel, cancellationToken);

            _ = _notificationService.NotifyOrderUpdatedAsync(
                request.MerchantId,
                request.Dto.TableId,
                order.TableName,
                CancellationToken.None);

            return _mapper.Map<OrderResponseDto>(order);
        }
    }
}
