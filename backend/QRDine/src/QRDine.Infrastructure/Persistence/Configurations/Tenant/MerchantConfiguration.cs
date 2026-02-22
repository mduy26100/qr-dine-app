using QRDine.Domain.Tenant;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Tenant
{
    public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
    {
        public void Configure(EntityTypeBuilder<Merchant> builder)
        {
            builder.ToTable("Merchants", SchemaNames.Tenant);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Name).IsRequired().HasMaxLength(256);

            builder.Property(x => x.Slug).IsRequired().HasMaxLength(256);
            builder.HasIndex(x => x.Slug).IsUnique();

            builder.Property(x => x.LogoUrl).HasMaxLength(2000);
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.PhoneNumber).HasMaxLength(20);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
