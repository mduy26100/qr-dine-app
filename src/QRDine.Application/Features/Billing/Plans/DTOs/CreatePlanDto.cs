namespace QRDine.Application.Features.Billing.Plans.DTOs
{
    public class CreatePlanDto
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsActive { get; set; } = true;

        public FeatureLimitDto Limits { get; set; } = default!;
    }
}
