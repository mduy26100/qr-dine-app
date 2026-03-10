using QRDine.Application.Features.Billing.Subscriptions.DTOs;
using QRDine.Application.Features.Billing.Subscriptions.Extensions;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Subscriptions.Specifications
{
    public class GetLatestSubscriptionInfoSpec : Specification<Subscription, MerchantSubscriptionInfoDto>, ISingleResultSpecification<Subscription, MerchantSubscriptionInfoDto>
    {
        public GetLatestSubscriptionInfoSpec(Guid merchantId)
        {
            Query.Where(s => s.MerchantId == merchantId)
                 .OrderByDescending(s => s.EndDate);

            Query.Select(SubscriptionSelectExtensions.ToMerchantSubscriptionInfoDto);
        }
    }
}
