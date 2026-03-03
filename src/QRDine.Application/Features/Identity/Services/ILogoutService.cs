namespace QRDine.Application.Features.Identity.Services
{
    public interface ILogoutService
    {
        Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
