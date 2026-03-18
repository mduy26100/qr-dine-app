using QRDine.Application.Features.Billing.Plans.Commands.ProcessPaymentWebhook;

namespace QRDine.API.Controllers.Webhooks
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/webhooks")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PayOSWebhookController : ControllerBase
    {
        private readonly PayOSClient _payOSClient;
        private readonly IMediator _mediator;
        private readonly ILogger<PayOSWebhookController> _logger;

        public PayOSWebhookController(PayOSClient payOSClient, IMediator mediator, ILogger<PayOSWebhookController> logger)
        {
            _payOSClient = payOSClient;
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("payos")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook([FromBody] Webhook webhookBody)
        {
            try
            {
                var webhookData = await _payOSClient.Webhooks.VerifyAsync(webhookBody);

                if (webhookData.Code == "00")
                {
                    var command = new ProcessPaymentWebhookCommand(
                        webhookData.OrderCode,
                        webhookData.Amount,
                        webhookData.Reference);

                    await _mediator.Send(command);
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý PayOS Webhook. Data: {@WebhookBody}", webhookBody);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
