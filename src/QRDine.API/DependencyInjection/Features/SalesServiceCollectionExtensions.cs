using QRDine.Application.Features.Sales.Repositories;
using QRDine.Infrastructure.Sales.Repositories;

namespace QRDine.API.DependencyInjection.Features
{
    public static class SalesServiceCollectionExtensions
    {
        public static IServiceCollection AddSales(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();

            return services;
        }
    }
}
