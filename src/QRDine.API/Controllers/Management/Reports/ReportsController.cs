using QRDine.API.Attributes;
using QRDine.API.Constants;
using QRDine.Application.Features.Reports.DTOs;
using QRDine.Application.Features.Reports.Queries.GetRevenueSummary;
using QRDine.Application.Features.Reports.Queries.GetRevenueChart;
using QRDine.Domain.Enums;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Reports
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/reports")]
    [Authorize(Roles = SystemRoles.Merchant)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("revenue-summary")]
        [CheckFeatureLimit(FeatureType.AdvancedReports)]
        [ProducesResponseType(typeof(RevenueSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
        {
            var query = new GetRevenueSummaryQuery(startDate ,endDate);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("revenue-chart")]
        [CheckFeatureLimit(FeatureType.AdvancedReports)]
        [ProducesResponseType(typeof(IEnumerable<RevenueChartItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueChart([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] RevenueChartGrouping grouping, CancellationToken cancellationToken)
        {
            var query = new GetRevenueChartQuery(startDate, endDate, grouping);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
