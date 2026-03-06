using QRDine.API.Constants;
using QRDine.Application.Features.Sales.Orders.Commands.CloseOrder;
using QRDine.Application.Features.Sales.Orders.Commands.ManagementCreateOrder;
using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Sales
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/orders")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = $"{SystemRoles.Merchant},{SystemRoles.Staff}")]
        public async Task<IActionResult> CreateOrder([FromBody] ManagementCreateOrderDto dto, CancellationToken cancellationToken)
        {
            var command = new ManagementCreateOrderCommand(dto);
            var result =await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, result);
        }

        [HttpPut("{orderId:guid}/close")]
        [Authorize(Roles = $"{SystemRoles.Merchant},{SystemRoles.Staff}")]
        public async Task<IActionResult> CloseOrder([FromRoute] Guid orderId, [FromBody] CloseOrderDto payload, CancellationToken cancellationToken)
        {
            var command = new CloseOrderCommand(orderId, payload.TargetStatus);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}
