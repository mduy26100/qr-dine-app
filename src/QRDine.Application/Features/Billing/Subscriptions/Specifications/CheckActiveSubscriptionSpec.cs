using QRDine.Domain.Billing;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Subscriptions.Specifications
{
    public class CheckActiveSubscriptionSpec : Specification<Subscription>
    {
        public CheckActiveSubscriptionSpec(Guid merchantId)
        {
            Query.Where(s => s.MerchantId == merchantId
                          && s.Status == SubscriptionStatus.Active
                          && s.EndDate > DateTime.UtcNow);
        }
    }
}
