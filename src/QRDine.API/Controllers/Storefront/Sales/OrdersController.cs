using QRDine.API.Constants;
using QRDine.Application.Features.Sales.Orders.Commands.StorefrontCreateOrder;
using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.API.Controllers.Storefront.Sales
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/storefront/merchants/{merchantId:guid}/orders")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Storefront)]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromRoute] Guid merchantId, [FromBody] StorefrontCreateOrderDto dto, CancellationToken cancellationToken)
        {
            var command = new StorefrontCreateOrderCommand(merchantId, dto);
            var result = await _mediator.Send(command);
            return Created(string.Empty, result);
        }
    }
}
