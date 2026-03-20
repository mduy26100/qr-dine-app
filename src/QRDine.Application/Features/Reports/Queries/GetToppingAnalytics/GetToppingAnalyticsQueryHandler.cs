using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Specifications;
using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Reports.Queries.GetToppingAnalytics
{
    public class GetToppingAnalyticsQueryHandler : IRequestHandler<GetToppingAnalyticsQuery, IEnumerable<ToppingAnalyticsDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetToppingAnalyticsQueryHandler(
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<ToppingAnalyticsDto>> Handle(
            GetToppingAnalyticsQuery request,
            CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId;
            if (!merchantId.HasValue)
            {
                return Enumerable.Empty<ToppingAnalyticsDto>();
            }

            var spec = new ToppingAnalyticsOrdersSpec(merchantId.Value, request.StartDate, request.EndDate);
            var orders = await _orderRepository.ListAsync(spec, cancellationToken);

            var toppingData = new Dictionary<(Guid Id, string Name), (int Count, decimal Revenue)>();

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    if (string.IsNullOrEmpty(item.ToppingsSnapshot))
                        continue;

                    var toppings = JsonSerializer.Deserialize<List<ToppingSnapshotItemDto>>(item.ToppingsSnapshot);
                    if (toppings == null || !toppings.Any())
                        continue;

                    foreach (var topping in toppings)
                    {
                        var key = (topping.Id, topping.Name);
                        var revenue = item.Quantity * topping.Price;

                        if (toppingData.ContainsKey(key))
                        {
                            var current = toppingData[key];
                            toppingData[key] = (current.Count + 1, current.Revenue + revenue);
                        }
                        else
                        {
                            toppingData[key] = (1, revenue);
                        }
                    }
                }
            }

            var result = toppingData
                .Select(kvp => new ToppingAnalyticsDto
                {
                    ToppingId = kvp.Key.Id,
                    ToppingName = kvp.Key.Name,
                    NumberOfTimes = kvp.Value.Count,
                    TotalRevenue = kvp.Value.Revenue
                })
                .OrderByDescending(t => t.TotalRevenue)
                .ToList();

            return result;
        }
    }
}
