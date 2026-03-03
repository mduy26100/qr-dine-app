using QRDine.API.Constants;
using QRDine.API.Services;
using QRDine.Application.Features.Identity.Commands.Login;
using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.API.Controllers.Identity
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuthCookieService _authCookieService;

        public AuthController(IMediator mediator, IAuthCookieService authCookieService)
        {
            _mediator = mediator;
            _authCookieService = authCookieService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
        {
            var command = new LoginCommand(dto);

            var result = await _mediator.Send(command, cancellationToken);

            _authCookieService.AppendRefreshTokenCookie(result.RefreshToken);

            return Ok(result);
        }
    }
}
