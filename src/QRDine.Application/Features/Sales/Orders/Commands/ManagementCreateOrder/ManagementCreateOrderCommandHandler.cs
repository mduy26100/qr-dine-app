using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Services;

namespace QRDine.Application.Features.Sales.Orders.Commands.ManagementCreateOrder
{
    public class ManagementCreateOrderCommandHandler : IRequestHandler<ManagementCreateOrderCommand, OrderResponseDto>
    {
        private readonly IOrderCreationService _orderCreationService;
        private readonly ITableRepository _tableRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public ManagementCreateOrderCommandHandler(
            IOrderCreationService orderCreationService,
            ITableRepository tableRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _orderCreationService = orderCreationService;
            _tableRepository = tableRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<OrderResponseDto> Handle(ManagementCreateOrderCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            if (!_currentUserService.MerchantId.HasValue)
                throw new ForbiddenException("Người dùng không thuộc chi nhánh/Merchant nào.");

            var orderModel = new OrderCreationDto
            {
                MerchantId = _currentUserService.MerchantId.Value,
                TableId = dto.TableId,
                SessionId = dto.SessionId,
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
