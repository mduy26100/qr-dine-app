namespace QRDine.API.DependencyInjection.Presentation
{
    public class IpBlockMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public IpBlockMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

            if (_cache.TryGetValue($"BlockedIP_{clientIp}", out _))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var json = JsonSerializer.Serialize(new
                {
                    type = "ip-banned",
                    message = "Hệ thống phát hiện hành vi truy cập bất thường. Địa chỉ IP của bạn đã bị khóa trong 24 giờ."
                });

                await context.Response.WriteAsync(json);
                return;
            }

            await _next(context);
        }
    }
}
