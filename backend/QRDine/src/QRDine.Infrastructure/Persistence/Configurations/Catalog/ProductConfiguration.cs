using QRDine.Domain.Catalog;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Catalog
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products", SchemaNames.Catalog);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.ImageUrl).HasMaxLength(2000);

            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Merchant)
                   .WithMany()
                   .HasForeignKey(x => x.MerchantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.MerchantId);
            builder.HasIndex(x => x.CategoryId);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
