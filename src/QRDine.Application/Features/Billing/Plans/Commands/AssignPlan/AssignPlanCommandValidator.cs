namespace QRDine.Application.Features.Billing.Plans.Commands.AssignPlan
{
    public class AssignPlanCommandValidator : AbstractValidator<AssignPlanCommand>
    {
        public AssignPlanCommandValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty().WithMessage("MerchantId không được để trống.");

            RuleFor(x => x.Dto.PlanId)
                .NotEmpty().WithMessage("PlanId không được để trống.");
        }
    }
}
