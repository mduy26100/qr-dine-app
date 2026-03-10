namespace QRDine.Application.Features.Billing.Plans.DTOs
{
    public class FeatureLimitDto
    {
        public int? MaxTables { get; set; }
        public int? MaxProducts { get; set; }
        public int? MaxStaffMembers { get; set; }
        public bool AllowAdvancedReports { get; set; }
    }
}
