using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Domain.Catalog;
using QRDine.Infrastructure.Persistence;
using QRDine.Infrastructure.Persistence.Repositories;

namespace QRDine.Infrastructure.Catalog.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task ShiftDisplayOrdersAsync(Guid? parentId, int fromOrder, CancellationToken cancellationToken)
        {
            await _dbSet
                .Where(c => c.ParentId == parentId && c.DisplayOrder >= fromOrder)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayOrder, c => c.DisplayOrder + 1), cancellationToken);
        }

        public async Task<int> GetMaxDisplayOrderAsync(Guid? parentId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(c => c.ParentId == parentId)
                .MaxAsync(c => (int?)c.DisplayOrder, cancellationToken) ?? 0;
        }
    }
}
