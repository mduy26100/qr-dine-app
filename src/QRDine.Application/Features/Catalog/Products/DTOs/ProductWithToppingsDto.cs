namespace QRDine.Application.Features.Catalog.Products.DTOs
{
    public class ProductWithToppingsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        public List<ProductToppingGroupDto> ToppingGroups { get; set; } = new List<ProductToppingGroupDto>();
    }
}
