using QRDine.Domain.Billing;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Billing
{
    internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions", SchemaNames.Billing);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ProviderReference).HasMaxLength(250);
            builder.Property(x => x.AdminNote).HasMaxLength(500);

            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Method).HasConversion<string>().HasMaxLength(30);

            builder.Property(x => x.PlanSnapshotName).HasMaxLength(150);

            builder.HasOne(x => x.Merchant)
                   .WithMany()
                   .HasForeignKey(x => x.MerchantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Plan)
                   .WithMany()
                   .HasForeignKey(x => x.PlanId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
