namespace QRDine.Application.Features.Catalog.Categories.DTOs
{
    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public Guid? ParentId { get; set; }
    }
}
