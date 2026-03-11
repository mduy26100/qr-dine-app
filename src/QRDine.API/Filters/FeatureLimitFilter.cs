using QRDine.API.Responses;
using QRDine.Application.Features.Billing.FeatureLimits.Services;
using QRDine.Domain.Enums;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Filters
{
    public class FeatureLimitFilter : IAsyncActionFilter
    {
        private readonly FeatureType _featureType;
        private readonly IFeatureLimitService _featureLimitService;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public FeatureLimitFilter(
            FeatureType featureType,
            IFeatureLimitService featureLimitService)
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
                    try
                    {
                        await _featureLimitService.CheckLimitAsync(
                            merchantId,
                            planCode,
                            _featureType);
                    }
                    catch (Exception ex)
                    {
                        await WritePaymentRequiredResponse(context, ex.Message);
                        return;
                    }
                }
            }

            await next();
        }

        private static async Task WritePaymentRequiredResponse(ActionExecutingContext context, string message)
        {
            var httpContext = context.HttpContext;

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status402PaymentRequired;

            var apiError = new ApiError
            {
                Type = "feature-limit-exceeded",
                Message = message
            };

            var meta = new Meta
            {
                Timestamp = DateTime.UtcNow,
                Path = httpContext.Request.Path,
                Method = httpContext.Request.Method,
                StatusCode = StatusCodes.Status402PaymentRequired,
                TraceId = httpContext.TraceIdentifier,
                RequestId = httpContext.Items["RequestId"]?.ToString(),
                ClientIp = httpContext.Connection.RemoteIpAddress?.ToString()
            };

            var response = ApiResponse.Fail(apiError);
            response.Meta = meta;

            var json = JsonSerializer.Serialize(response, JsonOptions);

            await httpContext.Response.WriteAsync(json);
        }
    }
}
