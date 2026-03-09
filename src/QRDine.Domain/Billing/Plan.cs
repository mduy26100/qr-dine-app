using QRDine.Domain.Common;

namespace QRDine.Domain.Billing
{
    public class Plan : BaseEntity
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual FeatureLimit FeatureLimit { get; set; } = default!;
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
