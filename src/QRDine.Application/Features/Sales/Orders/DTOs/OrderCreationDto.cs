namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class OrderCreationDto
    {
        public Guid MerchantId { get; set; }
        public Guid TableId { get; set; }
        public Guid SessionId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? Note { get; set; }
        public List<OrderItemCreationDto> Items { get; set; } = new();
    }
}
