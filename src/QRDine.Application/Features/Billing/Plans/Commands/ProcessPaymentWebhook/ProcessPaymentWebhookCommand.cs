namespace QRDine.Application.Features.Billing.Plans.Commands.ProcessPaymentWebhook
{
    public record ProcessPaymentWebhookCommand(long OrderCode, long Amount, string Reference) : IRequest<bool>;
}
