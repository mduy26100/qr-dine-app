namespace QRDine.Application.Features.Billing.Plans.DTOs
{
    public class PlanResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsActive { get; set; }

        public FeatureLimitDto FeatureLimit { get; set; } = default!;
    }
}
