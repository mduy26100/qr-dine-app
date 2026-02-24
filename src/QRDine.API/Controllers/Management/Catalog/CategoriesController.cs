using QRDine.API.Constants;
using QRDine.Application.Features.Catalog.Categories.Commands.CreateCategory;
using QRDine.Application.Features.Catalog.Categories.Queries.GetCategoriesByMerchant;
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
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCategories(CancellationToken cancellationToken)
        {
            var query = new GetCategoriesByMerchantQuery();

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
