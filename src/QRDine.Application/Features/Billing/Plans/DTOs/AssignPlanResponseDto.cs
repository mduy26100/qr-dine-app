namespace QRDine.Application.Features.Billing.Plans.DTOs
{
    public class AssignPlanResponseDto
    {
        public Guid SubscriptionId { get; set; }
        public Guid MerchantId { get; set; }
        public Guid PlanId { get; set; }
        public string Status { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? AdminNote { get; set; }
    }
}
