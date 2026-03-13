namespace QRDine.Application.Features.Tenant.Merchants.DTOs
{
    public class AdminMerchantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }

        public string? CurrentPlanName { get; set; }
        public DateTime? PlanEndDate { get; set; }
        public string? SubscriptionStatus { get; set; }
    }
}
