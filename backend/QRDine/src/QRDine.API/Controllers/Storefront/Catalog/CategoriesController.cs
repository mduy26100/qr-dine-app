using QRDine.Application.Features.Catalog.Categories.Queries.GetCategoriesByMerchant;

namespace QRDine.API.Controllers.Storefront.Catalog
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/storefront/merchants/{merchantId:guid}/categories")]
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
    }
}
