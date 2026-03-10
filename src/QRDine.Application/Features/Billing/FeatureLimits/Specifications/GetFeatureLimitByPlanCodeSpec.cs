using QRDine.Application.Features.Billing.FeatureLimits.DTOs;
using QRDine.Application.Features.Billing.FeatureLimits.Extensions;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.FeatureLimits.Specifications
{
    public class GetFeatureLimitByPlanCodeSpec : Specification<FeatureLimit, FeatureLimitCheckDto>, ISingleResultSpecification<FeatureLimit, FeatureLimitCheckDto>
    {
        public GetFeatureLimitByPlanCodeSpec(string planCode)
        {
            Query.Where(fl => fl.Plan.Code == planCode);

            Query.Select(FeatureLimitExtensions.ToFeatureLimitCheckDto);
        }
    }
}
