namespace QRDine.Application.Features.Reports.DTOs
{
    public class OrderWithItemsForToppingDto
    {
        public List<OrderItemForToppingDto> OrderItems { get; set; } = new();
    }
}
