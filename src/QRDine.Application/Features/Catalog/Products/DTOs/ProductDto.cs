namespace QRDine.Application.Features.Catalog.Products.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
        public string? ParentCategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
