namespace SharedKernel.QrCode
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedQrCodeGenerator(this IServiceCollection services)
        {
            services.AddTransient<IQrCodeService, QrCodeService>();

            return services;
        }
    }
}
