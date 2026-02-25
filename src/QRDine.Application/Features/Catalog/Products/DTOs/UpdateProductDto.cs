namespace QRDine.Application.Features.Catalog.Products.DTOs
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        public Guid CategoryId { get; set; }

        public Stream? ImgContent { get; set; }
        public string? ImgFileName { get; set; }
        public string? ImgContentType { get; set; }
    }
}
