using SharedKernel.Infrastructure.Identity.Constants;
using SharedKernel.Infrastructure.Identity.Models;

namespace SharedKernel.Infrastructure.Persistence.Configurations.Identity
{
    internal class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions", SchemaIdentity.Identity);

            builder.HasKey(x => new { x.RoleId, x.PermissionId });

            builder.HasOne(x => x.Role)
                   .WithMany(x => x.RolePermissions)
                   .HasForeignKey(x => x.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Permission)
                   .WithMany(x => x.RolePermissions)
                   .HasForeignKey(x => x.PermissionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
