namespace QRDine.Application.Features.Reports.DTOs
{
    public class OrderWithItemsForPerformanceDto
    {
        public List<OrderItemForPerformanceDto> OrderItems { get; set; } = new();
    }
}
