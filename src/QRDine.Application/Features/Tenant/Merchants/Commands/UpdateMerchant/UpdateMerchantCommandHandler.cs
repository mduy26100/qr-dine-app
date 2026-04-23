using QRDine.Application.Common.Abstractions.ExternalServices.FileUpload;
using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Tenant.Repositories;

namespace QRDine.Application.Features.Tenant.Merchants.Commands.UpdateMerchant
{
    public class UpdateMerchantCommandHandler : IRequestHandler<UpdateMerchantCommand, Unit>
    {
        private readonly IMerchantRepository _merchantRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileUploadService _fileUploadService;

        public UpdateMerchantCommandHandler(IMerchantRepository merchantRepository
            , ICurrentUserService currentUserService
            , IFileUploadService fileUploadService)
        {
            _merchantRepository = merchantRepository;
            _currentUserService = currentUserService;
            _fileUploadService = fileUploadService;
        }

        public async Task<Unit> Handle(UpdateMerchantCommand request, CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId
                ?? throw new UnauthorizedAccessException("Merchant context is missing.");

            var merchant = await _merchantRepository.GetByIdAsync(merchantId, cancellationToken);
            if (merchant == null)
            {
                throw new NotFoundException($"Không tìm thấy cửa hàng với Id {merchantId}.");
            }

            if (request.Dto.ImgContent != null && !string.IsNullOrWhiteSpace(request.Dto.ImgFileName))
            {
                var uploadRequest = new FileUploadRequest
                {
                    Content = request.Dto.ImgContent,
                    FileName = request.Dto.ImgFileName,
                };

                merchant.LogoUrl = await _fileUploadService.UploadAsync(uploadRequest, cancellationToken);
            }

            merchant.Name = request.Dto.Name;
            merchant.Slug = request.Dto.Slug;
            merchant.Address = request.Dto.Address;
            merchant.PhoneNumber = request.Dto.PhoneNumber;

            await _merchantRepository.UpdateAsync(merchant, cancellationToken);
            return Unit.Value;
        }
    }
}
