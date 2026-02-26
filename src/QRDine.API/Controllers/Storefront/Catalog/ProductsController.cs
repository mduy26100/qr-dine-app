using QRDine.API.Constants;
using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Queries.GetProductsByCategory;

namespace QRDine.API.Controllers.Storefront.Catalog
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/storefront/merchants/{merchantId:guid}/products")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Storefront)]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProductForStorefrontDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductsByCategory(
            [FromRoute] Guid merchantId,
            [FromQuery] Guid categoryId,
            CancellationToken cancellationToken)
        {
            var query = new GetProductsByCategoryQuery
            {
                MerchantId = merchantId,
                CategoryId = categoryId
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
