using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Application.Common.Constants;
using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Specifications;
using QRDine.Application.Features.Sales.Repositories;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Reports.Queries.GetProductPerformance
{
    public class GetProductPerformanceQueryHandler : IRequestHandler<GetProductPerformanceQuery, IEnumerable<ProductPerformanceDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICacheService _cacheService;

        public GetProductPerformanceQueryHandler(IOrderRepository orderRepository, ICacheService cacheService)
        {
            _orderRepository = orderRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ProductPerformanceDto>> Handle(
            GetProductPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.ProductPerformance(request.StartDate, request.EndDate, (int)request.SortBy, request.Top);
            var cached = await _cacheService.GetAsync<List<ProductPerformanceDto>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            var spec = new ProductPerformanceOrdersSpec(request.StartDate, request.EndDate);
            var orders = await _orderRepository.ListAsync(spec, cancellationToken);

            var products = orders
                .SelectMany(o => o.OrderItems)
                .Where(oi => !oi.IsDeleted && oi.Status != OrderItemStatus.Cancelled.ToString())
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

            var result = sorted.Take(request.Top).ToList();
            await _cacheService.SetAsync(cacheKey, result, CacheDurations.Reports, cancellationToken);
            return result;
        }
    }
}
