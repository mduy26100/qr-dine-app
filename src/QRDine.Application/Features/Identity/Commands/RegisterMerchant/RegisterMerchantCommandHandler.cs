using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Application.Common.Abstractions.Email;
using QRDine.Application.Common.Constants;
using QRDine.Application.Common.Templates;
using QRDine.Application.Features.Identity.Services;

namespace QRDine.Application.Features.Identity.Commands.RegisterMerchant
{
    public class RegisterMerchantCommandHandler : IRequestHandler<RegisterMerchantCommand, bool>
    {
        private readonly IRegisterService _registerService;
        private readonly ICacheService _cacheService;
        private readonly IEmailService _emailService;

        public RegisterMerchantCommandHandler(
            IRegisterService registerService,
            ICacheService cacheService,
            IEmailService emailService)
        {
            _registerService = registerService;
            _cacheService = cacheService;
            _emailService = emailService;
        }

        public async Task<bool> Handle(RegisterMerchantCommand request, CancellationToken cancellationToken)
        {
            await _registerService.ValidateNewMerchantAsync(request.Dto, cancellationToken);

            var token = Guid.NewGuid().ToString("N");

            var cacheKey = RegistrationCacheKeys.GetMerchantRegistrationKey(token);

            await _cacheService.SetAsync(cacheKey, request.Dto, TimeSpan.FromMinutes(15), cancellationToken);

            var verifyLink = _registerService.GenerateActivationLink(token);

            var htmlMessage = EmailTemplates.GetMerchantActivationTemplate(
                request.Dto.FirstName,
                request.Dto.LastName,
                request.Dto.MerchantName,
                verifyLink);

            var subject = "Kích hoạt tài khoản QRDine của bạn";
            await _emailService.SendEmailAsync(request.Dto.Email, subject, htmlMessage, cancellationToken);

            return true;
        }
    }
}
