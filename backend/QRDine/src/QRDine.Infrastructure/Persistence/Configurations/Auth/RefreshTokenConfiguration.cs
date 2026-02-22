using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Auth
{
    internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens", SchemaNames.Identity);

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
