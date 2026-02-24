using QRDine.API.Constants;
using QRDine.API.Requests.Catalog;
using QRDine.Application.Features.Catalog.Products.Commands.CreateProduct;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Catalog
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/products")]
    [Authorize(Roles = SystemRoles.Merchant)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductForm form, CancellationToken cancellation)
        {
            var command = new CreateProductCommand(form.ToDto());
            var result = await _mediator.Send(command, cancellation);
            return Created(string.Empty, result);
        }
    }
}
