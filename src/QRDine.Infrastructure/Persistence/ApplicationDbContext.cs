using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Application.Common.Constants;
using QRDine.Application.Common.Exceptions;
using QRDine.Domain.Billing;
using QRDine.Domain.Catalog;
using QRDine.Domain.Common;
using QRDine.Domain.Sales;
using QRDine.Domain.Tenant;
using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Persistence.Configurations.Billing;
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
        private readonly ICacheService _cacheService = null!;

        public ApplicationDbContext(DbContextOptions options, ICurrentUserService currentUserService, ICacheService cacheService) : base(options)
        {
            _currentUserService = currentUserService;
            _cacheService = cacheService;
        }

        protected ApplicationDbContext()
        {
        }

        public Guid CurrentMerchantId => _currentUserService.MerchantId ?? Guid.Empty;

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

            // Billing tables
            builder.ApplyConfiguration(new FeatureLimitConfiguration());
            builder.ApplyConfiguration(new PlanConfiguration());
            builder.ApplyConfiguration(new SubscriptionConfiguration());
            builder.ApplyConfiguration(new TransactionConfiguration());
            builder.ApplyConfiguration(new SubscriptionCheckoutConfiguration());

            //Global query filters
            // Apply a global query filter to automatically filter data by MerchantId and IsDeleted.
            builder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted && e.MerchantId == CurrentMerchantId);
            builder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted && e.MerchantId == CurrentMerchantId);
            builder.Entity<Table>().HasQueryFilter(e => !e.IsDeleted && e.MerchantId == CurrentMerchantId);
            builder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted && e.MerchantId == CurrentMerchantId);
            builder.Entity<ToppingGroup>().HasQueryFilter(e => !e.IsDeleted && e.MerchantId == CurrentMerchantId);

            // Apply a global query filter to automatically filter out IsDeleted data.
            builder.Entity<Subscription>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Transaction>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Plan>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<FeatureLimit>().HasQueryFilter(e => !e.IsDeleted);
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

            var hasCatalogChanges = ChangeTracker.Entries()
                .Any(e =>
                    (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted) &&
                    (e.Entity is Category || e.Entity is Product || e.Entity is ToppingGroup || e.Entity is Topping || e.Entity is ProductToppingGroup));

            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);

                if (hasCatalogChanges && result > 0 && merchantId.HasValue)
                {
                    var cacheKey = CacheKeys.StorefrontMenu(merchantId.Value);
                    await _cacheService.RemoveAsync(cacheKey, cancellationToken);
                }

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException("Dữ liệu đã bị thay đổi bởi một tiến trình khác.", ex);
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

        // Billing
        public DbSet<FeatureLimit> FeatureLimits { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<SubscriptionCheckout> SubscriptionCheckouts { get; set; }
    }
}
