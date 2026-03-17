namespace QRDine.Application.Features.Catalog.ToppingGroups.DTOs
{
    public class ToppingGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public int ToppingCount { get; set; }
        public int AppliedProductCount { get; set; }
    }
}
