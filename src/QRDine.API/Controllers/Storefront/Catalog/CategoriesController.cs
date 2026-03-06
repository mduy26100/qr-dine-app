using QRDine.API.Constants;
using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Queries.GetCategoriesByMerchant;
using QRDine.Application.Features.Catalog.Categories.Queries.GetStorefrontMenu;

namespace QRDine.API.Controllers.Storefront.Catalog
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/storefront/merchants/{merchantId:guid}/categories")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Storefront)]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesByMerchant(Guid merchantId, CancellationToken cancellationToken)
        {
            var query = new GetCategoriesByMerchantQuery(merchantId);

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("menu")]
        [ProducesResponseType(typeof(List<StorefrontMenuCategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenu([FromRoute] Guid merchantId, CancellationToken cancellationToken)
        {
            var query = new GetStorefrontMenuQuery(merchantId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
