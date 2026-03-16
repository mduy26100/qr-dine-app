using QRDine.API.Constants;
using QRDine.Application.Features.Catalog.ToppingGroups.Commands.CreateToppingGroup;
using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Catalog
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/topping-groups")]
    [Authorize(Roles = SystemRoles.Merchant)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class ToppingGroupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ToppingGroupsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateToppingGroup([FromBody] CreateToppingGroupRequestDto dto, CancellationToken cancellationToken)
        {
            var command = new CreateToppingGroupCommand(dto);

            var result = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, result);
        }
    }
}
