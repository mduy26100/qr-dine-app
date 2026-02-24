using QRDine.Domain.Common;
using QRDine.Domain.Tenant;

namespace QRDine.Domain.Catalog
{
    public class Category : BaseEntity, IMustHaveMerchant
    {
        public Guid MerchantId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public Guid? ParentId { get; set; }

        public Category? Parent { get; set; }
        public virtual Merchant Merchant { get; set; } = default!;
        public ICollection<Category> Children { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
