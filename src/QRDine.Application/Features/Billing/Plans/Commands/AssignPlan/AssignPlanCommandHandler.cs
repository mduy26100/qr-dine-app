using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Billing.Subscriptions.Services;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Plans.Commands.AssignPlan
{
    public class AssignPlanCommandHandler : IRequestHandler<AssignPlanCommand, AssignPlanResponseDto>
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IMapper _mapper;

        public AssignPlanCommandHandler(ISubscriptionService subscriptionService, IMapper mapper)
        {
            _subscriptionService = subscriptionService;
            _mapper = mapper;
        }

        public async Task<AssignPlanResponseDto> Handle(AssignPlanCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var subscription = await _subscriptionService.AssignPlanAsync(
                merchantId: dto.MerchantId,
                planId: dto.PlanId,
                paymentMethod: PaymentMethod.System_Grant,
                overrideAmount: 0,
                adminNote: dto.AdminNote ?? "Super Admin cấp gói thủ công",
                cancellationToken: cancellationToken
            );

            return _mapper.Map<AssignPlanResponseDto>(subscription);
        }
    }
}
