using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Subscriptions.DTOs
{
    public class MerchantSubscriptionInfoDto
    {
        public string PlanCode { get; set; } = default!;
        public SubscriptionStatus Status { get; set; }
        public DateTime EndDate { get; set; }
    }
}
