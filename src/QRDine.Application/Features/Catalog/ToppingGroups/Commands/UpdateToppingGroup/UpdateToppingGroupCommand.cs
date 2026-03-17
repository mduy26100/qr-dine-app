using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Commands.UpdateToppingGroup
{
    public record UpdateToppingGroupCommand(Guid Id, UpdateToppingGroupRequestDto Data) : IRequest<Unit>;
}
