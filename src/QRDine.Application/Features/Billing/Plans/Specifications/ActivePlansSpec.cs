using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Billing.Plans.Extensions;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Plans.Specifications
{
    public class ActivePlansSpec : Specification<Plan, PlanDto>
    {
        public ActivePlansSpec()
        {
            Query.Where(p => p.IsActive)
                 .OrderBy(p => p.Price);

            Query.Select(PlanExtensions.ToAdminPlanDto);
        }
    }
}
