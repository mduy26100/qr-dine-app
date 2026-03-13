using QRDine.API.Constants;
using QRDine.Application.Common.Models;
using QRDine.Application.Features.Billing.Plans.Commands.AssignPlan;
using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Tenant.Merchants.DTOs;
using QRDine.Application.Features.Tenant.Merchants.Queries.GetAdminMerchants;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Admin
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/merchants")]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Admin)]
    public class MerchantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MerchantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AdminMerchantDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMerchants([FromQuery] GetAdminMerchantsQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }


        [HttpPost("{merchantId:guid}/assign-plan")]
        [ProducesResponseType(typeof(AssignPlanResponseDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> AssignPlan([FromRoute] Guid merchantId, [FromBody] AssignPlanDto dto, CancellationToken cancellationToken)
        {
            var command = new AssignPlanCommand(merchantId, dto);
            var result = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, result);
        }
    }
}
