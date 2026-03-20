using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Specifications;
using QRDine.Application.Features.Sales.Repositories;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Reports.Queries.GetProductPerformance
{
    public class GetProductPerformanceQueryHandler : IRequestHandler<GetProductPerformanceQuery, IEnumerable<ProductPerformanceDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProductPerformanceQueryHandler(
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<ProductPerformanceDto>> Handle(
            GetProductPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId;
            if (!merchantId.HasValue)
            {
                return Enumerable.Empty<ProductPerformanceDto>();
            }

            var spec = new ProductPerformanceOrdersSpec(merchantId.Value, request.StartDate, request.EndDate);
            var orders = await _orderRepository.ListAsync(spec, cancellationToken);

            var products = orders
                .SelectMany(o => o.OrderItems)
                .Where(oi => !oi.IsDeleted && oi.Status != OrderItemStatus.Cancelled)
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new ProductPerformanceDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Amount)
                })
                .ToList();

            var sorted = request.SortBy switch
            {
                ProductPerformanceSortBy.Revenue => products.OrderByDescending(p => p.TotalRevenue),
                ProductPerformanceSortBy.Volume => products.OrderByDescending(p => p.QuantitySold),
                _ => products.OrderByDescending(p => p.TotalRevenue)
            };

            return sorted.Take(request.Top).ToList();
        }
    }
}
