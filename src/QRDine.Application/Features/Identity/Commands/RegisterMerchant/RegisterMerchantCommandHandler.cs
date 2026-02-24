using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;

namespace QRDine.Application.Features.Identity.Commands.RegisterMerchant
{
    public class RegisterMerchantCommandHandler : IRequestHandler<RegisterMerchantCommand, RegisterResponseDto>
    {
        private readonly IRegisterService _registerService;

        public RegisterMerchantCommandHandler(IRegisterService registerService)
        {
            _registerService = registerService;
        }

        public async Task<RegisterResponseDto> Handle(RegisterMerchantCommand request, CancellationToken cancellationToken)
        {
            var response = await _registerService.RegisterMerchantAsync(request.Dto, cancellationToken);
            return response;
        }
    }
}
