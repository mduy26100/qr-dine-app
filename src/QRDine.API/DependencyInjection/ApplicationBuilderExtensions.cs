using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Persistence;
using QRDine.Infrastructure.Persistence.Seeding;

namespace QRDine.API.DependencyInjection
{
    /// <summary>
    /// Extension methods for WebApplication (middleware pipeline configuration).
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Seeds initial data (roles, admin user, etc.) on application startup.
        /// </summary>
        public static async Task SeedDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                await IdentitySeeder.SeedAsync(userManager, roleManager, loggerFactory);
                await PlanSeeder.SeedAsync(context, loggerFactory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Error during startup seeding: {ex.Message}");
                throw;
            }
        }
    }
}
