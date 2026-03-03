using QRDine.API.Services;

namespace QRDine.API.DependencyInjection.Presentation
{
    public static class PresentationServiceCollectionExtensions
    {
        public static IServiceCollection AddPresentationServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IAuthCookieService, AuthCookieService>();

            return services;
        }
    }
}
