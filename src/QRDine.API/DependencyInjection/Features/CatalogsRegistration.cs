using QRDine.Application.Common.Abstractions.ExternalServices.QrCode;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Infrastructure.Catalog.Repositories;
using QRDine.Infrastructure.ExternalServices.QrCode;

namespace QRDine.API.DependencyInjection.Features
{
    public static class CatalogsRegistration
    {
        public static IServiceCollection AddCatalogsFeature(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ITableRepository, TableRepository>();
            services.AddScoped<IToppingGroupRepository, ToppingGroupRepository>();
            services.AddScoped<IProductToppingGroupRepository, ProductToppingGroupRepository>();
            services.AddScoped<IToppingRepository, ToppingRepository>();

            //Services
            services.AddScoped<ITableQrGeneratorService, TableQrGeneratorService>();

            return services;
        }
    }
}
