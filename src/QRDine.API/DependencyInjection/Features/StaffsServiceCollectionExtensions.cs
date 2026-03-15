using QRDine.Application.Features.Staffs.Services;
using QRDine.Infrastructure.Staffs.Services;

namespace QRDine.API.DependencyInjection.Features
{
    public static class StaffsServiceCollectionExtensions
    {
        public static IServiceCollection AddStaffsFeature(this IServiceCollection services)
        {
            services.AddScoped<IStaffService, StaffService>();

            return services;
        }
    }
}
