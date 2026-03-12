using QRDine.API.Constants;
using QRDine.Application.Features.Dashboards.DTOs;
using QRDine.Application.Features.Dashboards.Queries.GetDashboardSummary;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Dashboard
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/dashboard")]
    [Authorize(Roles = SystemRoles.Merchant)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
        {
            var query = new GetDashboardSummaryQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
