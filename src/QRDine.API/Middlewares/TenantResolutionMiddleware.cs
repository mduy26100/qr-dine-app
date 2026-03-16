using QRDine.Application.Common.Constants;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Middlewares
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private const string TenantHeaderName = "X-Merchant-Id";
        private const string TenantQueryName = "merchantId";
        private const string TenantRouteName = "merchantId";

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var hasMerchantClaim = context.User?.HasClaim(c => c.Type == AppClaimTypes.MerchantId) ?? false;

            if (!hasMerchantClaim)
            {
                Guid? resolvedMerchantId = null;

                if (context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValues) &&
                    Guid.TryParse(headerValues.FirstOrDefault(), out Guid parsedHeaderId))
                {
                    resolvedMerchantId = parsedHeaderId;
                }

                if (resolvedMerchantId == null)
                {
                    var routeValue = context.GetRouteValue(TenantRouteName);
                    if (routeValue != null && Guid.TryParse(routeValue.ToString(), out Guid parsedRouteId))
                    {
                        resolvedMerchantId = parsedRouteId;
                    }
                }

                if (resolvedMerchantId == null &&
                    context.Request.Query.TryGetValue(TenantQueryName, out var queryValues) &&
                    Guid.TryParse(queryValues.FirstOrDefault(), out Guid parsedQueryId))
                {
                    resolvedMerchantId = parsedQueryId;
                }

                if (resolvedMerchantId.HasValue)
                {
                    context.Items[HttpContextKeys.ResolvedMerchantId] = resolvedMerchantId.Value;
                }
            }

            await _next(context);
        }
    }
}
