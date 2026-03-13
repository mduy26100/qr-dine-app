using QRDine.Application.Features.Tenant.Merchants.DTOs;
using QRDine.Domain.Enums;
using QRDine.Domain.Tenant;

namespace QRDine.Application.Features.Tenant.Merchants.Extensions
{
    public static class MerchantExtensions
    {
        public static Expression<Func<Merchant, AdminMerchantDto>> ToAdminMerchantDto =>
            m => new AdminMerchantDto
            {
                Id = m.Id,
                Name = m.Name,
                PhoneNumber = m.PhoneNumber,
                IsActive = m.IsActive,

                CurrentPlanName = m.Subscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => s.Plan != null ? s.Plan.Name : null)
                    .FirstOrDefault(),

                PlanEndDate = m.Subscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => (DateTime?)s.EndDate)
                    .FirstOrDefault(),

                SubscriptionStatus = m.Subscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => s.Status.ToString())
                    .FirstOrDefault()
            };
    }
}
