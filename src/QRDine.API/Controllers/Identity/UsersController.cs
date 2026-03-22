using QRDine.API.Attributes;
using QRDine.API.Constants;
using QRDine.API.Requests.Identity;
using QRDine.Application.Features.Identity.Commands.Logout;
using QRDine.Application.Features.Identity.Commands.RegisterStaff;
using QRDine.Application.Features.Identity.Commands.UpdateProfile;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Queries.Profile;
using QRDine.Domain.Enums;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Identity
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    [Authorize]
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
        [SkipSubscriptionCheck]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var command = new LogoutCommand();
            var result = await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpGet("profile")]
        [SkipSubscriptionCheck]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var query = new ProfileQuery();
            var resule = await _mediator.Send(query, cancellationToken);

            return Ok(resule);
        }

        [HttpPut("update-profile")]
        [Consumes("multipart/form-data")]
        [SkipSubscriptionCheck]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileForm form, CancellationToken cancellationToken)
        {
            var command = new UpdateProfileCommand(form.ToDto());
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}
