using QRDine.API.Constants;
using QRDine.API.Services;
using QRDine.Application.Features.Identity.Commands.ConfirmRegister;
using QRDine.Application.Features.Identity.Commands.Login;
using QRDine.Application.Features.Identity.Commands.RefreshToken;
using QRDine.Application.Features.Identity.Commands.RegisterMerchant;
using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.API.Controllers.Identity
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Identity)]
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
        [EnableRateLimiting(RateLimitPolicies.Login)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
        {
            var command = new LoginCommand(dto);

            var result = await _mediator.Send(command, cancellationToken);

            _authCookieService.AppendRefreshTokenCookie(result.RefreshToken);

            return Ok(result);
        }


        [HttpPost("register-merchant")]
        [EnableRateLimiting(RateLimitPolicies.Register)]
        public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Created(string.Empty, new
            {
                message = "Yêu cầu đăng ký thành công. Vui lòng kiểm tra email để kích hoạt tài khoản.",
                data = result
            });
        }

        [HttpPost("confirm-register")]
        [EnableRateLimiting(RateLimitPolicies.Register)]
        public async Task<IActionResult> ConfirmRegister([FromBody] ConfirmRegisterCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
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
