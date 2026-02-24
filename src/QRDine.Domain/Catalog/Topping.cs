using QRDine.Domain.Common;

namespace QRDine.Domain.Catalog
{
    public class Topping : BaseEntity
    {
        public Guid ToppingGroupId { get; set; }
        public virtual ToppingGroup ToppingGroup { get; set; } = default!;

        public string Name { get; set; } = default!;
        public decimal Price { get; set; } = 0; 

        public int DisplayOrder { get; set; } = 0;
        public bool IsAvailable { get; set; } = true;
    }
}
