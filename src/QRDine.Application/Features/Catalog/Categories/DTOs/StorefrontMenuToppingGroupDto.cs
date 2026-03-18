namespace QRDine.Application.Features.Catalog.Categories.DTOs
{
    public class StorefrontMenuToppingGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public int MinSelections { get; set; }
        public int MaxSelections { get; set; }

        public List<StorefrontMenuToppingDto> Toppings { get; set; } = new();
    }
}
