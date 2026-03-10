using QRDine.Application.Features.Billing.Plans.DTOs;

namespace QRDine.Application.Features.Billing.Plans.Commands.CreatePlan
{
    public record CreatePlanCommand(CreatePlanDto Dto) : IRequest<PlanResponseDto>;
}
