using QRDine.Application.Features.Billing.Plans.DTOs;

namespace QRDine.Application.Features.Billing.Plans.Commands.AssignPlan
{
    public record AssignPlanCommand(Guid MerchantId, AssignPlanDto Dto) : IRequest<AssignPlanResponseDto>;
}
