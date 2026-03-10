using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
    }
}
