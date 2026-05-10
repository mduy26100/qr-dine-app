using SharedKernel.Infrastructure.Identity.Constants;
using SharedKernel.Infrastructure.Identity.Models;

namespace SharedKernel.Infrastructure.Persistence.Configurations.Identity
{
    internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens", SchemaIdentity.Identity);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.HasIndex(x => x.Token).IsUnique();

            builder.Property(x => x.CreatedByIp).HasMaxLength(50);
            builder.Property(x => x.RevokedByIp).HasMaxLength(50);

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
