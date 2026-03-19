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

            var logger = services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("ApplicationSeeder");

            var config = services.GetRequiredService<IConfiguration>();

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                var runMigrations = config.GetValue<bool>("RunMigrations");

                if (runMigrations)
                {
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        logger.LogInformation("Applying database migrations...");
                        context.Database.Migrate();
                        logger.LogInformation("Database migrations completed successfully.");
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations. Database is already up to date.");
                    }
                }
                else
                {
                    logger.LogInformation("Automatic database migrations are disabled.");
                }

                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                logger.LogInformation("Starting identity seeding...");
                await IdentitySeeder.SeedAsync(userManager, roleManager, loggerFactory);
                logger.LogInformation("Identity seeding completed.");

                logger.LogInformation("Starting plan seeding...");
                await PlanSeeder.SeedAsync(context, loggerFactory);
                logger.LogInformation("Plan seeding completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during application startup seeding.");
                throw;
            }
        }
    }
}