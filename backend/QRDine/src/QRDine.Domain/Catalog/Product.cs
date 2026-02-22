using QRDine.Domain.Common;
using QRDine.Domain.Tenant;

namespace QRDine.Domain.Catalog
{
    public class Product : BaseEntity
    {
        public Guid MerchantId { get; set; }
        public virtual Merchant Merchant { get; set; } = default!;

        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; } = default!;

        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
