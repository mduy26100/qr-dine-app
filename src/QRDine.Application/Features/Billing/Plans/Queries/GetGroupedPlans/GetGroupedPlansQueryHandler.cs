using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Billing.Plans.Specifications;
using QRDine.Application.Features.Billing.Repositories;
using QRDine.Domain.Constants;

namespace QRDine.Application.Features.Billing.Plans.Queries.GetGroupedPlans
{
    public class GetGroupedPlansQueryHandler : IRequestHandler<GetGroupedPlansQuery, List<PlanTierGroupDto>>
    {
        private readonly IPlanRepository _planRepository;

        public GetGroupedPlansQueryHandler(IPlanRepository planRepository)
        {
            _planRepository = planRepository;
        }

        public async Task<List<PlanTierGroupDto>> Handle(GetGroupedPlansQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetActiveGroupablePlansSpec();
            var activePlans = await _planRepository.ListAsync(spec, cancellationToken);

            var groupedPlans = activePlans
                .GroupBy(p => p.Code.Split('_')[0])
                .Select(g => new PlanTierGroupDto
                {
                    TierName = g.Key,
                    Plans = g.ToList()
                })
                .ToList();

            var tierOrder = new List<string> { PlanTiers.Standard, PlanTiers.Premium, PlanTiers.Business };

            return groupedPlans
                .OrderBy(g => tierOrder.IndexOf(g.TierName) != -1 ? tierOrder.IndexOf(g.TierName) : 99)
                .ToList();
        }
    }
}
