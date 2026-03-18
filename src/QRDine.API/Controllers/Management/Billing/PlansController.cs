using QRDine.API.Constants;
using QRDine.Application.Features.Billing.Plans.Commands.CreateCheckoutLink;
using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Billing
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/plans")]
    [Authorize(Roles = SystemRoles.Merchant)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
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
    }
}
