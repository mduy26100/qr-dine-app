using QRDine.Domain.Catalog;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Catalog
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories", SchemaNames.Catalog);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasOne(x => x.Merchant)
                   .WithMany(m => m.Categories)
                   .HasForeignKey(x => x.MerchantId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Parent)
                   .WithMany(x => x.Children)
                   .HasForeignKey(x => x.ParentId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.MerchantId, x.ParentId, x.DisplayOrder });
            builder.HasIndex(x => new { x.MerchantId, x.ParentId, x.Name })
                   .IsUnique();
        }
    }
}