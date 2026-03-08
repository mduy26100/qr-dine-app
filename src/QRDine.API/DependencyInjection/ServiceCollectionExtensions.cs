using QRDine.API.DependencyInjection.Application;
using QRDine.API.DependencyInjection.CrossCutting;
using QRDine.API.DependencyInjection.Features;
using QRDine.API.DependencyInjection.Infrastructure;
using QRDine.API.DependencyInjection.Presentation;
using QRDine.API.DependencyInjection.Security;

namespace QRDine.API.DependencyInjection
{
    /// <summary>
    /// Main entry point for all service registrations.
    /// Groups services by architectural layer for clarity.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all application services in the correct order.
        /// </summary>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddInfrastructure(configuration)
                .AddSecurity(configuration)
                .AddCrossCutting(configuration)
                .AddApplication()
                .AddFeatures()
                .AddPresentation(configuration);
        }

        /// <summary>
        /// Infrastructure layer: Database, Cache, External Services
        /// </summary>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddPersistence(configuration);
            services.AddExternalServices(configuration);
            services.AddCryptography(configuration);
            services.AddCaching(configuration);
            return services;
        }

        /// <summary>
        /// Security layer: Identity, JWT Authentication
        /// </summary>
        public static IServiceCollection AddSecurity(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityServices(configuration);
            services.AddJwtAuthentication(configuration);
            return services;
        }

        /// <summary>
        /// Cross-cutting concerns: CORS, API Versioning, etc.
        /// </summary>
        public static IServiceCollection AddCrossCutting(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiVersioningConfig();
            services.AddCorsPolicies(configuration);
            return services;
        }

        /// <summary>
        /// Application layer: MediatR, Mapping, Behaviors
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatRPipeline();
            services.AddMapping();
            return services;
        }

        /// <summary>
        /// Feature modules: Auth, Products, Carts, etc.
        /// </summary>
        public static IServiceCollection AddFeatures(this IServiceCollection services)
        {
            services.AddCatalogsFeature();
            services.AddSalesFeature();
            return services;
        }

        /// <summary>
        /// Presentation: Controllers, HTTP concerns, Swagger
        /// </summary>
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddPresentationServices(configuration);
            return services;
        }
    }
}
