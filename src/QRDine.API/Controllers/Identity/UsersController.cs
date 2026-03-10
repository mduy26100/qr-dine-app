using QRDine.API.Attributes;
using QRDine.API.Constants;
using QRDine.Application.Features.Identity.Commands.Logout;
using QRDine.Application.Features.Identity.Commands.RegisterMerchant;
using QRDine.Application.Features.Identity.Commands.RegisterStaff;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Identity
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register-merchant")]
        [Authorize(Roles = SystemRoles.SuperAdmin)]
        public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Created(string.Empty, result);
        }

        [HttpPost("register-staff")]
        [Authorize(Roles = SystemRoles.Merchant)]
        [SkipSubscriptionCheck]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffCommand command, CancellationToken cancellationToken)
        {
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
