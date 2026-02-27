using QRDine.API.Constants;
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

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
        {
            var command = new LoginCommand(dto);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
