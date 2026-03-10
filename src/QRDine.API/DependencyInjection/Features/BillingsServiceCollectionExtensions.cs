using QRDine.Application.Features.Billing.Plans.Services;
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
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // Services
            services.AddScoped<ISubscriptionService, SubscriptionService>();

            return services;
        }
    }
}
