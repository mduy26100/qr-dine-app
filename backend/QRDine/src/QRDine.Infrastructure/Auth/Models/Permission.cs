namespace QRDine.Infrastructure.Auth.Models
{
    public class Permission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public string Module { get; set; } = default!;

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
