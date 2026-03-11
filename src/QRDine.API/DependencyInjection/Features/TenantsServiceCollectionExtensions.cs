using QRDine.Application.Features.Tenant.Repositories;
using QRDine.Infrastructure.Tenant.Repositories;

namespace QRDine.API.DependencyInjection.Features
{
    public static class TenantsServiceCollectionExtensions
    {
        public static IServiceCollection AddTenantsFeature(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IMerchantRepository, MerchantRepository>();

            return services;
        }
    }
}
