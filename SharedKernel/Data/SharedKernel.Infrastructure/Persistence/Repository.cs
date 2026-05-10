using SharedKernel.Application.Interfaces.Persistence;

namespace SharedKernel.Infrastructure.Persistence
{
    public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : class
    {
        protected readonly SharedKernelDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(SharedKernelDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
    }
}
