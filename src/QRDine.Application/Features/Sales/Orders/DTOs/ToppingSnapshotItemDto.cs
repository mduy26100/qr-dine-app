namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class ToppingSnapshotItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
    }
}
