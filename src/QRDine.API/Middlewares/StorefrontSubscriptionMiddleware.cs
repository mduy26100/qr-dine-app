using QRDine.API.Responses;
using QRDine.Application.Common.Constants;
using QRDine.Application.Features.Billing.Subscriptions.Services;

namespace QRDine.API.Middlewares
{
    public class StorefrontSubscriptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StorefrontSubscriptionMiddleware> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public StorefrontSubscriptionMiddleware(RequestDelegate next, ILogger<StorefrontSubscriptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api/v1/storefront", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (context.Items.TryGetValue(HttpContextKeys.ResolvedMerchantId, out var resolvedId) && resolvedId is Guid merchantId)
            {
                var subscriptionService = context.RequestServices.GetRequiredService<ISubscriptionService>();

                var isActive = await subscriptionService.IsSubscriptionActiveAsync(merchantId, context.RequestAborted);

                if (!isActive)
                {
                    _logger.LogWarning($"Storefront access blocked: Merchant {merchantId} has no active subscription.");
                    await WriteStorefrontClosedResponse(context);
                    return;
                }
            }

            await _next(context);
        }

        private static async Task WriteStorefrontClosedResponse(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            var apiError = new ApiError
            {
                Type = "store-closed",
                Message = "Cửa hàng hiện đang tạm ngưng phục vụ. Vui lòng quay lại sau!"
            };

            var meta = new Meta
            {
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = StatusCodes.Status403Forbidden,
                TraceId = context.TraceIdentifier,
                ClientIp = context.Connection.RemoteIpAddress?.ToString()
            };

            var response = ApiResponse.Fail(apiError);
            response.Meta = meta;

            var json = JsonSerializer.Serialize(response, JsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
