using QRDine.Domain.Billing;

namespace QRDine.Infrastructure.Persistence.Configurations.Billing
{
    public class SubscriptionCheckoutConfiguration : IEntityTypeConfiguration<SubscriptionCheckout>
    {
        public void Configure(EntityTypeBuilder<SubscriptionCheckout> builder)
        {
            builder.ToTable("SubscriptionCheckouts");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.OrderCode).IsUnique();

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.PlanSnapshotName).HasMaxLength(150);

            builder.HasOne(x => x.Merchant)
                .WithMany()
                .HasForeignKey(x => x.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Plan)
                .WithMany()
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(sc => !sc.Merchant.IsDeleted);
        }
    }
}
