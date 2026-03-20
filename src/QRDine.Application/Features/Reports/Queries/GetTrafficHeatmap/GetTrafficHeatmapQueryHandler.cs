using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Application.Common.Constants;
using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Reports.Queries.GetTrafficHeatmap
{
    public class GetTrafficHeatmapQueryHandler : IRequestHandler<GetTrafficHeatmapQuery, IEnumerable<TrafficHeatmapDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICacheService _cacheService;

        public GetTrafficHeatmapQueryHandler(IOrderRepository orderRepository, ICacheService cacheService)
        {
            _orderRepository = orderRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<TrafficHeatmapDto>> Handle(
            GetTrafficHeatmapQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.TrafficHeatmap(request.StartDate, request.EndDate);
            var cached = await _cacheService.GetAsync<List<TrafficHeatmapDto>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            var spec = new TrafficHeatmapOrdersSpec(request.StartDate, request.EndDate);
            var orders = await _orderRepository.ListAsync(spec, cancellationToken);

            var heatmap = orders
                .GroupBy(o => new { DayOfWeek = o.CreatedAt.DayOfWeek, Hour = o.CreatedAt.Hour })
                .Select(g => new TrafficHeatmapDto
                {
                    DayOfWeek = g.Key.DayOfWeek.ToString(),
                    Hour = g.Key.Hour,
                    OrderCount = g.Count()
                })
                .OrderBy(h => GetDayOfWeekOrder(h.DayOfWeek))
                .ThenBy(h => h.Hour)
                .ToList();

            await _cacheService.SetAsync(cacheKey, heatmap, CacheDurations.Reports, cancellationToken);
            return heatmap;
        }

        private static int GetDayOfWeekOrder(string dayOfWeek)
        {
            return dayOfWeek switch
            {
                "Monday" => 1,
                "Tuesday" => 2,
                "Wednesday" => 3,
                "Thursday" => 4,
                "Friday" => 5,
                "Saturday" => 6,
                "Sunday" => 7,
                _ => 8
            };
        }
    }
}
