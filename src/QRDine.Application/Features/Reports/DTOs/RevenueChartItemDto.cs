namespace QRDine.Application.Features.Reports.DTOs
{
    public class RevenueChartItemDto
    {
        public string Label { get; set; } = default!;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}
