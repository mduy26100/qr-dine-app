using SharedKernel.Application.Interfaces.Persistence;
using SharedKernel.Infrastructure.Persistence;
using SharedKernel.Infrastructure.Persistence.Interceptors;

namespace SharedKernel.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedKernelInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<EfSlowQueryInterceptor>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}
