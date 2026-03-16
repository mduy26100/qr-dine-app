using QRDine.Application.Features.Billing.Subscriptions.DTOs;
using QRDine.Domain.Billing;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Subscriptions.Services
{
    public interface ISubscriptionService
    {
        Task<Subscription> AssignPlanAsync(
            Guid merchantId,
            Guid planId,
            PaymentMethod paymentMethod,
            decimal? overrideAmount = null,
            string? adminNote = null,
            CancellationToken cancellationToken = default);

        Task<MerchantSubscriptionInfoDto?> GetLatestSubscriptionInfoAsync(Guid merchantId, CancellationToken cancellationToken = default);
        Task<bool> IsSubscriptionActiveAsync(Guid merchantId, CancellationToken cancellationToken = default);
    }
}
