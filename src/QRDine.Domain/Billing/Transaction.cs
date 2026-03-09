using QRDine.Domain.Common;
using QRDine.Domain.Enums;
using QRDine.Domain.Tenant;

namespace QRDine.Domain.Billing
{
    public class Transaction : BaseEntity, IMustHaveMerchant
    {
        public Guid MerchantId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid PlanId { get; set; }

        public decimal Amount { get; set; }
        public string? ProviderReference { get; set; }

        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? AdminNote { get; set; }

        public virtual Plan Plan { get; set; } = default!;
        public virtual Merchant Merchant { get; set; } = default!;
        public virtual Subscription Subscription { get; set; } = default!;
    }
}
