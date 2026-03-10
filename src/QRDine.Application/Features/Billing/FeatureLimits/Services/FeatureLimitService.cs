using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Billing.FeatureLimits.Specifications;
using QRDine.Application.Features.Billing.Repositories;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.FeatureLimits.Services
{
    public class FeatureLimitService : IFeatureLimitService
    {
        private readonly IFeatureLimitRepository _featureLimitRepository;
        private readonly ITableRepository _tableRepository;
        private readonly IProductRepository _productRepository;

        public FeatureLimitService(
            IFeatureLimitRepository featureLimitRepository,
            ITableRepository tableRepository,
            IProductRepository productRepository)
        {
            _featureLimitRepository = featureLimitRepository;
            _tableRepository = tableRepository;
            _productRepository = productRepository;
        }

        public async Task CheckLimitAsync(Guid merchantId, string planCode, FeatureType featureType, CancellationToken cancellationToken = default)
        {
            var spec = new GetFeatureLimitByPlanCodeSpec(planCode);
            var limits = await _featureLimitRepository.SingleOrDefaultAsync(spec, cancellationToken);

            if (limits == null)
                throw new NotFoundException("Không tìm thấy cấu hình giới hạn cho gói cước này.");

            switch (featureType)
            {
                case FeatureType.MaxTables:
                    if (limits.MaxTables.HasValue)
                    {
                        var currentCount = await _tableRepository.CountAsync(cancellationToken);

                        if (currentCount >= limits.MaxTables.Value)
                            throw new ConflictException($"Gói cước hiện tại giới hạn tối đa {limits.MaxTables.Value} bàn. Vui lòng nâng cấp gói cước để tạo thêm.");
                    }
                    break;

                case FeatureType.MaxProducts:
                    if (limits.MaxProducts.HasValue)
                    {
                        var currentCount = await _productRepository.CountAsync(cancellationToken);

                        if (currentCount >= limits.MaxProducts.Value)
                            throw new ConflictException($"Gói cước hiện tại giới hạn tối đa {limits.MaxProducts.Value} món ăn. Vui lòng nâng cấp gói cước.");
                    }
                    break;

                case FeatureType.AdvancedReports:
                    if (!limits.AllowAdvancedReports)
                        throw new ForbiddenException("Tính năng Báo cáo nâng cao không có trong gói cước của bạn. Vui lòng nâng cấp.");
                    break;
            }
        }
    }
}
