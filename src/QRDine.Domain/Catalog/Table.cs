using QRDine.Domain.Common;
using QRDine.Domain.Tenant;

namespace QRDine.Domain.Catalog
{
    public class Table : BaseEntity, IMustHaveMerchant
    {
        public Guid MerchantId { get; set; }
        public virtual Merchant Merchant { get; set; } = default!;

        public string Name { get; set; } = default!;
        public bool IsOccupied { get; set; } = false;
        public string? QrCodeToken { get; set; }
        public string? QrCodeImageUrl { get; set; }
    }
}
