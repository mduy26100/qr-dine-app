using QRDine.Application.Features.Billing.FeatureLimits.Services;
using QRDine.Application.Features.Billing.Repositories;
using QRDine.Application.Features.Billing.Subscriptions.Services;
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
            services.AddScoped<ISubscriptionCheckoutRepository, SubscriptionCheckoutRepository>();

            // Services
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IFeatureLimitService, FeatureLimitService>();

            return services;
        }
    }
}
