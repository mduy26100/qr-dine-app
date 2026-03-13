using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Billing.Plans.Specifications;
using QRDine.Application.Features.Billing.Repositories;

namespace QRDine.Application.Features.Billing.Plans.Queries.GetAdminPlans
{
    public class GetAdminPlansQueryHandler : IRequestHandler<GetAdminPlansQuery, List<PlanDto>>
    {
        private readonly IPlanRepository _planRepository;

        public GetAdminPlansQueryHandler(IPlanRepository planRepository)
        {
            _planRepository = planRepository;
        }

        public async Task<List<PlanDto>> Handle(GetAdminPlansQuery request, CancellationToken cancellationToken)
        {
            var spec = new ActivePlansSpec();
            return await _planRepository.ListAsync(spec, cancellationToken);
        }
    }
}
