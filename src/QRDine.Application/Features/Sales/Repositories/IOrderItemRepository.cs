using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Repositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
    }
}
