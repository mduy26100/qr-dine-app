using QRDine.Application.Features.Billing.FeatureLimits.Services;
using QRDine.Domain.Enums;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Filters
{
    public class FeatureLimitFilter : IAsyncActionFilter
    {
        private readonly FeatureType _featureType;
        private readonly IFeatureLimitService _featureLimitService;

        public FeatureLimitFilter(FeatureType featureType, IFeatureLimitService featureLimitService)
        {
            _featureType = featureType;
            _featureLimitService = featureLimitService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var merchantIdString = user.FindFirst(AppClaimTypes.MerchantId)?.Value;
                var planCode = user.FindFirst(AppClaimTypes.PlanCode)?.Value;

                if (Guid.TryParse(merchantIdString, out Guid merchantId) && !string.IsNullOrEmpty(planCode))
                {
                    await _featureLimitService.CheckLimitAsync(merchantId, planCode, _featureType);
                }
            }

            await next();
        }
    }
}
