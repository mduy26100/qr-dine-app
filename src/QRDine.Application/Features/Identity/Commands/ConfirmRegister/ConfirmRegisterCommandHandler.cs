using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Application.Common.Constants;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;

namespace QRDine.Application.Features.Identity.Commands.ConfirmRegister
{
    public class ConfirmRegisterCommandHandler : IRequestHandler<ConfirmRegisterCommand, RegisterResponseDto>
    {
        private readonly ICacheService _cacheService;
        private readonly IRegisterService _registerService;

        public ConfirmRegisterCommandHandler(
            ICacheService cacheService,
            IRegisterService registerService)
        {
            _cacheService = cacheService;
            _registerService = registerService;
        }

        public async Task<RegisterResponseDto> Handle(ConfirmRegisterCommand request, CancellationToken cancellationToken)
        {
            var cacheKey = RegistrationCacheKeys.GetMerchantRegistrationKey(request.Token);

            var cachedDto = await _cacheService.GetAsync<RegisterMerchantDto>(cacheKey, cancellationToken);

            if (cachedDto == null)
            {
                throw new BadRequestException("Link kích hoạt đã hết hạn hoặc không hợp lệ. Vui lòng đăng ký lại.");
            }

            var response = await _registerService.ConfirmMerchantRegistrationAsync(cachedDto, cancellationToken);

            await _cacheService.RemoveAsync(cacheKey, cancellationToken);

            return response;
        }
    }
}
