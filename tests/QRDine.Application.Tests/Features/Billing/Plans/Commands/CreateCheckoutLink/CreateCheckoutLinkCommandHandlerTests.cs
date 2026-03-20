namespace QRDine.Application.Tests.Features.Billing.Plans.Commands.CreateCheckoutLink;

public class CreateCheckoutLinkCommandHandlerTests
{
    private readonly Mock<IPlanRepository> _planRepository;
    private readonly Mock<ISubscriptionCheckoutRepository> _checkoutRepo;
    private readonly Mock<ISubscriptionRepository> _subscriptionRepo;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IPayOSService> _payOSService;
    private readonly Mock<IFrontendConfig> _frontendConfig;
    private readonly CreateCheckoutLinkCommandHandler _handler;

    public CreateCheckoutLinkCommandHandlerTests()
    {
        _planRepository = new Mock<IPlanRepository>();
        _checkoutRepo = new Mock<ISubscriptionCheckoutRepository>();
        _subscriptionRepo = new Mock<ISubscriptionRepository>();
        _currentUserService = new Mock<ICurrentUserService>();
        _payOSService = new Mock<IPayOSService>();
        _frontendConfig = new Mock<IFrontendConfig>();

        _handler = new CreateCheckoutLinkCommandHandler(
            _planRepository.Object,
            _checkoutRepo.Object,
            _subscriptionRepo.Object,
            _currentUserService.Object,
            _payOSService.Object,
            _frontendConfig.Object
        );
    }

