using QRDine.Domain.Catalog;
using QRDine.Domain.Common;

namespace QRDine.Domain.Sales
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }

        public virtual Product Product { get; set; } = default!;
        public virtual Order Order { get; set; } = default!;
    }
}
