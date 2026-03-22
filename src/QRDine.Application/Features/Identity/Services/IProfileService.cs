using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Services
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    }
}
