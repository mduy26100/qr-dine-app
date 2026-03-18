using QRDine.API.Attributes;
using QRDine.API.Constants;
using QRDine.Application.Features.Billing.Plans.Commands.CreateCheckoutLink;
using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Billing.Plans.Queries.GetGroupedPlans;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Billing
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/plans")]
    [Authorize(Roles = SystemRoles.Merchant)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    [SkipSubscriptionCheck]
    public class PlansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{id}/checkout-link")]
        public async Task<IActionResult> CreateCheckoutLink(Guid id, [FromBody] CreateCheckoutDto dto)
        {
            var command = new CreateCheckoutLinkCommand(id, dto);
            var checkoutUrl = await _mediator.Send(command);

            return Ok(new { url = checkoutUrl });
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PlanTierGroupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroupedPlans(CancellationToken cancellationToken)
        {
            var query = new GetGroupedPlansQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
