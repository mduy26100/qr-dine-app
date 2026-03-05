using QRDine.Domain.Sales;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Sales
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems", SchemaNames.Sales);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.ProductName).IsRequired().HasMaxLength(256);
            builder.Property(x => x.ToppingsSnapshot).HasMaxLength(1000);

            builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne(x => x.Order)
                   .WithMany(o => o.OrderItems)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                   .WithMany()
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
