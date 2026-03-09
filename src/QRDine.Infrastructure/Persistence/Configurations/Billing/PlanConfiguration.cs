using QRDine.Domain.Billing;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Billing
{
    internal class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.ToTable("Plans", SchemaNames.Billing);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.HasIndex(x => x.Code).IsUnique();

            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);

            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.FeatureLimit)
                   .WithOne(x => x.Plan)
                   .HasForeignKey<FeatureLimit>(x => x.PlanId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Subscriptions)
                   .WithOne(x => x.Plan)
                   .HasForeignKey(x => x.PlanId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
