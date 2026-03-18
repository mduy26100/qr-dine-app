using QRDine.API.Constants;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.API.Controllers.Admin
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/payment-gateways")]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    [ApiExplorerSettings(GroupName = SwaggerGroups.Admin)]
    public class PaymentGatewaysController : ControllerBase
    {
        private readonly PayOSClient _payOSClient;

        public PaymentGatewaysController(PayOSClient payOSClient)
        {
            _payOSClient = payOSClient;
        }

        [HttpPost("payos/webhook-setup")]
        public async Task<IActionResult> SetupPayOSWebhook([FromBody] SetupWebhookRequest request)
        {
            try
            {
                var webhookUrl = await _payOSClient.Webhooks.ConfirmAsync(request.WebhookUrl);
                return Ok(new { success = true, registeredUrl = webhookUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class SetupWebhookRequest
    {
        public string WebhookUrl { get; set; } = default!;
    }
}
