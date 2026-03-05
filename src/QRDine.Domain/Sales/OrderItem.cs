using QRDine.Domain.Catalog;
using QRDine.Domain.Common;

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

        public string? Note { get; set; }
    }
}
