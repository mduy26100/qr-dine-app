using QRDine.API.Constants;
using QRDine.Application.Common.Models;
using QRDine.Application.Features.Staffs.DTOs;
using QRDine.Application.Features.Staffs.Queries.GetStaffsPaged;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Management.Staffs
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/management/staffs")]
    [Authorize(Roles = SystemRoles.Merchant)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Management)]
    public class StaffsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StaffsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<StaffDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStaffsPaged([FromQuery] GetStaffsPagedQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
