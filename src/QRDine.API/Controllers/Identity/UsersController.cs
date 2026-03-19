using QRDine.API.Attributes;
using QRDine.API.Constants;
using QRDine.Application.Features.Identity.Commands.Logout;
using QRDine.Application.Features.Identity.Commands.RegisterStaff;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Domain.Enums;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Identity
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Identity)]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register-staff")]
        [Authorize(Roles = SystemRoles.Merchant)]
        [CheckFeatureLimit(FeatureType.MaxStaffMembers)]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDto dto, CancellationToken cancellationToken)
        {
            var command = new RegisterStaffCommand(dto);
            var result = await _mediator.Send(command, cancellationToken);
            return Created(string.Empty, result);
        }

        [HttpPost("logout")]
        [Authorize]
        [SkipSubscriptionCheck]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var command = new LogoutCommand();
            var result = await _mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}
