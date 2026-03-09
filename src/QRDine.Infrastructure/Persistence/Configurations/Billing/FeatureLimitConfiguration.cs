using QRDine.Domain.Billing;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Billing
{
    internal class FeatureLimitConfiguration : IEntityTypeConfiguration<FeatureLimit>
    {
        public void Configure(EntityTypeBuilder<FeatureLimit> builder)
        {
            builder.ToTable("FeatureLimits", SchemaNames.Billing);

            builder.HasKey(x => x.Id);
        }
    }
}
