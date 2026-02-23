using QRDine.Application.Features.Catalog.Categories.Commands.CreateCategory;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/categories")]
    [Authorize(Roles = SystemRoles.Merchant)]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, result);
        }
    }
}
