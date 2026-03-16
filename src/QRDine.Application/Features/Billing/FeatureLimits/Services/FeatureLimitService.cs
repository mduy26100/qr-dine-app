using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Constants;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Billing.FeatureLimits.DTOs;
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
        private readonly IIdentityService _identityService;
        private readonly ICacheService _cacheService;

        public FeatureLimitService(
            IFeatureLimitRepository featureLimitRepository,
            ITableRepository tableRepository,
            IProductRepository productRepository,
            IIdentityService identityService,
            ICacheService cacheService)
        {
            _featureLimitRepository = featureLimitRepository;
            _tableRepository = tableRepository;
            _productRepository = productRepository;
            _identityService = identityService;
            _cacheService = cacheService;
        }

        public async Task CheckLimitAsync(Guid merchantId, string planCode, FeatureType featureType, CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.FeatureLimit(planCode);

            var limits = await _cacheService.GetAsync<FeatureLimitCheckDto>(cacheKey, cancellationToken);

            if (limits == null)
            {
                var spec = new GetFeatureLimitByPlanCodeSpec(planCode);
                limits = await _featureLimitRepository.SingleOrDefaultAsync(spec, cancellationToken);

                if (limits == null)
                    throw new NotFoundException("Không tìm thấy cấu hình giới hạn cho gói cước này.");

                await _cacheService.SetAsync(cacheKey, limits, CacheDurations.FeatureLimits, cancellationToken);
            }

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

                case FeatureType.MaxStaffMembers:
                    if (limits.MaxStaffMembers.HasValue)
                    {
                        var currentCount = await _identityService.CountStaffByMerchantAsync(merchantId, cancellationToken);

                        if (currentCount >= limits.MaxStaffMembers.Value)
                            throw new ConflictException($"Gói cước hiện tại giới hạn tối đa {limits.MaxStaffMembers.Value} nhân viên. Vui lòng nâng cấp gói cước để tạo thêm.");
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
