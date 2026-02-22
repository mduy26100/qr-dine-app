using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Infrastructure.Persistence;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class PersistenceRegistration
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            return services;
        }
    }
}
