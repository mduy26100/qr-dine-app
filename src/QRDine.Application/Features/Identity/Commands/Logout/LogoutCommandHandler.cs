using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Identity.Services;

namespace QRDine.Application.Features.Identity.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly ILogoutService _logoutService;
        private readonly ICurrentUserService _currentUserService;

        public LogoutCommandHandler(ILogoutService logoutService, ICurrentUserService currentUserService)
        {
            _logoutService = logoutService;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            await _logoutService.LogoutAsync(userId, cancellationToken);
            return Unit.Value;
        }
    }
}
