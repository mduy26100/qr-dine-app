namespace QRDine.Application.Features.Catalog.Categories.DTOs
{
    public class CategoryLookupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public int DisplayOrder { get; set; }
        public List<ProductLookupDto> Products { get; set; } = new();
    }
}
