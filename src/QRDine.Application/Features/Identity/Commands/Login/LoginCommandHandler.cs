using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;

namespace QRDine.Application.Features.Identity.Commands.Login
{
    internal class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly ILoginService _loginService;

        public LoginCommandHandler(ILoginService loginService)
        {
            _loginService = loginService;
        }

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var result = await _loginService.AuthenticateAsync(request.Dto, cancellationToken);
            return result;
        }
    }
}
