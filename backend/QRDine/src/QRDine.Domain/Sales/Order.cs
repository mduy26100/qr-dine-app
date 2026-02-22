using QRDine.Domain.Catalog;
using QRDine.Domain.Common;
using QRDine.Domain.Enums;
using QRDine.Domain.Tenant;

namespace QRDine.Domain.Sales
{
    public class Order : BaseEntity
    {
        public Guid MerchantId { get; set; }
        public Guid TableId { get; set; }
        public Guid SessionId { get; set; } = default!;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }

        public virtual Merchant Merchant { get; set; } = default!;
        public virtual Table Table { get; set; } = default!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
