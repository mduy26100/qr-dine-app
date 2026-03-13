using QRDine.API.Constants;
using QRDine.Application.Features.Billing.Plans.Commands.AssignPlan;
using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Billing.Plans.Queries.GetAdminPlans;
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

        [HttpGet]
        [ProducesResponseType(typeof(List<PlanDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
        {
            var query = new GetAdminPlansQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignPlan([FromBody] AssignPlanDto dto, CancellationToken cancellationToken)
        {
            var command = new AssignPlanCommand(dto);
            var result = await _mediator.Send(command, cancellationToken);
            return Created(string.Empty, result);
        }
    }
}
