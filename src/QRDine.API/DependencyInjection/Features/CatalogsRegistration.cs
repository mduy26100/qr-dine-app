using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Infrastructure.Catalog.Repositories;

namespace QRDine.API.DependencyInjection.Features
{
    public static class CatalogsRegistration
    {
        public static IServiceCollection AddCatalogsFeature(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            return services;
        }
    }
}
