using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Domain.Catalog;
using QRDine.Infrastructure.Persistence;
using QRDine.Infrastructure.Persistence.Repositories;

namespace QRDine.Infrastructure.Catalog.Repositories
{
    public class ToppingRepository : Repository<Topping>, IToppingRepository
    {
        public ToppingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
