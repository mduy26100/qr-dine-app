namespace QRDine.Application.Features.Billing.Plans.DTOs
{
    public class AssignPlanDto
    {
        public Guid MerchantId { get; set; }
        public Guid PlanId { get; set; }
        public string? AdminNote { get; set; }
    }
}
