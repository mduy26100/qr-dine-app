namespace QRDine.Application.Tests.Features.Billing.FeatureLimits.Services
{
    public class FeatureLimitServiceTests
    {
        private readonly Mock<IFeatureLimitRepository> _featureLimitRepo;
        private readonly Mock<ITableRepository> _tableRepo;
        private readonly Mock<IProductRepository> _productRepo;
        private readonly Mock<IIdentityService> _identityService;
        private readonly Mock<ICacheService> _cacheService;
        private readonly FeatureLimitService _service;

        public FeatureLimitServiceTests()
        {
            _featureLimitRepo = new Mock<IFeatureLimitRepository>();
            _tableRepo = new Mock<ITableRepository>();
            _productRepo = new Mock<IProductRepository>();
            _identityService = new Mock<IIdentityService>();
            _cacheService = new Mock<ICacheService>();

            _service = new FeatureLimitService(
                _featureLimitRepo.Object,
                _tableRepo.Object,
                _productRepo.Object,
                _identityService.Object,
                _cacheService.Object
            );
        }

        [Fact]
        public async Task CheckLimitAsync_MaxTables_WithinLimit_ShouldPass()
        {
            var merchantId = Guid.NewGuid();
            var planCode = "PRO";
            var cancellationToken = CancellationToken.None;

            var limits = new FeatureLimitCheckDtoBuilder().WithMaxTables(10).Build();

            _cacheService
                .Setup(x => x.GetAsync<FeatureLimitCheckDto>(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync((FeatureLimitCheckDto?)null);

            _featureLimitRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<FeatureLimit, FeatureLimitCheckDto>>(), cancellationToken))
                .ReturnsAsync(limits);

            _tableRepo
                .Setup(x => x.CountAsync(cancellationToken))
                .ReturnsAsync(5);

            _cacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), cancellationToken))
                .Returns(Task.CompletedTask);

            await _service.CheckLimitAsync(merchantId, planCode, FeatureType.MaxTables, cancellationToken);
        }

        [Fact]
        public async Task CheckLimitAsync_MaxTables_ExceedsLimit_ShouldThrow()
        {
            var merchantId = Guid.NewGuid();
            var planCode = "PRO";
            var cancellationToken = CancellationToken.None;

            var limits = new FeatureLimitCheckDtoBuilder().WithMaxTables(5).Build();

            _cacheService
                .Setup(x => x.GetAsync<FeatureLimitCheckDto>(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync((FeatureLimitCheckDto?)null);

            _featureLimitRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<FeatureLimit, FeatureLimitCheckDto>>(), cancellationToken))
                .ReturnsAsync(limits);

            _tableRepo
                .Setup(x => x.CountAsync(cancellationToken))
                .ReturnsAsync(5);

            _cacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), cancellationToken))
                .Returns(Task.CompletedTask);

            await Assert.ThrowsAsync<ConflictException>(
                () => _service.CheckLimitAsync(merchantId, planCode, FeatureType.MaxTables, cancellationToken)
            );
        }

        [Fact]
        public async Task CheckLimitAsync_MaxProducts_WithinLimit_ShouldPass()
        {
            var merchantId = Guid.NewGuid();
            var planCode = "PRO";
            var cancellationToken = CancellationToken.None;

            var limits = new FeatureLimitCheckDtoBuilder().WithMaxProducts(100).Build();

            _cacheService
                .Setup(x => x.GetAsync<FeatureLimitCheckDto>(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync((FeatureLimitCheckDto?)null);

            _featureLimitRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<FeatureLimit, FeatureLimitCheckDto>>(), cancellationToken))
                .ReturnsAsync(limits);

            _productRepo
                .Setup(x => x.CountAsync(cancellationToken))
                .ReturnsAsync(50);

            _cacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), cancellationToken))
                .Returns(Task.CompletedTask);

            await _service.CheckLimitAsync(merchantId, planCode, FeatureType.MaxProducts, cancellationToken);
        }

        [Fact]
        public async Task CheckLimitAsync_AdvancedReports_NotAllowed_ShouldThrow()
        {
            var merchantId = Guid.NewGuid();
            var planCode = "BASIC";
            var cancellationToken = CancellationToken.None;

            var limits = new FeatureLimitCheckDtoBuilder().WithAllowAdvancedReports(false).Build();

            _cacheService
                .Setup(x => x.GetAsync<FeatureLimitCheckDto>(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync((FeatureLimitCheckDto?)null);

            _featureLimitRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<FeatureLimit, FeatureLimitCheckDto>>(), cancellationToken))
                .ReturnsAsync(limits);

            _cacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), cancellationToken))
                .Returns(Task.CompletedTask);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => _service.CheckLimitAsync(merchantId, planCode, FeatureType.AdvancedReports, cancellationToken)
            );
        }

        [Fact]
        public async Task CheckLimitAsync_CachedLimits_ShouldNotQueryRepository()
        {
            var merchantId = Guid.NewGuid();
            var planCode = "PRO";
            var cancellationToken = CancellationToken.None;

            var cachedLimits = new FeatureLimitCheckDtoBuilder().WithMaxTables(10).Build();

            _cacheService
                .Setup(x => x.GetAsync<FeatureLimitCheckDto>(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync(cachedLimits);

            _tableRepo
                .Setup(x => x.CountAsync(cancellationToken))
                .ReturnsAsync(5);

            await _service.CheckLimitAsync(merchantId, planCode, FeatureType.MaxTables, cancellationToken);

            _featureLimitRepo.Verify(
                x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<FeatureLimit, FeatureLimitCheckDto>>(), cancellationToken),
                Times.Never
            );
        }

        [Fact]
        public async Task CheckLimitAsync_LimitNotFound_ShouldThrowNotFound()
        {
            var merchantId = Guid.NewGuid();
            var planCode = "NONEXISTENT";
            var cancellationToken = CancellationToken.None;

            _cacheService
                .Setup(x => x.GetAsync<FeatureLimitCheckDto>(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync((FeatureLimitCheckDto?)null);

            _featureLimitRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<FeatureLimit, FeatureLimitCheckDto>>(), cancellationToken))
                .ReturnsAsync((FeatureLimitCheckDto?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.CheckLimitAsync(merchantId, planCode, FeatureType.MaxTables, cancellationToken)
            );
        }
    }
}
