using QRDine.Infrastructure.Auth.Constants;
using QRDine.Infrastructure.Auth.Models;

namespace QRDine.Infrastructure.Persistence.Seeding
{
    public class IdentitySeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<IdentitySeeder>();

            try
            {
                foreach (var roleName in SystemRoles.AllRoles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new ApplicationRole
                        {
                            Name = roleName,
                            Description = $"Role for {roleName}"
                        });
                        logger.LogInformation($"Seeded role: {roleName}");
                    }
                }

                var adminEmail = "admin@qrdine.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    var newAdmin = new ApplicationUser
                    {
                        UserName = "superadmin",
                        Email = adminEmail,
                        FirstName = "Super",
                        LastName = "Admin",
                        EmailConfirmed = true,
                        IsActive = true,
                        MerchantId = null
                    };

                    var result = await userManager.CreateAsync(newAdmin, "Admin@123!");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newAdmin, SystemRoles.SuperAdmin);
                        logger.LogInformation("Seeded SuperAdmin user successfully.");
                    }
                    else
                    {
                        logger.LogError("Failed to seed Admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred during Identity data seeding.");
            }
        }
    }
}
