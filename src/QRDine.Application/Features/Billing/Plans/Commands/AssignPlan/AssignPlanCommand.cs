using QRDine.Application.Features.Billing.Plans.DTOs;

namespace QRDine.Application.Features.Billing.Plans.Commands.AssignPlan
{
    public record AssignPlanCommand(AssignPlanDto Dto) : IRequest<AssignPlanResponseDto>;
}
