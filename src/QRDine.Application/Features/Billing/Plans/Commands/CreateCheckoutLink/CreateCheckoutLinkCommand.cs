using QRDine.Application.Features.Billing.Plans.DTOs;

namespace QRDine.Application.Features.Billing.Plans.Commands.CreateCheckoutLink
{
    public record CreateCheckoutLinkCommand(Guid PlanId, CreateCheckoutDto Dto) : IRequest<string>;
}
