using QRDine.Application.Features.Tenant.Merchants.DTOs;
using QRDine.Domain.Enums;
using QRDine.Domain.Tenant;

namespace QRDine.Application.Features.Tenant.Merchants.Extensions
{
    public static class MerchantProfileExtensions
    {
        public static Expression<Func<Merchant, MerchantProfileProjectionDto>> ToProfileProjection =>
            m => new MerchantProfileProjectionDto
            {
                Id = m.Id,
                Name = m.Name,
                Slug = m.Slug,
                Address = m.Address,
                PhoneNumber = m.PhoneNumber,
                LogoUrl = m.LogoUrl,

                PlanName = m.Subscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => s.Plan.Name)
                    .FirstOrDefault(),

                SubscriptionStatus = m.Subscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => s.Status.ToString())
                    .FirstOrDefault(),

                PlanEndDate = m.Subscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => (DateTime?)s.EndDate)
                    .FirstOrDefault()
            };
    }
}
