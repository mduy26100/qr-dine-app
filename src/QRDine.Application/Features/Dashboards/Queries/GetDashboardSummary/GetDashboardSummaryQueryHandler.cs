using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Dashboards.DTOs;
using QRDine.Application.Features.Dashboards.Specifications;
using QRDine.Application.Features.Sales.Repositories;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Dashboards.Queries.GetDashboardSummary
{
    public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;

        public GetDashboardSummaryQueryHandler(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService,
            IIdentityService identityService)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId
                ?? throw new UnauthorizedAccessException("Bạn không có quyền xem thông tin này.");

            // 1. Tính toán mốc thời gian (UTC)
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            var startOfLast7Days = now.Date.AddDays(-6);
            var endOfToday = now.Date.AddDays(1).AddTicks(-1);

            // 2. Chạy LẦN LƯỢT từng query để đảm bảo an toàn cho DbContext (Thread-safe)
            var totalProducts = await _productRepository.CountAsync(new ActiveProductsByMerchantSpec(merchantId), cancellationToken);
            var totalStaff = await _identityService.CountStaffByMerchantAsync(merchantId, cancellationToken);
            var monthlyOrders = await _orderRepository.ListAsync(new MonthlyOrdersSummarySpec(merchantId, startOfMonth, endOfMonth), cancellationToken);
            var rawChartData = await _orderRepository.ListAsync(new Last7DaysRevenueSpec(merchantId, startOfLast7Days, endOfToday), cancellationToken);

            // 3. Xử lý logic tổng quan tháng
            var revenueThisMonth = monthlyOrders
                .Where(o => o.Status == OrderStatus.Paid)
                .Sum(o => o.TotalAmount);

            // 4. Xử lý Logic Chart (Khởi tạo 7 ngày = 0đ, sau đó lấp data thật vào)
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
                    g => g.Sum(x => x.TotalAmount)
                );

            foreach (var day in revenueChart)
            {
                if (groupedData.TryGetValue(day.Date, out var total))
                {
                    day.Revenue = total;
                }
            }

            // 5. Trả kết quả
            return new DashboardSummaryDto
            {
                TotalProducts = totalProducts,
                TotalStaff = totalStaff,
                TotalOrdersThisMonth = monthlyOrders.Count,
                RevenueThisMonth = revenueThisMonth,
                RevenueChart = revenueChart
            };
        }
    }
}
