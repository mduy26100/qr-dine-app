using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Plans.Specifications
{
    public class GetSubscriptionByMerchantIdSpec : Specification<Subscription>, ISingleResultSpecification<Subscription>
    {
        public GetSubscriptionByMerchantIdSpec(Guid merchantId)
        {
            Query.Where(s => s.MerchantId == merchantId);
        }
    }
}
