using QRDine.API.Constants;
using QRDine.API.Services;
using QRDine.Application.Features.Identity.Commands.Login;
using QRDine.Application.Features.Identity.Commands.RefreshToken;
using QRDine.Application.Features.Identity.Commands.RegisterMerchant;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Infrastructure.Identity.Constants;

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


        [HttpPost("register-merchant")]
        public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Created(string.Empty, result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
        {
            var plainRefreshToken = Request.Cookies[CookieNames.RefreshToken];

            if (string.IsNullOrWhiteSpace(plainRefreshToken))
            {
                return Unauthorized();
            }

            var command = new RefreshTokenCommand(plainRefreshToken);
            var result = await _mediator.Send(command, cancellationToken);

            _authCookieService.AppendRefreshTokenCookie(result.RefreshToken);

            return Ok(result);
        }
    }
}
