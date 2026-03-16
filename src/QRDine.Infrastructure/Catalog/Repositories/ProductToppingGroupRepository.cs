using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Domain.Catalog;
using QRDine.Infrastructure.Persistence;
using QRDine.Infrastructure.Persistence.Repositories;

namespace QRDine.Infrastructure.Catalog.Repositories
{
    public class ProductToppingGroupRepository : Repository<ProductToppingGroup>, IProductToppingGroupRepository
    {
        public ProductToppingGroupRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
