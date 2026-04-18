using QRDine.Domain.Catalog;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Catalog
{
    public class ProductToppingGroupConfiguration : IEntityTypeConfiguration<ProductToppingGroup>
    {
        public void Configure(EntityTypeBuilder<ProductToppingGroup> builder)
        {
            builder.ToTable("ProductToppingGroups", SchemaNames.Catalog);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.HasIndex(x => new { x.ProductId, x.ToppingGroupId }).IsUnique();

            builder.HasOne(x => x.Product)
                   .WithMany(p => p.ProductToppingGroups)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ToppingGroup)
                   .WithMany(tg => tg.ProductToppingGroups)
                   .HasForeignKey(x => x.ToppingGroupId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ptg => ptg.ProductId);
            builder.HasIndex(ptg => ptg.ToppingGroupId);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
