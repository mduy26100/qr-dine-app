namespace QRDine.Application.Features.Catalog.Products.DTOs
{
    public class ProductToppingDto
    {
        public Guid Id { get; set; }
        public Guid ToppingGroupId { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
