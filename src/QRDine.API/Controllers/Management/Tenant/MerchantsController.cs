using QRDine.API.Attributes;
using QRDine.API.Constants;
using QRDine.API.Requests.Tenant;
using QRDine.Application.Features.Tenant.Merchants.Commands.UpdateMerchant;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Tenant
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/merchants")]
    [Authorize(Roles = SystemRoles.Merchant)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class MerchantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MerchantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut]
        [Consumes("multipart/form-data")]
        [SkipSubscriptionCheck]
        public async Task<IActionResult> UpdateMerchant([FromForm] UpdateMerchantForm form, CancellationToken cancellationToken)
        {
            var command = new UpdateMerchantCommand(form.ToDto());
            var result = await _mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}
