namespace QRDine.Application.Features.Catalog.ToppingGroups.DTOs
{
    public class ToppingGroupDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public int MinSelections { get; set; }
        public int MaxSelections { get; set; }
        public bool IsActive { get; set; }

        public List<ToppingDto> Toppings { get; set; } = new();

        public List<Guid> AppliedProductIds { get; set; } = new();
    }
}
