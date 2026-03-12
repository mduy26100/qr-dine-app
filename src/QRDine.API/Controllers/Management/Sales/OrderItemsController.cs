using QRDine.API.Constants;
using QRDine.Application.Features.Sales.OrderItems.Commands.UpdateOrderItemsStatus;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Sales
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/order-items")]
    [Authorize(Roles = $"{SystemRoles.Merchant},{SystemRoles.Staff}")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class OrderItemsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("status")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateItemsStatus([FromBody] UpdateOrderItemsStatusCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
