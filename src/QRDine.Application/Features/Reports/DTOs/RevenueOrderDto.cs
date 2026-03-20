namespace QRDine.Application.Features.Reports.DTOs
{
    public class RevenueOrderDto
    {
        public Guid Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
