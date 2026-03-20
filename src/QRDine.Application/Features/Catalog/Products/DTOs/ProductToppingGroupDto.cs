namespace QRDine.Application.Features.Catalog.Products.DTOs
{
    public class ProductToppingGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public bool IsRequired { get; set; }
        public int MinSelections { get; set; }
        public int MaxSelections { get; set; }
        public bool IsActive { get; set; }

        public List<ProductToppingDto> Toppings { get; set; } = new List<ProductToppingDto>();
    }
}
