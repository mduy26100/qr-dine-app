namespace QRDine.Application.Tests.Features.Billing.Subscriptions.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<IPlanRepository> _planRepo = new();
        private readonly Mock<ISubscriptionRepository> _subRepo = new();
        private readonly Mock<IMerchantRepository> _merchantRepo = new();
        private readonly Mock<ITransactionRepository> _transRepo = new();
        private readonly Mock<IApplicationDbContext> _context = new();
        private readonly Mock<ICacheService> _cache = new();

        private readonly SubscriptionService _service;

        public SubscriptionServiceTests()
        {
            _service = new SubscriptionService(
                _planRepo.Object,
                _subRepo.Object,
                _merchantRepo.Object,
                _transRepo.Object,
                _context.Object,
                _cache.Object
            );
        }

        [Fact]
        public async Task AssignPlanAsync_NewSubscription_ShouldCreate()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();

            var merchant = new Merchant { Id = merchantId };
            var plan = new Plan { Id = planId, DurationDays = 30, Price = 100000, IsActive = true };

            _merchantRepo.Setup(x => x.GetByIdAsync(merchantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);

            _planRepo.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            _subRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Subscription>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            var result = await _service.AssignPlanAsync(merchantId, planId, PaymentMethod.System_Grant);

            result.Should().NotBeNull();
            result.PlanId.Should().Be(planId);
            result.Status.Should().Be(SubscriptionStatus.Active);

            _subRepo.Verify(x => x.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
            _transRepo.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssignPlanAsync_SamePlan_Active_ShouldExtend()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var merchant = new Merchant { Id = merchantId };
            var plan = new Plan { Id = planId, DurationDays = 30, Price = 100000, IsActive = true };

            var sub = new Subscription
            {
                MerchantId = merchantId,
                PlanId = planId,
                EndDate = now.AddDays(10)
            };

            _merchantRepo.Setup(x => x.GetByIdAsync(merchantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);

            _planRepo.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            _subRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Subscription>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sub);

            var result = await _service.AssignPlanAsync(merchantId, planId, PaymentMethod.System_Grant);

            result.EndDate.Should().BeAfter(now.AddDays(10));

            _subRepo.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssignPlanAsync_SamePlan_Expired_ShouldResetFromNow()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var merchant = new Merchant { Id = merchantId };
            var plan = new Plan { Id = planId, DurationDays = 30, Price = 100000, IsActive = true };

            var sub = new Subscription
            {
                MerchantId = merchantId,
                PlanId = planId,
                EndDate = now.AddDays(-5)
            };

            _merchantRepo.Setup(x => x.GetByIdAsync(merchantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);

            _planRepo.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            _subRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Subscription>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sub);

            var result = await _service.AssignPlanAsync(merchantId, planId, PaymentMethod.System_Grant);

            result.EndDate.Should().BeAfter(now);

            _subRepo.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssignPlanAsync_DifferentPlan_ShouldReplace()
        {
            var merchantId = Guid.NewGuid();
            var oldPlan = Guid.NewGuid();
            var newPlan = Guid.NewGuid();

            var merchant = new Merchant { Id = merchantId };
            var plan = new Plan { Id = newPlan, DurationDays = 30, Price = 200000, IsActive = true };

            var sub = new Subscription
            {
                MerchantId = merchantId,
                PlanId = oldPlan
            };

            _merchantRepo.Setup(x => x.GetByIdAsync(merchantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);

            _planRepo.Setup(x => x.GetByIdAsync(newPlan, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            _subRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Subscription>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sub);

            var result = await _service.AssignPlanAsync(merchantId, newPlan, PaymentMethod.System_Grant);

            result.PlanId.Should().Be(newPlan);

            _subRepo.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssignPlanAsync_MerchantNotFound_ShouldThrow()
        {
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.AssignPlanAsync(Guid.NewGuid(), Guid.NewGuid(), PaymentMethod.System_Grant));
        }

        [Fact]
        public async Task AssignPlanAsync_PlanNotFound_ShouldThrow()
        {
            var merchant = new Merchant { Id = Guid.NewGuid() };

            _merchantRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.AssignPlanAsync(merchant.Id, Guid.NewGuid(), PaymentMethod.System_Grant));
        }

        [Fact]
        public async Task AssignPlanAsync_ShouldInvalidateCache()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();

            var merchant = new Merchant { Id = merchantId };
            var plan = new Plan { Id = planId, DurationDays = 30, Price = 100000, IsActive = true };

            _merchantRepo.Setup(x => x.GetByIdAsync(merchantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);

            _planRepo.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            _subRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Subscription>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            await _service.AssignPlanAsync(merchantId, planId, PaymentMethod.System_Grant);

            _cache.Verify(x => x.RemoveAsync(CacheKeys.MerchantActiveStatus(merchantId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task IsSubscriptionActiveAsync_NoCache_ShouldQueryAndCache()
        {
            var merchantId = Guid.NewGuid();

            _cache.Setup(x => x.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((bool?)null);

            _subRepo.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Subscription>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.IsSubscriptionActiveAsync(merchantId);

            result.Should().BeTrue();

            _cache.Verify(x => x.SetAsync(
                CacheKeys.MerchantActiveStatus(merchantId),
                true,
                CacheDurations.MerchantActiveStatus,
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task IsSubscriptionActiveAsync_WithCache_ShouldReturnCache()
        {
            _cache.Setup(x => x.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.IsSubscriptionActiveAsync(Guid.NewGuid());

            result.Should().BeTrue();

            _subRepo.Verify(x => x.AnyAsync(It.IsAny<ISpecification<Subscription>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}