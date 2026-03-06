namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class ManagementCreateOrderDto
    {
        public Guid TableId { get; set; }

        public Guid? SessionId { get; set; }

        public string? Note { get; set; }

        public List<ManagementCreateOrderItemDto> Items { get; set; } = new();
    }
}
