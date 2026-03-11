using QRDine.Application.Features.Tenant.Repositories;
using QRDine.Domain.Tenant;
using QRDine.Infrastructure.Persistence;
using QRDine.Infrastructure.Persistence.Repositories;

namespace QRDine.Infrastructure.Tenant.Repositories
{
    public class MerchantRepository : Repository<Merchant>, IMerchantRepository
    {
        public MerchantRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
