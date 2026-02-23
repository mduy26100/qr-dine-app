using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;

namespace QRDine.Application.Features.Identity.Commands.RegisterStaff
{
    internal class RegisterStaffCommandHandler : IRequestHandler<RegisterStaffCommand, RegisterResponseDto>
    {
        private readonly IRegisterService _registerService;
        private readonly ICurrentUserService _currentUserService;

        public RegisterStaffCommandHandler(IRegisterService registerService, ICurrentUserService currentUserService)
        {
            _registerService = registerService;
            _currentUserService = currentUserService;
        }

        public async Task<RegisterResponseDto> Handle(RegisterStaffCommand request, CancellationToken cancellationToken)
        {
            var currentMerchantId = _currentUserService.MerchantId
                ?? throw new UnauthorizedAccessException("You don't belong to any store.");

            return await _registerService.RegisterStaffAsync(request.Dto, currentMerchantId, cancellationToken);
        }
    }
}
