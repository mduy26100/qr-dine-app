using QRDine.Application.Features.Catalog.Mappings;

namespace QRDine.API.DependencyInjection.Application
{
    public static class MappingRegistration
    {
        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<CategoryMappingProfile>();
                cfg.AddProfile<ProductMappingProfile>();
                cfg.AddProfile<TableMappingProfile>();
            });

            return services;
        }
    }
}
