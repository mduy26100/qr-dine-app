using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Identity.Services;

namespace QRDine.Application.Features.Identity.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IProfileService _profileService;

        public ChangePasswordCommandHandler(
            ICurrentUserService currentUserService,
            IProfileService profileService)
        {
            _currentUserService = currentUserService;
            _profileService = profileService;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");
            }

            string userIdString = _currentUserService.UserId.ToString();

            await _profileService.ChangePasswordAsync(userIdString, request.Dto, cancellationToken);

            return Unit.Value;
        }
    }
}
