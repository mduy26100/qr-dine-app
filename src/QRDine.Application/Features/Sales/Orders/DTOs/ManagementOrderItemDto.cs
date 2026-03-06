namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class ManagementOrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public string? ToppingsSnapshot { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
