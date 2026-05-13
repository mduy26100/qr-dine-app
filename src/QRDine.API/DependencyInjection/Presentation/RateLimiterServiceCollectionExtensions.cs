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
                    var httpContext = context.HttpContext;
                    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
                    var path = httpContext.Request.Path.Value ?? string.Empty;

                    if (!path.Contains("/auth/"))
                    {
                        var cache = httpContext.RequestServices.GetRequiredService<IMemoryCache>();
                        cache.Set($"BlockedIP_{clientIp}", true, TimeSpan.FromHours(24));
                    }

                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    var apiError = new ApiError
                    {
                        Type = "too-many-requests",
                        Message = "Hệ thống phát hiện hành vi truy cập bất thường. Địa chỉ IP của bạn đã bị giới hạn."
                    };

                    var meta = new Meta
                    {
                        Timestamp = DateTime.UtcNow,
                        Path = path,
                        Method = httpContext.Request.Method,
                        StatusCode = StatusCodes.Status429TooManyRequests,
                        TraceId = httpContext.TraceIdentifier,
                        ClientIp = clientIp
                    };

                    var response = ApiResponse.Fail(apiError);
                    response.Meta = meta;

                    var json = JsonSerializer.Serialize(response, JsonOptions);
                    await httpContext.Response.WriteAsync(json, cancellationToken: token);
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromSeconds(10),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
                });

                options.AddPolicy(RateLimitPolicies.Login, httpContext =>
                {
                    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        AutoReplenishment = true
                    });
                });

                options.AddPolicy(RateLimitPolicies.Register, httpContext =>
                {
                    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, partition => new FixedWindowRateLimiterOptions
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
