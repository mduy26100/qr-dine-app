namespace SharedKernel.Application.Interfaces.Persistence
{
    public interface IRepository<T> : IRepositoryBase<T> where T : class
    {
    }
}
