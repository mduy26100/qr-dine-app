namespace QRDine.Application.Features.Catalog.Categories.DTOs
{
    public class StorefrontMenuCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public int DisplayOrder { get; set; }

        public List<StorefrontMenuProductDto> Products { get; set; } = new();
    }
}
