using SharedKernel.Application.Interfaces.Persistence;

namespace SharedKernel.Infrastructure.Persistence
{
    public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : class
    {
        public Repository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
