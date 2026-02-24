using QRDine.Domain.Sales;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Sales
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders", SchemaNames.Sales);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Note).HasMaxLength(1000);

            builder.HasOne(x => x.Merchant)
                   .WithMany()
                   .HasForeignKey(x => x.MerchantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Table)
                   .WithMany()
                   .HasForeignKey(x => x.TableId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.SessionId);
            builder.HasIndex(x => x.MerchantId);
            builder.HasIndex(x => x.TableId);
            builder.HasIndex(x => x.Status);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
