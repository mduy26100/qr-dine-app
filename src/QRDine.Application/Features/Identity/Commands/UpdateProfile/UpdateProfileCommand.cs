using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Commands.UpdateProfile
{
    public record UpdateProfileCommand(UpdateProfileRequestDto Dto) : IRequest<Unit>;
}
