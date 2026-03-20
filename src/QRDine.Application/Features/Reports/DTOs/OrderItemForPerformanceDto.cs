namespace QRDine.Application.Features.Reports.DTOs
{
    public class OrderItemForPerformanceDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = default!;
        public bool IsDeleted { get; set; }
    }
}
