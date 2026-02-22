namespace QRDine.Application.Common.Abstractions.Persistence
{
    public interface IApplicationDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}