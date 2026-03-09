namespace QRDine.Application.Features.Catalog.Products.DTOs
{
    public class ProductPriceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
    }
}
