using QRDine.API.Constants;
using QRDine.Application.Features.Catalog.Categories.Commands.CreateCategory;
using QRDine.Application.Features.Catalog.Categories.Commands.DeleteCategory;
using QRDine.Application.Features.Catalog.Categories.Commands.UpdateCategory;
using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Queries.GetMyCategories;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Catalog
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/categories")]
    [Authorize(Roles = SystemRoles.Merchant)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken)
        {
            var command = new CreateCategoryCommand(dto);
            var result = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCategories(CancellationToken cancellationToken)
        {
            var query = new GetMyCategoriesQuery();

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken cancellationToken)
        {
            var command = new UpdateCategoryCommand(id, dto);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteCategoryCommand(id);
            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}
