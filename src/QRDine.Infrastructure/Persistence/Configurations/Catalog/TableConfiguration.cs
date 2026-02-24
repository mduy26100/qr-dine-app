using QRDine.Domain.Catalog;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Catalog
{
    public class TableConfiguration : IEntityTypeConfiguration<Table>
    {
        public void Configure(EntityTypeBuilder<Table> builder)
        {
            builder.ToTable("Tables", SchemaNames.Catalog);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.QrCodeToken).HasMaxLength(500);

            builder.HasOne(x => x.Merchant)
                   .WithMany(m => m.Tables)
                   .HasForeignKey(x => x.MerchantId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.MerchantId);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
