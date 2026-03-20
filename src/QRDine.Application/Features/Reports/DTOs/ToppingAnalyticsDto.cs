namespace QRDine.Application.Features.Reports.DTOs
{
    public class ToppingAnalyticsDto
    {
        public Guid ToppingId { get; set; }
        public string ToppingName { get; set; } = default!;
        public int NumberOfTimes { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
