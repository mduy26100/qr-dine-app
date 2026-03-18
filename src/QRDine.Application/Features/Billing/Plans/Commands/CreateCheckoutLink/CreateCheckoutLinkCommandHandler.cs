using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Abstractions.PayOS;
using QRDine.Application.Common.Abstractions.PayOS.Models;
using QRDine.Application.Features.Billing.Plans.Specifications;
using QRDine.Application.Features.Billing.Repositories;
using QRDine.Domain.Billing;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Plans.Commands.CreateCheckoutLink
{
    public class CreateCheckoutLinkCommandHandler : IRequestHandler<CreateCheckoutLinkCommand, string>
    {
        private readonly IPlanRepository _planRepository;
        private readonly ISubscriptionCheckoutRepository _checkoutRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPayOSService _payOSService;

        public CreateCheckoutLinkCommandHandler(
            IPlanRepository planRepository,
            ISubscriptionCheckoutRepository checkoutRepo,
            ISubscriptionRepository subscriptionRepo,
            ICurrentUserService currentUserService,
            IPayOSService payOSService)
        {
            _planRepository = planRepository;
            _checkoutRepo = checkoutRepo;
            _subscriptionRepo = subscriptionRepo;
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

            var subSpec = new GetSubscriptionByMerchantIdSpec(merchantId);
            var currentSubscription = await _subscriptionRepo.SingleOrDefaultAsync(subSpec, cancellationToken);

            string prefix = "Mua";
            if (currentSubscription != null && currentSubscription.Status == SubscriptionStatus.Active && currentSubscription.EndDate > DateTime.UtcNow)
            {
                if (currentSubscription.PlanId == plan.Id)
                {
                    prefix = "Gia han";
                }
                else
                {
                    prefix = "Nang cap";
                }
            }

            var checkoutRecord = new SubscriptionCheckout
            {
                OrderCode = orderCode,
                MerchantId = merchantId,
                PlanId = plan.Id,
                Amount = plan.Price,
                Status = PaymentStatus.Pending
            };

            await _checkoutRepo.AddAsync(checkoutRecord, cancellationToken);

            var shortCode = merchantId.ToString().Substring(0, 6).ToUpper();
            var cleanPlanName = RemoveVietnameseAccents(plan.Name);

            int reservedLength = prefix.Length + shortCode.Length + 2;
            int maxPlanNameLength = 25 - reservedLength;

            if (cleanPlanName.Length > maxPlanNameLength)
            {
                cleanPlanName = cleanPlanName.Substring(0, maxPlanNameLength).Trim();
            }

            var description = $"{prefix} {cleanPlanName} {shortCode}";

            var paymentData = new PaymentLinkRequestDto
            {
                OrderCode = checkoutRecord.OrderCode,
                Amount = (int)plan.Price,
                Description = description,
                CancelUrl = $"{request.Dto.ReturnDomain}/management/billing/cancel",
                ReturnUrl = $"{request.Dto.ReturnDomain}/management/billing/success"
            };

            var checkoutUrl = await _payOSService.CreatePaymentLinkAsync(paymentData);

            return checkoutUrl;
        }

        private static string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }
            return text;
        }
    }
}
