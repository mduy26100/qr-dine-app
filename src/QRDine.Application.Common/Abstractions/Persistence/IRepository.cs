namespace QRDine.Application.Common.Abstractions.Persistence
{
    public interface IRepository<T> : IRepositoryBase<T> where T : class
    {
    }
}
