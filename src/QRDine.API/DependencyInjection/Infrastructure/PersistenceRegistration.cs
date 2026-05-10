using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Infrastructure.Persistence;
using QRDine.Infrastructure.Persistence.Interceptors;
using QRDine.Infrastructure.Persistence.Repositories;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class PersistenceRegistration
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                var logger = sp.GetRequiredService<ILogger<EfSlowQueryInterceptor>>();

                options.AddInterceptors(new EfSlowQueryInterceptor(logger));
            });

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}
