using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;

namespace QRDine.Application.Features.Identity.Queries.Profile
{
    public class ProfileQueryHandler : IRequestHandler<ProfileQuery, UserProfileDto>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IProfileService _profileService;

        public ProfileQueryHandler(
            ICurrentUserService currentUserService,
            IProfileService profileService)
        {
            _currentUserService = currentUserService;
            _profileService = profileService;
        }

        public async Task<UserProfileDto> Handle(ProfileQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");
            }

            string userIdString = _currentUserService.UserId.ToString();

            return await _profileService.GetProfileAsync(userIdString, cancellationToken);
        }
    }
}
