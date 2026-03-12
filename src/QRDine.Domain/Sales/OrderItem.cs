using QRDine.Domain.Catalog;
using QRDine.Domain.Common;
using QRDine.Domain.Enums;

namespace QRDine.Domain.Sales
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = default!;

        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;

        public string ProductName { get; set; } = default!;
        public decimal UnitPrice { get; set; }

        public string? ToppingsSnapshot { get; set; }

        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;

        public string? Note { get; set; }
    }
}
