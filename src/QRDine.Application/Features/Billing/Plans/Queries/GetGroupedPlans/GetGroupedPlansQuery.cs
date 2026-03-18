using QRDine.Application.Features.Billing.Plans.DTOs;

namespace QRDine.Application.Features.Billing.Plans.Queries.GetGroupedPlans
{
    public record GetGroupedPlansQuery : IRequest<List<PlanTierGroupDto>>;
}
