using QRDine.API.Constants;
using QRDine.Application.Common.Models;
using QRDine.Application.Features.Catalog.ToppingGroups.Commands.CreateToppingGroup;
using QRDine.Application.Features.Catalog.ToppingGroups.Commands.DeleteToppingGroup;
using QRDine.Application.Features.Catalog.ToppingGroups.Commands.UpdateToppingGroup;
using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;
using QRDine.Application.Features.Catalog.ToppingGroups.Queries.GetToppingGroupDetail;
using QRDine.Application.Features.Catalog.ToppingGroups.Queries.GetToppingGroupsByPage;
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

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateToppingGroup([FromRoute] Guid id, [FromBody] UpdateToppingGroupRequestDto dto, CancellationToken cancellationToken)
        {
            var command = new UpdateToppingGroupCommand(id, dto);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteToppingGroup([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteToppingGroupCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ToppingGroupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetToppingGroups([FromQuery] GetToppingGroupsByPageQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ToppingGroupDetailDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetToppingGroupDetail([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var query = new GetToppingGroupDetailQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
