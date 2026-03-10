using QRDine.Application.Features.Billing.Repositories;
using QRDine.Domain.Billing;
using QRDine.Infrastructure.Persistence;
using QRDine.Infrastructure.Persistence.Repositories;

namespace QRDine.Infrastructure.Billing.Repositories
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
