using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Specifications;
using QRDine.Application.Features.Sales.Repositories;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Reports.Queries.GetRevenueChart
{
    public class GetRevenueChartQueryHandler : IRequestHandler<GetRevenueChartQuery, IEnumerable<RevenueChartItemDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetRevenueChartQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<RevenueChartItemDto>> Handle(
            GetRevenueChartQuery request,
            CancellationToken cancellationToken)
        {
            var spec = new RevenueOrdersSpec(request.StartDate, request.EndDate);
            var orders = await _orderRepository.ListAsync(spec, cancellationToken);

            var merchantOrders = orders
                .Where(o => o.Id != Guid.Empty && (o.Status == OrderStatus.Paid.ToString() || o.Status == OrderStatus.Open.ToString()))
                .ToList();

            var groupedData = GroupByTimeFrame(merchantOrders, request.Grouping, request.StartDate, request.EndDate);

            return groupedData.OrderBy(x => ExtractDateTime(x.Label, request.Grouping)).ToList();
        }

        private List<RevenueChartItemDto> GroupByTimeFrame(
            List<RevenueOrderDto> orders,
            RevenueChartGrouping grouping,
            DateTime startDate,
            DateTime endDate)
        {
            var result = new Dictionary<string, RevenueChartItemDto>();

            foreach (var order in orders)
            {
                var key = GetGroupKey(order.CreatedAt, grouping);
                if (key == null) continue;

                if (!result.ContainsKey(key))
                {
                    result[key] = new RevenueChartItemDto
                    {
                        Label = key,
                        Revenue = 0,
                        OrderCount = 0
                    };
                }

                result[key].Revenue += order.TotalAmount;
                result[key].OrderCount += 1;
            }

            FillMissingGaps(result, grouping, startDate, endDate);

            return result.Values.ToList();
        }

        private string? GetGroupKey(DateTime createdAt, RevenueChartGrouping grouping)
        {
            return grouping switch
            {
                RevenueChartGrouping.ByHour => new DateTime(createdAt.Year, createdAt.Month, createdAt.Day, createdAt.Hour, 0, 0).ToString("HH:mm dd/MM/yyyy"),
                RevenueChartGrouping.ByDay => new DateTime(createdAt.Year, createdAt.Month, createdAt.Day).ToString("dd/MM/yyyy"),
                RevenueChartGrouping.ByMonth => createdAt.ToString("MM/yyyy"),
                _ => null
            };
        }

        private void FillMissingGaps(
            Dictionary<string, RevenueChartItemDto> data,
            RevenueChartGrouping grouping,
            DateTime startDate,
            DateTime endDate)
        {
            var current = grouping switch
            {
                RevenueChartGrouping.ByHour => new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0),
                RevenueChartGrouping.ByDay => new DateTime(startDate.Year, startDate.Month, startDate.Day),
                RevenueChartGrouping.ByMonth => new DateTime(startDate.Year, startDate.Month, 1),
                _ => startDate
            };

            while (current <= endDate)
            {
                var label = grouping switch
                {
                    RevenueChartGrouping.ByHour => current.ToString("HH:mm dd/MM/yyyy"),
                    RevenueChartGrouping.ByDay => current.ToString("dd/MM/yyyy"),
                    RevenueChartGrouping.ByMonth => current.ToString("MM/yyyy"),
                    _ => null
                };

                if (label != null && !data.ContainsKey(label))
                {
                    data[label] = new RevenueChartItemDto
                    {
                        Label = label,
                        Revenue = 0,
                        OrderCount = 0
                    };
                }

                current = grouping switch
                {
                    RevenueChartGrouping.ByHour => current.AddHours(1),
                    RevenueChartGrouping.ByDay => current.AddDays(1),
                    RevenueChartGrouping.ByMonth => current.AddMonths(1),
                    _ => current.AddDays(1)
                };
            }
        }

        private DateTime ExtractDateTime(string label, RevenueChartGrouping grouping)
        {
            return grouping switch
            {
                RevenueChartGrouping.ByHour => DateTime.ParseExact(label, "HH:mm dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                RevenueChartGrouping.ByDay => DateTime.ParseExact(label, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                RevenueChartGrouping.ByMonth => DateTime.ParseExact(label, "MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                _ => DateTime.MinValue
            };
        }
    }
}
