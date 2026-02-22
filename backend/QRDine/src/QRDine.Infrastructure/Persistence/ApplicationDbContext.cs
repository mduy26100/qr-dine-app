using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Infrastructure.Auth.Models;
using QRDine.Infrastructure.Persistence.Configurations.Auth;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Auth tables
            builder.ApplyConfiguration(new RefreshTokenConfiguration());
            builder.ApplyConfiguration(new PermissionConfiguration());
            builder.ApplyConfiguration(new RolePermissionConfiguration());
            builder.Entity<ApplicationUser>().ToTable("Users", SchemaNames.Identity);
            builder.Entity<ApplicationRole>().ToTable("Roles", SchemaNames.Identity);
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", SchemaNames.Identity);
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", SchemaNames.Identity);
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", SchemaNames.Identity);
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", SchemaNames.Identity);
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", SchemaNames.Identity);
        }


        // Auth
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
    }
}
