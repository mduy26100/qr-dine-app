using QRDine.Domain.Billing;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Plans.Services
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
    }
}
