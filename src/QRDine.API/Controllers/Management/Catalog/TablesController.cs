using QRDine.API.Constants;
using QRDine.Application.Features.Catalog.Tables.Commands.CreateTable;
using QRDine.Application.Features.Catalog.Tables.Commands.DeleteTable;
using QRDine.Application.Features.Catalog.Tables.Commands.UpdateTable;
using QRDine.Application.Features.Catalog.Tables.DTOs;
using QRDine.Application.Features.Catalog.Tables.Queries.GetMyTables;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Catalog
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/tables")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class TablesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TablesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = SystemRoles.Merchant)]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Created(string.Empty, result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = SystemRoles.Merchant)]
        public async Task<IActionResult> UpdateTable([FromRoute] Guid id, [FromBody] UpdateTableDto dto, CancellationToken cancellationToken)
        {
            var command = new UpdateTableCommand(id, dto);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = SystemRoles.Merchant)]
        public async Task<IActionResult> DeleteTable([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteTableCommand(id);
            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = $"{SystemRoles.Merchant},{SystemRoles.Staff}")]
        [ProducesResponseType(typeof(List<TableResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyTables([FromQuery] bool? isOccupied, CancellationToken cancellationToken)
        {
            var query = new GetMyTablesQuery(isOccupied);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
