using QRDine.Domain.Catalog;
using QRDine.Domain.Common;

namespace QRDine.Domain.Tenant
{
    public class Merchant : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string? LogoUrl { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<Table> Tables { get; set; } = new List<Table>();
    }
}
