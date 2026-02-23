namespace QRDine.Application.Features.Catalog.Categories.DTOs
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public Guid? ParentId { get; set; }
    }
}
