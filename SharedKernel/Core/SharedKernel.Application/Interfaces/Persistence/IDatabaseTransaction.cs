namespace SharedKernel.Application.Interfaces.Persistence
{
    public interface IDatabaseTransaction : IDisposable, IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
