namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class OrderListDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = default!;
        public string TableName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
