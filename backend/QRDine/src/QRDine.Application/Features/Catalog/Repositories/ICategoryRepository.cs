using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task ShiftDisplayOrdersAsync(Guid? parentId, int fromOrder, CancellationToken cancellationToken);

        Task<int> GetMaxDisplayOrderAsync(Guid? parentId, CancellationToken cancellationToken);
    }
}
