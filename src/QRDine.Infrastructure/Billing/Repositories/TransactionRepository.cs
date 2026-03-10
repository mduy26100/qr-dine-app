using QRDine.Application.Features.Billing.Repositories;
using QRDine.Domain.Billing;
using QRDine.Infrastructure.Persistence;
using QRDine.Infrastructure.Persistence.Repositories;

namespace QRDine.Infrastructure.Billing.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
