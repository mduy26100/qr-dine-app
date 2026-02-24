using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Identity
{
    internal class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions", SchemaNames.Identity);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(x => x.Code).IsUnique();

            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Module).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500);
        }
    }
}
