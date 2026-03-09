namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = default!;
        public string TableName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid SessionId { get; set; }
    }
}
