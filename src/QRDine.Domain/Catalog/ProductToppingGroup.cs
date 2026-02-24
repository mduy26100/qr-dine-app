using QRDine.Domain.Common;

namespace QRDine.Domain.Catalog
{
    public class ProductToppingGroup : BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;

        public Guid ToppingGroupId { get; set; }
        public virtual ToppingGroup ToppingGroup { get; set; } = default!;
    }
}
