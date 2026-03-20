namespace QRDine.Application.Features.Reports.DTOs
{
    public class ProductPerformanceDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
