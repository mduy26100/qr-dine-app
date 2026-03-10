using QRDine.API.Constants;
using QRDine.Application.Features.Billing.Plans.Commands.CreatePlan;
using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Admin
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/plans")]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Admin)]
    public class PlansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto dto, CancellationToken cancellationToken)
        {
            var command = new CreatePlanCommand(dto);
            var result = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, result);
        }
    }
}
