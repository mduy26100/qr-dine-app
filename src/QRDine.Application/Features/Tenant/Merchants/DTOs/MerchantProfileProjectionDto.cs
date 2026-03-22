namespace QRDine.Application.Features.Tenant.Merchants.DTOs
{
    public class MerchantProfileProjectionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LogoUrl { get; set; }

        public string? PlanName { get; set; }
        public string? SubscriptionStatus { get; set; }
        public DateTime? PlanEndDate { get; set; }
    }
}
