using QRDine.API.Constants;
using QRDine.API.Responses;

namespace QRDine.API.DependencyInjection.Presentation
{
    public static class RateLimiterServiceCollectionExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    var apiError = new ApiError
                    {
                        Type = "too-many-requests",
                        Message = "Bạn thao tác quá nhanh. Vui lòng đợi vài phút rồi thử lại."
                    };

                    var meta = new Meta
                    {
                        Timestamp = DateTime.UtcNow,
                        Path = context.HttpContext.Request.Path,
                        Method = context.HttpContext.Request.Method,
                        StatusCode = StatusCodes.Status429TooManyRequests,
                        TraceId = context.HttpContext.TraceIdentifier,
                        ClientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString()
                    };

                    var response = ApiResponse.Fail(apiError);
                    response.Meta = meta;

                    var json = JsonSerializer.Serialize(response, JsonOptions);
                    await context.HttpContext.Response.WriteAsync(json, cancellationToken: token);
                };

                string GetClientIp(HttpContext httpContext) =>
                    httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
                    httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ??
                    httpContext.Connection.RemoteIpAddress?.ToString() ??
                    "unknown_ip";

                options.AddPolicy(RateLimitPolicies.Login, httpContext =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter(GetClientIp(httpContext), partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        AutoReplenishment = true
                    });
                });

                options.AddPolicy(RateLimitPolicies.Register, httpContext =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter(GetClientIp(httpContext), partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(5),
                        AutoReplenishment = true
                    });
                });
            });

            return services;
        }
    }
}
