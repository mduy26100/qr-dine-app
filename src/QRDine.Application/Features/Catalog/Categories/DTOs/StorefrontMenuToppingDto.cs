namespace QRDine.Application.Features.Catalog.Categories.DTOs
{
    public class StorefrontMenuToppingDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
    }
}
