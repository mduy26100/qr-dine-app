using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetStorefrontOrder
{
    public class GetStorefrontOrderQueryHandler : IRequestHandler<GetStorefrontOrderQuery, StorefrontOrderDto?>
    {
        private readonly IOrderRepository _orderRepository;

        public GetStorefrontOrderQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<StorefrontOrderDto?> Handle(GetStorefrontOrderQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetActiveOrderBySessionWithProductsSpec(request.MerchantId, request.SessionId);

            var orderDto = await _orderRepository.SingleOrDefaultAsync(spec, cancellationToken);

            return orderDto;
        }
    }
}
