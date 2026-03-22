using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Services
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
        Task UpdateProfileAsync(string userId, UpdateProfileRequestDto request, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    }
}
