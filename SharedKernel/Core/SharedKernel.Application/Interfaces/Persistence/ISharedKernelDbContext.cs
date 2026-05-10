namespace SharedKernel.Application.Interfaces.Persistence
{
    public interface ISharedKernelDbContext
    {
        Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}