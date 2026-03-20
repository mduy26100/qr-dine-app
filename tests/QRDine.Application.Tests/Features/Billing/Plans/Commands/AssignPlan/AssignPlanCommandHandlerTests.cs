namespace QRDine.Application.Tests.Features.Billing.Plans.Commands.AssignPlan
{
    public class AssignPlanCommandHandlerTests
    {
        private readonly Mock<ISubscriptionService> _subscriptionService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IApplicationDbContext> _context;
        private readonly Mock<IDatabaseTransaction> _transaction;
        private readonly AssignPlanCommandHandler _handler;

        public AssignPlanCommandHandlerTests()
        {
            _subscriptionService = new Mock<ISubscriptionService>();
            _mapper = new Mock<IMapper>();
            _context = new Mock<IApplicationDbContext>();
            _transaction = new Mock<IDatabaseTransaction>();

            _context
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_transaction.Object);

            _handler = new AssignPlanCommandHandler(_subscriptionService.Object, _mapper.Object, _context.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldAssignPlanSuccessfully()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var command = new AssignPlanCommand(merchantId, new AssignPlanDto { PlanId = planId });
            var cancellationToken = CancellationToken.None;

            var subscription = new Subscription
            {
                Id = subscriptionId,
                MerchantId = merchantId,
                PlanId = planId,
                Status = SubscriptionStatus.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30)
            };

            var responseDto = new AssignPlanResponseDto
            {
                SubscriptionId = subscriptionId,
                MerchantId = merchantId,
                PlanId = planId,
                Status = SubscriptionStatus.Active.ToString(),
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate
            };

            _subscriptionService
                .Setup(x => x.AssignPlanAsync(
                    merchantId,
                    planId,
                    PaymentMethod.System_Grant,
                    0,
                    "Super Admin cấp gói thủ công",
                    cancellationToken
                ))
                .ReturnsAsync(subscription);

            _context
                .Setup(x => x.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            _mapper
                .Setup(x => x.Map<AssignPlanResponseDto>(subscription))
                .Returns(responseDto);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().NotBeNull();
            result.SubscriptionId.Should().Be(subscriptionId);
            _transaction.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_WithAdminNote_ShouldPassNoteToService()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var adminNote = "Custom admin note";
            var command = new AssignPlanCommand(merchantId, new AssignPlanDto { PlanId = planId, AdminNote = adminNote });
            var cancellationToken = CancellationToken.None;

            var subscription = new Subscription { Id = subscriptionId, MerchantId = merchantId, PlanId = planId };
            var responseDto = new AssignPlanResponseDto { SubscriptionId = subscriptionId };

            _subscriptionService
                .Setup(x => x.AssignPlanAsync(merchantId, planId, PaymentMethod.System_Grant, 0, adminNote, cancellationToken))
                .ReturnsAsync(subscription);

            _context.Setup(x => x.SaveChangesAsync(cancellationToken)).ReturnsAsync(1);
            _mapper.Setup(x => x.Map<AssignPlanResponseDto>(subscription)).Returns(responseDto);

            await _handler.Handle(command, cancellationToken);

            _subscriptionService.Verify(
                x => x.AssignPlanAsync(merchantId, planId, PaymentMethod.System_Grant, 0, adminNote, cancellationToken),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ServiceThrows_ShouldRollbackTransaction()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var command = new AssignPlanCommand(merchantId, new AssignPlanDto { PlanId = planId });
            var cancellationToken = CancellationToken.None;

            _subscriptionService
                .Setup(x => x.AssignPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PaymentMethod>(), It.IsAny<decimal?>(), It.IsAny<string>(), cancellationToken))
                .ThrowsAsync(new NotFoundException("Plan not found"));

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, cancellationToken));

            _transaction.Verify(x => x.RollbackAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_SaveChangesThrows_ShouldRollbackTransaction()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var command = new AssignPlanCommand(merchantId, new AssignPlanDto { PlanId = planId });
            var cancellationToken = CancellationToken.None;

            _subscriptionService
                .Setup(x => x.AssignPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PaymentMethod>(), It.IsAny<decimal?>(), It.IsAny<string>(), cancellationToken))
                .ReturnsAsync(new Subscription());

            _context
                .Setup(x => x.SaveChangesAsync(cancellationToken))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, cancellationToken));

            _transaction.Verify(x => x.RollbackAsync(cancellationToken), Times.Once);
        }
    }
}
