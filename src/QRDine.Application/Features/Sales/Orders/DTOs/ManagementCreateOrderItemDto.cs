namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class ManagementCreateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
        public List<Guid>? SelectedToppingIds { get; set; }
    }
}
