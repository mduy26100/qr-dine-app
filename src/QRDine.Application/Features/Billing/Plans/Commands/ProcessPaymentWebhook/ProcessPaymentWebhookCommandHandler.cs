using QRDine.Application.Common.Abstractions.Persistence;
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
        private readonly IApplicationDbContext _context;

        public ProcessPaymentWebhookCommandHandler(
            ISubscriptionCheckoutRepository checkoutRepo,
            ISubscriptionService subscriptionService,
            IApplicationDbContext context)
        {
            _checkoutRepo = checkoutRepo;
            _subscriptionService = subscriptionService;
            _context = context;
        }

        public async Task<bool> Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
        {
            var spec = new SubscriptionCheckoutByOrderCodeSpec(request.OrderCode);
            var checkoutRecord = await _checkoutRepo.SingleOrDefaultAsync(spec, cancellationToken);

            if (checkoutRecord == null || checkoutRecord.Status != PaymentStatus.Pending)
            {
                return true;
            }

            if (request.Amount < checkoutRecord.Amount)
            {
                checkoutRecord.Status = PaymentStatus.Failed;
                checkoutRecord.FailureReason = $"Khách hàng chuyển thiếu tiền. Yêu cầu: {checkoutRecord.Amount}, Thực nhận: {request.Amount}";
                await _checkoutRepo.UpdateAsync(checkoutRecord, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
            try
            {
                await _subscriptionService.AssignPlanAsync(
                    merchantId: checkoutRecord.MerchantId,
                    planId: checkoutRecord.PlanId,
                    paymentMethod: PaymentMethod.BankTransfer,
                    overrideAmount: request.Amount,
                    adminNote: $"Thanh toán PayOS. Reference: {request.Reference}",
                    cancellationToken: cancellationToken
                );

                checkoutRecord.Status = PaymentStatus.Success;
                await _checkoutRepo.UpdateAsync(checkoutRecord, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
