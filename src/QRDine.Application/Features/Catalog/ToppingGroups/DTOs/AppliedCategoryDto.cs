namespace QRDine.Application.Features.Catalog.ToppingGroups.DTOs
{
    public class AppliedCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public List<AppliedProductDto> Products { get; set; } = new();
    }
}
