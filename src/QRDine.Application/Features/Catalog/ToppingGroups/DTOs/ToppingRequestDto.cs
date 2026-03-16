namespace QRDine.Application.Features.Catalog.ToppingGroups.DTOs
{
    public class ToppingRequestDto
    {
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
