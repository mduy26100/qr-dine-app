namespace QRDine.Application.Common.Abstractions.Persistence
{
    public interface IApplicationDbContext
    {
        Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}