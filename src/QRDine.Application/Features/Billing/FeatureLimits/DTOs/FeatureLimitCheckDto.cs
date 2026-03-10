namespace QRDine.Application.Features.Billing.FeatureLimits.DTOs
{
    public class FeatureLimitCheckDto
    {
        public int? MaxTables { get; set; }
        public int? MaxProducts { get; set; }
        public int? MaxStaffMembers { get; set; }
        public bool AllowAdvancedReports { get; set; }
    }
}
