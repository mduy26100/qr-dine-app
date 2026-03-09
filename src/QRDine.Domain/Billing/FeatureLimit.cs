using QRDine.Domain.Common;

namespace QRDine.Domain.Billing
{
    public class FeatureLimit : BaseEntity
    {
        public Guid PlanId { get; set; }

        public int? MaxTables { get; set; }
        public int? MaxProducts { get; set; }
        public int? MaxStaffMembers { get; set; }
        public bool AllowAdvancedReports { get; set; }

        public virtual Plan Plan { get; set; } = default!;
    }
}
