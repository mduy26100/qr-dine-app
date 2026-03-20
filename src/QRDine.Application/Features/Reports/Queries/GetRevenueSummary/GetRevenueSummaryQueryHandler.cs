using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Application.Common.Constants;
using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Specifications;
using QRDine.Application.Features.Sales.Repositories;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Reports.Queries.GetRevenueSummary
{
    public class GetRevenueSummaryQueryHandler : IRequestHandler<GetRevenueSummaryQuery, RevenueSummaryDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICacheService _cacheService;

        public GetRevenueSummaryQueryHandler(IOrderRepository orderRepository, ICacheService cacheService)
        {
            _orderRepository = orderRepository;
            _cacheService = cacheService;
        }

        public async Task<RevenueSummaryDto> Handle(GetRevenueSummaryQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.RevenueSummary(request.StartDate, request.EndDate);
            var cached = await _cacheService.GetAsync<RevenueSummaryDto>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return cached;
            }
            var currentDuration = request.EndDate - request.StartDate;
            var previousStartDate = request.StartDate.Subtract(currentDuration);
            var previousEndDate = request.StartDate;

            var currentSpec = new RevenueOrdersSpec(request.StartDate, request.EndDate);
            var currentOrders = await _orderRepository.ListAsync(currentSpec, cancellationToken);

            var previousSpec = new RevenueOrdersSpec(previousStartDate, previousEndDate);
            var previousOrders = await _orderRepository.ListAsync(previousSpec, cancellationToken);

            var currentPaidOrServed = currentOrders.Where(o => o.Status == OrderStatus.Paid.ToString() || o.Status == OrderStatus.Open.ToString()).ToList();
            var currentTotalRevenue = currentPaidOrServed.Sum(o => o.TotalAmount);
            var currentTotalOrdersCount = currentPaidOrServed.Count;
            var currentCancelCount = currentOrders.Count(o => o.Status == OrderStatus.Cancelled.ToString());

            var previousPaidOrServed = previousOrders.Where(o => o.Status == OrderStatus.Paid.ToString() || o.Status == OrderStatus.Open.ToString()).ToList();
            var prevTotalRevenue = previousPaidOrServed.Sum(o => o.TotalAmount);
            var prevTotalOrdersCount = previousPaidOrServed.Count;
            var prevCancelCount = previousOrders.Count(o => o.Status == OrderStatus.Cancelled.ToString());

            var result = new RevenueSummaryDto
            {
                TotalRevenue = new TrendValueDto
                {
                    Value = currentTotalRevenue,
                    Trend = CalculateTrend(currentTotalRevenue, prevTotalRevenue)
                },
                TotalOrders = new TrendValueDto
                {
                    Value = currentTotalOrdersCount,
                    Trend = CalculateTrend(currentTotalOrdersCount, prevTotalOrdersCount)
                },
                AverageOrderValue = new TrendValueDto
                {
                    Value = currentTotalOrdersCount > 0 ? Math.Round(currentTotalRevenue / currentTotalOrdersCount, 0) : 0,
                    Trend = CalculateTrend(
                        currentTotalOrdersCount > 0 ? currentTotalRevenue / currentTotalOrdersCount : 0,
                        prevTotalOrdersCount > 0 ? prevTotalRevenue / prevTotalOrdersCount : 0)
                },
                CancelRate = new TrendValueDto
                {
                    Value = currentOrders.Count > 0 ? Math.Round((decimal)currentCancelCount / currentOrders.Count * 100, 2) : 0,
                    Trend = CalculateTrend(
                        currentOrders.Count > 0 ? (decimal)currentCancelCount / currentOrders.Count : 0,
                        previousOrders.Count > 0 ? (decimal)prevCancelCount / previousOrders.Count : 0)
                }
            };
            
            await _cacheService.SetAsync(cacheKey, result, CacheDurations.Reports, cancellationToken);
            return result;
        }

        private decimal CalculateTrend(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0;
            }

            var trend = ((current - previous) / previous) * 100;
            return Math.Round(trend, 1);
        }
    }
}
