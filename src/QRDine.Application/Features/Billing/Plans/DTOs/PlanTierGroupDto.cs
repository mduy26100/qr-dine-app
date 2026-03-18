namespace QRDine.Application.Features.Billing.Plans.DTOs
{
    public class PlanTierGroupDto
    {
        public string TierName { get; set; } = default!;
        public List<PlanDto> Plans { get; set; } = new();
    }
}
