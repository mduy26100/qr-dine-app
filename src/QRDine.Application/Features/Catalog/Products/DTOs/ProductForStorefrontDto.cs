namespace QRDine.Application.Features.Catalog.Products.DTOs
{
    public class ProductForStorefrontDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; }
    }
}
