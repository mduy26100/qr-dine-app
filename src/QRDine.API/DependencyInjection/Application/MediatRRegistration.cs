using QRDine.Application.Common.Behaviors;
using QRDine.Application.Features.Identity.Commands.Login;

namespace QRDine.API.DependencyInjection.Application
{
    public static class MediatRRegistration
    {
        public static IServiceCollection AddMediatRPipeline(this IServiceCollection services)
        {
            var applicationAssembly = typeof(LoginCommand).Assembly;

            var commonAssembly = typeof(ValidationBehavior<,>).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(applicationAssembly, commonAssembly);

                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssemblies(new[] { applicationAssembly, commonAssembly });

            return services;
        }
    }
}
