using Microsoft.AspNetCore.HttpOverrides;
using QRDine.API.Services;

namespace QRDine.API.DependencyInjection.Presentation
{
    public static class PresentationServiceCollectionExtensions
    {
        public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddHttpContextAccessor();
            services.AddScoped<IAuthCookieService, AuthCookieService>();

            return services;
        }
    }
}
