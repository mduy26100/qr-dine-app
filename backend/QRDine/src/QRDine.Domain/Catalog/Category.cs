using QRDine.Domain.Common;
using QRDine.Domain.Tenant;

namespace QRDine.Domain.Catalog
{
    public class Category : BaseEntity, IMustHaveMerchant
    {
        public Guid MerchantId { get; set; }
        public virtual Merchant Merchant { get; set; } = default!;

        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
