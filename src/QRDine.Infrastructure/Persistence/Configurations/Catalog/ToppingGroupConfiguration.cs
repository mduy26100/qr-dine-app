using QRDine.Domain.Catalog;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Catalog
{
    public class ToppingGroupConfiguration : IEntityTypeConfiguration<ToppingGroup>
    {
        public void Configure(EntityTypeBuilder<ToppingGroup> builder)
        {
            builder.ToTable("ToppingGroups", SchemaNames.Catalog);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Description).HasMaxLength(2000);

            builder.HasOne(x => x.Merchant)
                   .WithMany()
                   .HasForeignKey(x => x.MerchantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Toppings)
                   .WithOne(t => t.ToppingGroup)
                   .HasForeignKey(t => t.ToppingGroupId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.MerchantId);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
