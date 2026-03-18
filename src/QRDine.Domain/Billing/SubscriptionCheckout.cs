using QRDine.Domain.Common;
using QRDine.Domain.Enums;
using QRDine.Domain.Tenant;

namespace QRDine.Domain.Billing
{
    public class SubscriptionCheckout : BaseEntity, IMustHaveMerchant
    {
        public long OrderCode { get; set; }
        public Guid MerchantId { get; set; }
        public Guid PlanId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }

        public virtual Merchant Merchant { get; set; } = default!;
        public virtual Plan Plan { get; set; } = default!;
    }
}
