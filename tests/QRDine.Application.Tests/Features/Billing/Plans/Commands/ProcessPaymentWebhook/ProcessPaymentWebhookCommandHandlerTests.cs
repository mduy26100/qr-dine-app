namespace QRDine.Application.Tests.Features.Billing.Plans.Commands.ProcessPaymentWebhook
{
    public class ProcessPaymentWebhookCommandHandlerTests
    {
        private readonly Mock<ISubscriptionCheckoutRepository> _checkoutRepo;
        private readonly Mock<ISubscriptionService> _subscriptionService;
        private readonly Mock<IApplicationDbContext> _context;
        private readonly Mock<IDatabaseTransaction> _transaction;
        private readonly ProcessPaymentWebhookCommandHandler _handler;

        public ProcessPaymentWebhookCommandHandlerTests()
        {
            _checkoutRepo = new Mock<ISubscriptionCheckoutRepository>();
            _subscriptionService = new Mock<ISubscriptionService>();
            _context = new Mock<IApplicationDbContext>();
            _transaction = new Mock<IDatabaseTransaction>();

            _context
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_transaction.Object);

            _handler = new ProcessPaymentWebhookCommandHandler(_checkoutRepo.Object, _subscriptionService.Object, _context.Object);
        }

        [Fact]
        public async Task Handle_ValidPaymentCorrectAmount_ShouldProcessSuccessfully()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var orderCode = 202401011234567L;
            var amount = 99000L;
            var reference = "REF123456";
            var command = new ProcessPaymentWebhookCommand(orderCode, amount, reference);
            var cancellationToken = CancellationToken.None;

            var checkout = new SubscriptionCheckout
            {
                OrderCode = orderCode,
                MerchantId = merchantId,
                PlanId = planId,
                Amount = 99000m,
                Status = PaymentStatus.Pending
            };

            var subscription = new Subscription { MerchantId = merchantId, PlanId = planId };

            _checkoutRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<SubscriptionCheckout>>(), cancellationToken))
                .ReturnsAsync(checkout);

            _subscriptionService
                .Setup(x => x.AssignPlanAsync(merchantId, planId, PaymentMethod.BankTransfer, 99000m, It.IsAny<string>(), cancellationToken))
                .ReturnsAsync(subscription);

            _checkoutRepo
                .Setup(x => x.UpdateAsync(It.IsAny<SubscriptionCheckout>(), cancellationToken))
                .Returns(Task.CompletedTask);

            _context
                .Setup(x => x.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            _transaction.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_CheckoutNotFound_ShouldReturnTrue()
        {
            var orderCode = 999999999L;
            var amount = 99000L;
            var reference = "REF123456";
            var command = new ProcessPaymentWebhookCommand(orderCode, amount, reference);
            var cancellationToken = CancellationToken.None;

            _checkoutRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<SubscriptionCheckout>>(), cancellationToken))
                .ReturnsAsync((SubscriptionCheckout?)null);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            _subscriptionService.Verify(
                x => x.AssignPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PaymentMethod>(), It.IsAny<decimal?>(), It.IsAny<string>(), cancellationToken),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_CheckoutAlreadyProcessed_ShouldReturnTrue()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var orderCode = 202401011234567L;
            var amount = 99000L;
            var reference = "REF123456";
            var command = new ProcessPaymentWebhookCommand(orderCode, amount, reference);
            var cancellationToken = CancellationToken.None;

            var checkout = new SubscriptionCheckout
            {
                OrderCode = orderCode,
                MerchantId = merchantId,
                PlanId = planId,
                Status = PaymentStatus.Success
            };

            _checkoutRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<SubscriptionCheckout>>(), cancellationToken))
                .ReturnsAsync(checkout);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_InsufficientAmount_ShouldMarkFailed()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var orderCode = 202401011234567L;
            var requiredAmount = 99000m;
            var receivedAmount = 50000L;
            var reference = "REF123456";
            var command = new ProcessPaymentWebhookCommand(orderCode, receivedAmount, reference);
            var cancellationToken = CancellationToken.None;

            var checkout = new SubscriptionCheckout
            {
                OrderCode = orderCode,
                MerchantId = merchantId,
                PlanId = planId,
                Amount = requiredAmount,
                Status = PaymentStatus.Pending
            };

            _checkoutRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<SubscriptionCheckout>>(), cancellationToken))
                .ReturnsAsync(checkout);

            _checkoutRepo
                .Setup(x => x.UpdateAsync(It.IsAny<SubscriptionCheckout>(), cancellationToken))
                .Returns(Task.CompletedTask);

            _context
                .Setup(x => x.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            _checkoutRepo.Verify(
                x => x.UpdateAsync(It.Is<SubscriptionCheckout>(c => c.Status == PaymentStatus.Failed), cancellationToken),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ServiceThrows_ShouldRollbackTransaction()
        {
            var merchantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var orderCode = 202401011234567L;
            var amount = 99000L;
            var reference = "REF123456";
            var command = new ProcessPaymentWebhookCommand(orderCode, amount, reference);
            var cancellationToken = CancellationToken.None;

            var checkout = new SubscriptionCheckout
            {
                OrderCode = orderCode,
                MerchantId = merchantId,
                PlanId = planId,
                Amount = 99000m,
                Status = PaymentStatus.Pending
            };

            _checkoutRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<SubscriptionCheckout>>(), cancellationToken))
                .ReturnsAsync(checkout);

            _subscriptionService
                .Setup(x => x.AssignPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PaymentMethod>(), It.IsAny<decimal?>(), It.IsAny<string>(), cancellationToken))
                .ThrowsAsync(new NotFoundException("Merchant not found"));

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, cancellationToken));

            _transaction.Verify(x => x.RollbackAsync(cancellationToken), Times.Once);
        }
    }
}
