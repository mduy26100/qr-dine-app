namespace QRDine.API.DependencyInjection.Presentation
{
    public static class ForwardedHeadersServiceCollectionExtensions
    {
        public static IServiceCollection AddAppForwardedHeaders(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            return services;
        }
    }
}
