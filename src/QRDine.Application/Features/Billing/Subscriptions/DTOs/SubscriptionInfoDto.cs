namespace QRDine.Application.Features.Billing.Subscriptions.DTOs
{
    public class SubscriptionInfoDto
    {
        public string PlanName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTime EndDate { get; set; }
        public int DaysRemaining { get; set; }
    }
}
