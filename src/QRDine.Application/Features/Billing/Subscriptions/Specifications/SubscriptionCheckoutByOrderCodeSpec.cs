using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Subscriptions.Specifications
{
    public class SubscriptionCheckoutByOrderCodeSpec : SingleResultSpecification<SubscriptionCheckout>
    {
        public SubscriptionCheckoutByOrderCodeSpec(long orderCode)
        {
            Query.Where(x => x.OrderCode == orderCode);
        }
    }
}
