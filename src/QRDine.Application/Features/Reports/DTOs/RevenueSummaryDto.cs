namespace QRDine.Application.Features.Reports.DTOs
{
    public class RevenueSummaryDto
    {
        public TrendValueDto TotalRevenue { get; set; } = new();
        public TrendValueDto TotalOrders { get; set; } = new();
        public TrendValueDto AverageOrderValue { get; set; } = new();
        public TrendValueDto CancelRate { get; set; } = new();
    }
}
