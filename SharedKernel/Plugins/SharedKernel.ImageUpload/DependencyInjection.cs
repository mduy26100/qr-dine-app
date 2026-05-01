namespace SharedKernel.ImageUpload
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedCloudinaryImageUpload(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));

            services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();

            return services;
        }

        public static IServiceCollection AddSharedLocalImageUpload(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<LocalImageSettings>(configuration.GetSection(LocalImageSettings.SectionName));

            services.AddScoped<IImageUploadService, LocalImageUploadService>();

            return services;
        }
    }
}
