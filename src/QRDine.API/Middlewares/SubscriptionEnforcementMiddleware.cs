using QRDine.API.Attributes;
using QRDine.API.Constants;
using QRDine.API.Responses;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Middlewares
{
    public class SubscriptionEnforcementMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SubscriptionEnforcementMiddleware> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public SubscriptionEnforcementMiddleware(RequestDelegate next, ILogger<SubscriptionEnforcementMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (context.Request.Path.StartsWithSegments(ApiRoutePrefixes.Webhooks))
            {
                await _next(context);
                return;
            }

            if (endpoint == null || endpoint.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            if (endpoint.Metadata?.GetMetadata<SkipSubscriptionCheckAttribute>() != null)
            {
                await _next(context);
                return;
            }

            var user = context.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                bool isMerchant = user.IsInRole(SystemRoles.Merchant);
                bool isStaff = user.IsInRole(SystemRoles.Staff);

                if (isMerchant || isStaff)
                {
                    var statusClaim = user.FindFirst(AppClaimTypes.SubscriptionStatus)?.Value;

                    if (string.IsNullOrEmpty(statusClaim) || statusClaim.Equals("Expired", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Chặn request từ Merchant/Staff do gói cước đã hết hạn.");
                        await WritePaymentRequiredResponse(context);
                        return;
                    }
                }
            }

            await _next(context);
        }

        private static async Task WritePaymentRequiredResponse(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status402PaymentRequired;

            var apiError = new ApiError
            {
                Type = "payment-required",
                Message = "Gói cước của cửa hàng đã hết hạn hoặc chưa được đăng ký. Vui lòng gia hạn để tiếp tục sử dụng hệ thống."
            };

            var meta = new Meta
            {
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = StatusCodes.Status402PaymentRequired,
                TraceId = context.TraceIdentifier,
                RequestId = context.Items["RequestId"]?.ToString(),
                ClientIp = context.Connection.RemoteIpAddress?.ToString()
            };

            var response = ApiResponse.Fail(apiError);
            response.Meta = meta;

            var json = JsonSerializer.Serialize(response, JsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
