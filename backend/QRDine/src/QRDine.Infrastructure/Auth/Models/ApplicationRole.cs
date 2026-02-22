namespace QRDine.Infrastructure.Auth.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
