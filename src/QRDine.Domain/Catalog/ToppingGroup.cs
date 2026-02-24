using QRDine.Domain.Common;
using QRDine.Domain.Tenant;

namespace QRDine.Domain.Catalog
{
    public class ToppingGroup : BaseEntity, IMustHaveMerchant
    {
        public Guid MerchantId { get; set; }
        public virtual Merchant Merchant { get; set; } = default!;

        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public bool IsRequired { get; set; } = false;
        public int MinSelections { get; set; } = 0;
        public int MaxSelections { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Topping> Toppings { get; set; } = new List<Topping>();
        public virtual ICollection<ProductToppingGroup> ProductToppingGroups { get; set; } = new List<ProductToppingGroup>();
    }
}
