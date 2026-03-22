using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Queries.Profile
{
    public record ProfileQuery() : IRequest<UserProfileDto>;
}
