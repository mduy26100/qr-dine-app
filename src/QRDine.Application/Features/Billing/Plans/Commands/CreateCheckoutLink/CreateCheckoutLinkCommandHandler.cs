using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Abstractions.PayOS;
using QRDine.Application.Common.Abstractions.PayOS.Models;
using QRDine.Application.Features.Billing.Repositories;
using QRDine.Domain.Billing;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Plans.Commands.CreateCheckoutLink
{
    public class CreateCheckoutLinkCommandHandler : IRequestHandler<CreateCheckoutLinkCommand, string>
    {
        private readonly IPlanRepository _planRepository;
        private readonly ISubscriptionCheckoutRepository _checkoutRepo;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPayOSService _payOSService;

        public CreateCheckoutLinkCommandHandler(
            IPlanRepository planRepository,
            ISubscriptionCheckoutRepository checkoutRepo,
            ICurrentUserService currentUserService,
            IPayOSService payOSService)
        {
            _planRepository = planRepository;
            _checkoutRepo = checkoutRepo;
            _currentUserService = currentUserService;
            _payOSService = payOSService;
        }

        public async Task<string> Handle(CreateCheckoutLinkCommand request, CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin Merchant.");

            var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken);
            if (plan == null || !plan.IsActive)
            {
                throw new Exception("Gói cước không tồn tại hoặc đã ngừng bán.");
            }

            var orderCode = long.Parse(DateTimeOffset.UtcNow.ToString("yyMMddHHmmssfff"));

            var checkoutRecord = new SubscriptionCheckout
            {
                OrderCode = orderCode,
                MerchantId = merchantId,
                PlanId = plan.Id,
                Amount = plan.Price,
                Status = PaymentStatus.Pending
            };

            await _checkoutRepo.AddAsync(checkoutRecord, cancellationToken);

            var description = $"Mua {plan.Name}";
            if (description.Length > 25)
            {
                description = description.Substring(0, 25);
            }

            var paymentData = new PaymentLinkRequestDto
            {
                OrderCode = checkoutRecord.OrderCode,
                Amount = (int)plan.Price,
                Description = description,
                CancelUrl = $"{request.Dto.ReturnDomain}/billing/cancel",
                ReturnUrl = $"{request.Dto.ReturnDomain}/billing/success"
            };

            var checkoutUrl = await _payOSService.CreatePaymentLinkAsync(paymentData);

            return checkoutUrl;
        }
    }
}
