using QRDine.Application.Features.Billing.Plans.DTOs;

namespace QRDine.Application.Features.Billing.Plans.Queries.GetAdminPlans
{
    public record GetAdminPlansQuery : IRequest<List<PlanDto>>;
}
