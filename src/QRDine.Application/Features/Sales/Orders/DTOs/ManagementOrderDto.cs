namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class ManagementOrderDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = default!;
        public string TableName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<ManagementOrderItemDto> Items { get; set; } = new();
    }
}
