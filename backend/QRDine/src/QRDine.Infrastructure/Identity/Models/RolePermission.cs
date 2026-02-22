namespace QRDine.Infrastructure.Identity.Models
{
    public class RolePermission
    {
        public Guid RoleId { get; set; }
        public virtual ApplicationRole Role { get; set; } = default!;

        public Guid PermissionId { get; set; }
        public virtual Permission Permission { get; set; } = default!;
    }
}
