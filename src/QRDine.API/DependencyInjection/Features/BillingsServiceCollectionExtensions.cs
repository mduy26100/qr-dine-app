using QRDine.Application.Features.Billing.Repositories;
using QRDine.Infrastructure.Billing.Repositories;

namespace QRDine.API.DependencyInjection.Features
{
    public static class BillingsServiceCollectionExtensions
    {
        public static IServiceCollection AddBillingsFeature(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IFeatureLimitRepository, FeatureLimitRepository>();
            services.AddScoped<IPlanRepository, PlanRepository>();

            return services;
        }
    }
}
