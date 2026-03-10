using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.FeatureLimits.Services
{
    public interface IFeatureLimitService
    {
        Task CheckLimitAsync(Guid merchantId, string planCode, FeatureType featureType, CancellationToken cancellationToken = default);
    }
}
