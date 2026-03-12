using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Dashboards.DTOs;
using QRDine.Application.Features.Dashboards.Specifications;
using QRDine.Application.Features.Sales.Repositories;

namespace QRDine.Application.Features.Dashboards.Queries.GetDashboardSummary
{
    public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;

        public GetDashboardSummaryQueryHandler(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            ICurrentUserService currentUserService,
            IIdentityService identityService)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId
                ?? throw new UnauthorizedAccessException("Bạn không có quyền xem thông tin này.");

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            var startOfLast7Days = now.Date.AddDays(-6);
            var endOfToday = now.Date.AddDays(1).AddTicks(-1);

            var totalProducts = await _productRepository.CountAsync(new ActiveProductsSpec(), cancellationToken);

            var totalStaff = await _identityService.CountStaffByMerchantAsync(merchantId, cancellationToken);

            var totalOrdersThisMonth = await _orderRepository.CountAsync(new OrdersByDateRangeSpec(startOfMonth, endOfMonth), cancellationToken);

            var monthlyServedItems = await _orderItemRepository.ListAsync(new ServedOrderItemsByDateRangeSpec(startOfMonth, endOfMonth), cancellationToken);
            var revenueThisMonth = monthlyServedItems.Sum(oi => oi.Amount);

            var rawChartData = await _orderItemRepository.ListAsync(new ServedOrderItemsByDateRangeSpec(startOfLast7Days, endOfToday), cancellationToken);

            var revenueChart = Enumerable.Range(0, 7)
                .Select(i => new ChartDataDto
                {
                    Date = startOfLast7Days.AddDays(i).ToString("dd/MM"),
                    Revenue = 0
                })
                .ToList();

            var groupedData = rawChartData
                .GroupBy(x => x.CreatedAt.Date)
                .ToDictionary(
                    g => g.Key.ToString("dd/MM"),
                    g => g.Sum(x => x.Amount)
                );

            foreach (var day in revenueChart)
            {
                if (groupedData.TryGetValue(day.Date, out var total))
                {
                    day.Revenue = total;
                }
            }

            return new DashboardSummaryDto
            {
                TotalProducts = totalProducts,
                TotalStaff = totalStaff,
                TotalOrdersThisMonth = totalOrdersThisMonth,
                RevenueThisMonth = revenueThisMonth,
                RevenueChart = revenueChart
            };
        }
    }
}
