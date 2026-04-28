using QRDine.Application.Common.Abstractions.BackgroundJobs;
using QRDine.Infrastructure.BackgroundJobs;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class BackgroundJobServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString));

            services.AddHangfireServer();

            services.AddTransient<IBackgroundJobService, HangfireBackgroundJobService>();

            return services;
        }
    }
}
