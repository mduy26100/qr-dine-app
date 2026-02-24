using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Application.Common.Exceptions;
using QRDine.Domain.Catalog;
using QRDine.Domain.Common;
using QRDine.Domain.Sales;
using QRDine.Domain.Tenant;
using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Persistence.Configurations.Catalog;
using QRDine.Infrastructure.Persistence.Configurations.Identity;
using QRDine.Infrastructure.Persistence.Configurations.Sales;
using QRDine.Infrastructure.Persistence.Configurations.Tenant;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService = null!;

        public ApplicationDbContext(DbContextOptions options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        protected ApplicationDbContext()
        {
        }

        public Guid? CurrentMerchantId => _currentUserService.MerchantId;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity tables
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

            // Tenant tables
            builder.ApplyConfiguration(new MerchantConfiguration());

            // Catalog tables
            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new ProductConfiguration());
            builder.ApplyConfiguration(new TableConfiguration());
            builder.ApplyConfiguration(new ToppingGroupConfiguration());
            builder.ApplyConfiguration(new ToppingConfiguration());
            builder.ApplyConfiguration(new ProductToppingGroupConfiguration());

            // Sales tables
            builder.ApplyConfiguration(new OrderConfiguration());
            builder.ApplyConfiguration(new OrderItemConfiguration());

            //Global query filters
            builder.Entity<Category>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
            builder.Entity<Product>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
            builder.Entity<Table>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
            builder.Entity<Order>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
            builder.Entity<ToppingGroup>().HasQueryFilter(e => !CurrentMerchantId.HasValue || e.MerchantId == CurrentMerchantId);
        }

        public async Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await Database.BeginTransactionAsync(cancellationToken);

            return new DatabaseTransaction(transaction);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var merchantId = _currentUserService.MerchantId;

            if (merchantId.HasValue)
            {
                var entries = ChangeTracker.Entries<IMustHaveMerchant>()
                                           .Where(e => e.State == EntityState.Added);

                foreach (var entry in entries)
                {
                    entry.Entity.MerchantId = merchantId.Value;
                }
            }

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("The data has been changed by someone else. Please try again.");
            }
        }

        // Identity
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Tenant
        public DbSet<Merchant> Merchants { get; set; }

        // Catalog
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<ProductToppingGroup> ProductToppingGroups { get; set; }
        public DbSet<ToppingGroup> ToppingGroups { get; set; }
        public DbSet<Topping> Toppings { get; set; }

        // Sales
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
