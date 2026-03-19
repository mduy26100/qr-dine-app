namespace QRDine.Application.Features.Billing.Plans.Commands.CreateCheckoutLink
{
    public record CreateCheckoutLinkCommand(Guid PlanId) : IRequest<string>;
}