    [Fact]
    public async Task Handle_ValidPlanWithoutCurrentSubscription_ShouldReturnCheckoutUrl()
    {
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var checkoutUrl = "https://payos.example.com/checkout/abc123";
        var cancellationToken = CancellationToken.None;
        var command = new CreateCheckoutLinkCommand(planId);

        var plan = new Plan 
        { 
            Id = planId, 
            Price = 99000, 
            DurationDays = 30, 
            IsActive = true, 
            Code = "PRO", 
            Name = "Professional Plan" 
        };

        _currentUserService.Setup(x => x.MerchantId).Returns(merchantId);
        _planRepository.Setup(x => x.GetByIdAsync(planId, cancellationToken)).ReturnsAsync(plan);
        _subscriptionRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Subscription>>(), cancellationToken)).ReturnsAsync((Subscription?)null);
        _checkoutRepo
            .Setup(x => x.AddAsync(It.IsAny<SubscriptionCheckout>(), cancellationToken))
            .ReturnsAsync((SubscriptionCheckout c, CancellationToken ct) => c);
        _frontendConfig.Setup(x => x.BaseUrl).Returns("https://app.qrdine.com/");
        _payOSService.Setup(x => x.CreatePaymentLinkAsync(It.IsAny<PaymentLinkRequestDto>())).ReturnsAsync(checkoutUrl);

        var result = await _handler.Handle(command, cancellationToken);

        result.Should().Be(checkoutUrl);
        _payOSService.Verify(x => x.CreatePaymentLinkAsync(It.IsAny<PaymentLinkRequestDto>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PlanNotFound_ShouldThrowException()
    {
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var command = new CreateCheckoutLinkCommand(planId);

        _currentUserService.Setup(x => x.MerchantId).Returns(merchantId);
        _planRepository.Setup(x => x.GetByIdAsync(planId, cancellationToken)).ReturnsAsync((Plan?)null);

        await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task Handle_PlanInactive_ShouldThrowException()
    {
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var command = new CreateCheckoutLinkCommand(planId);

        var inactivePlan = new Plan 
        { 
            Id = planId, 
            Price = 50000, 
            DurationDays = 30, 
            IsActive = false, 
            Code = "LEGACY", 
            Name = "Legacy Plan" 
        };

        _currentUserService.Setup(x => x.MerchantId).Returns(merchantId);
        _planRepository.Setup(x => x.GetByIdAsync(planId, cancellationToken)).ReturnsAsync(inactivePlan);

        await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task Handle_NoMerchantId_ShouldThrowUnauthorizedAccessException()
    {
        var planId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var command = new CreateCheckoutLinkCommand(planId);

        _currentUserService.Setup(x => x.MerchantId).Returns((Guid?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task Handle_WithExistingActiveSubscriptionSamePlan_ShouldUseGiaHanPrefix()
    {
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var checkoutUrl = "https://payos.example.com/checkout/xyz789";
        var cancellationToken = CancellationToken.None;
        var command = new CreateCheckoutLinkCommand(planId);

        var plan = new Plan 
        { 
            Id = planId, 
            Price = 99000, 
            DurationDays = 30, 
            IsActive = true, 
            Code = "PRO", 
            Name = "Professional Plan" 
        };

        var existingSubscription = new Subscription
        {
            MerchantId = merchantId,
            PlanId = planId,
            Status = SubscriptionStatus.Active,
            EndDate = DateTime.UtcNow.AddDays(10)
        };

        _currentUserService.Setup(x => x.MerchantId).Returns(merchantId);
        _planRepository.Setup(x => x.GetByIdAsync(planId, cancellationToken)).ReturnsAsync(plan);
        _subscriptionRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Subscription>>(), cancellationToken)).ReturnsAsync(existingSubscription);
        _checkoutRepo.Setup(x => x.UpdateAsync(It.IsAny<SubscriptionCheckout>(), cancellationToken)).Returns(Task.CompletedTask);
        _frontendConfig.Setup(x => x.BaseUrl).Returns("https://app.qrdine.com/");
        _payOSService.Setup(x => x.CreatePaymentLinkAsync(It.IsAny<PaymentLinkRequestDto>())).ReturnsAsync(checkoutUrl);

        var result = await _handler.Handle(command, cancellationToken);

        result.Should().Be(checkoutUrl);
        _payOSService.Verify(x => x.CreatePaymentLinkAsync(It.Is<PaymentLinkRequestDto>(p => p.Description.Contains("Gia han"))), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingActiveSubscriptionDifferentPlan_ShouldUseNangCapPrefix()
    {
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var oldPlanId = Guid.NewGuid();
        var checkoutUrl = "https://payos.example.com/checkout/pqr456";
        var cancellationToken = CancellationToken.None;
        var command = new CreateCheckoutLinkCommand(planId);

        var plan = new Plan 
        { 
            Id = planId, 
            Price = 199000, 
            DurationDays = 30, 
            IsActive = true, 
            Code = "ENT", 
            Name = "Enterprise Plan" 
        };

        var existingSubscription = new Subscription
        {
            MerchantId = merchantId,
            PlanId = oldPlanId,
            Status = SubscriptionStatus.Active,
            EndDate = DateTime.UtcNow.AddDays(10)
        };

        _currentUserService.Setup(x => x.MerchantId).Returns(merchantId);
        _planRepository.Setup(x => x.GetByIdAsync(planId, cancellationToken)).ReturnsAsync(plan);
        _subscriptionRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Subscription>>(), cancellationToken)).ReturnsAsync(existingSubscription);
        _checkoutRepo.Setup(x => x.UpdateAsync(It.IsAny<SubscriptionCheckout>(), cancellationToken)).Returns(Task.CompletedTask);
        _frontendConfig.Setup(x => x.BaseUrl).Returns("https://app.qrdine.com/");
        _payOSService.Setup(x => x.CreatePaymentLinkAsync(It.IsAny<PaymentLinkRequestDto>())).ReturnsAsync(checkoutUrl);

        var result = await _handler.Handle(command, cancellationToken);

        result.Should().Be(checkoutUrl);
        _payOSService.Verify(x => x.CreatePaymentLinkAsync(It.Is<PaymentLinkRequestDto>(p => p.Description.Contains("Nang cap"))), Times.Once);
    }
}
