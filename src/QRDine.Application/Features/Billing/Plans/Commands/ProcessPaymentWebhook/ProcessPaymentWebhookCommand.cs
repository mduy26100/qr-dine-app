namespace QRDine.Application.Features.Billing.Plans.Commands.ProcessPaymentWebhook
{
    public record ProcessPaymentWebhookCommand(long OrderCode, string Reference) : IRequest<bool>;
}
