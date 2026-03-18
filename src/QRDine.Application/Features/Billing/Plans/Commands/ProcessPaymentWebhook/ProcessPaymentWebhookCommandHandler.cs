using QRDine.Application.Features.Billing.Repositories;
using QRDine.Application.Features.Billing.Subscriptions.Services;
using QRDine.Application.Features.Billing.Subscriptions.Specifications;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Plans.Commands.ProcessPaymentWebhook
{
    public class ProcessPaymentWebhookCommandHandler : IRequestHandler<ProcessPaymentWebhookCommand, bool>
    {
        private readonly ISubscriptionCheckoutRepository _checkoutRepo;
        private readonly ISubscriptionService _subscriptionService;

        public ProcessPaymentWebhookCommandHandler(
            ISubscriptionCheckoutRepository checkoutRepo,
            ISubscriptionService subscriptionService)
        {
            _checkoutRepo = checkoutRepo;
            _subscriptionService = subscriptionService;
        }

        public async Task<bool> Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
        {
            var spec = new SubscriptionCheckoutByOrderCodeSpec(request.OrderCode);
            var checkoutRecord = await _checkoutRepo.SingleOrDefaultAsync(spec, cancellationToken);

            if (checkoutRecord == null || checkoutRecord.Status != PaymentStatus.Pending)
            {
                return true;
            }

            await _subscriptionService.AssignPlanAsync(
                merchantId: checkoutRecord.MerchantId,
                planId: checkoutRecord.PlanId,
                paymentMethod: PaymentMethod.BankTransfer,
                overrideAmount: checkoutRecord.Amount,
                adminNote: $"Thanh toán PayOS. Reference: {request.Reference}",
                cancellationToken: cancellationToken
            );

            checkoutRecord.Status = PaymentStatus.Success;
            await _checkoutRepo.UpdateAsync(checkoutRecord, cancellationToken);

            return true;
        }
    }
}
