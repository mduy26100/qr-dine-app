using QRDine.Domain.Common;
using QRDine.Domain.Enums;
using QRDine.Domain.Tenant;

namespace QRDine.Domain.Billing
{
    public class Subscription : BaseEntity, IMustHaveMerchant
    {
        public Guid MerchantId { get; set; }
        public Guid PlanId { get; set; }

        public SubscriptionStatus Status { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? AdminNote { get; set; }

        public virtual Merchant Merchant { get; set; } = default!;
        public virtual Plan Plan { get; set; } = default!;
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
