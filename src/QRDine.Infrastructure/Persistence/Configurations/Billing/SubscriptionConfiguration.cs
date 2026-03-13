using QRDine.Domain.Billing;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Billing
{
    internal class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscriptions", SchemaNames.Billing);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

            builder.Property(x => x.AdminNote).HasMaxLength(500);

            builder.HasOne(x => x.Merchant)
                   .WithMany(m => m.Subscriptions)
                   .HasForeignKey(x => x.MerchantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Transactions)
                   .WithOne(x => x.Subscription)
                   .HasForeignKey(x => x.SubscriptionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
