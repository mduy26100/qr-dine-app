using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Commands.CreateToppingGroup
{
    public record CreateToppingGroupCommand(CreateToppingGroupRequestDto Data) : IRequest<ToppingGroupResponseDto>;
}
