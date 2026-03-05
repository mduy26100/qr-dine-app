using QRDine.API.Constants;
using QRDine.Application.Features.Catalog.Tables.Queries.GetStorefrontTableInfo;

namespace QRDine.API.Controllers.Storefront.Catalog
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/storefront/merchants/{merchantId:guid}/tables")]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Storefront)]
    public class TablesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TablesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("info")]
        public async Task<IActionResult> TableInfo(Guid merchantId, [FromQuery] string qrCodeToken, CancellationToken cancellationToken)
        {
            var query = new GetStorefrontTableInfoQuery(merchantId, qrCodeToken);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
