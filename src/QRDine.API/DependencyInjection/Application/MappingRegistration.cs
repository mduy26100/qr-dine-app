using QRDine.Application.Features.Billing.Mappings;
using QRDine.Application.Features.Catalog.Mappings;
using QRDine.Application.Features.Sales.Mappings;

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
                cfg.AddProfile<OrderMappingProfile>();
                cfg.AddProfile<OrderItemMappingProfile>();
                cfg.AddProfile<FeatureLimitMappingProfile>();
                cfg.AddProfile<PlanMappingProfile>();
            });

            return services;
        }
    }
}
