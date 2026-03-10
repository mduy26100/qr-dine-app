using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Plans.Specifications
{
    public class GetPlanByCodeSpec : Specification<Plan>, ISingleResultSpecification<Plan>
    {
        public GetPlanByCodeSpec(string code)
        {
            Query.Where(p => p.Code == code);
        }
    }
}
