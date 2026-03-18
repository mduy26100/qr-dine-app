using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Billing.Plans.Extensions;
using QRDine.Domain.Billing;
using QRDine.Domain.Constants;

namespace QRDine.Application.Features.Billing.Plans.Specifications
{
    public class GetActiveGroupablePlansSpec : Specification<Plan, PlanDto>
    {
        public GetActiveGroupablePlansSpec()
        {
            Query.Where(p => p.IsActive && !p.Code.StartsWith(PlanTiers.Trial))
                 .OrderBy(p => p.DurationDays);

            Query.Select(PlanExtensions.ToPlanDto);
        }
    }
}
