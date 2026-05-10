using SharedKernel.Application.Interfaces.Persistence;
using SharedKernel.Infrastructure.Identity.Constants;
using SharedKernel.Infrastructure.Identity.Models;
using SharedKernel.Infrastructure.Persistence.Configurations.Identity;

namespace SharedKernel.Infrastructure.Persistence
{
    public abstract class SharedKernelDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, ISharedKernelDbContext
    {
        protected SharedKernelDbContext(DbContextOptions options) : base(options)
        {
        }

        protected SharedKernelDbContext()
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new RefreshTokenConfiguration());
            builder.ApplyConfiguration(new PermissionConfiguration());
            builder.ApplyConfiguration(new RolePermissionConfiguration());

            builder.Entity<ApplicationUser>().ToTable("Users", SchemaIdentity.Identity);
            builder.Entity<ApplicationRole>().ToTable("Roles", SchemaIdentity.Identity);
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", SchemaIdentity.Identity);
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", SchemaIdentity.Identity);
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", SchemaIdentity.Identity);
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", SchemaIdentity.Identity);
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", SchemaIdentity.Identity);
        }

        public async Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await Database.BeginTransactionAsync(cancellationToken);

            return new DatabaseTransaction(transaction);
        }
    }
}
