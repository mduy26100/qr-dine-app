using QRDine.Application.Features.Billing.Subscriptions.DTOs;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Subscriptions.Extensions
{
    public static class SubscriptionSelectExtensions
    {
        public static Expression<Func<Subscription, MerchantSubscriptionInfoDto>> ToMerchantSubscriptionInfoDto =>
            s => new MerchantSubscriptionInfoDto
            {
                PlanCode = s.Plan.Code,
                Status = s.Status,
                EndDate = s.EndDate
            };
    }
}
