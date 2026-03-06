namespace QRDine.Application.Features.Catalog.Categories.DTOs
{
    public class StorefrontMenuProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
    }
}
