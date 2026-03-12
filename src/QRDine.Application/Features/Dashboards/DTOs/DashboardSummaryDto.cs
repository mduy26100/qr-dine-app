namespace QRDine.Application.Features.Dashboards.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalProducts { get; set; }
        public int TotalStaff { get; set; }
        public int TotalOrdersThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }

        public List<ChartDataDto> RevenueChart { get; set; } = new();
    }
}
