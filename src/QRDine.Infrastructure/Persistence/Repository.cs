using QRDine.Application.Common.Abstractions.Persistence;

namespace QRDine.Infrastructure.Persistence.Repositories
{
    public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
    }
}