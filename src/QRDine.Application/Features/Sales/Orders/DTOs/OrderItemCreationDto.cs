namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class OrderItemCreationDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? ToppingsSnapshot { get; set; }
        public decimal ToppingSurcharge { get; set; }
        public string? Note { get; set; }
    }
}
