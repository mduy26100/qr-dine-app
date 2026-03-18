using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Billing.Subscriptions.Services;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Plans.Commands.AssignPlan
{
    public class AssignPlanCommandHandler : IRequestHandler<AssignPlanCommand, AssignPlanResponseDto>
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;

        public AssignPlanCommandHandler(
            ISubscriptionService subscriptionService,
            IMapper mapper,
            IApplicationDbContext context)
        {
            _subscriptionService = subscriptionService;
            _mapper = mapper;
            _context = context;
        }

        public async Task<AssignPlanResponseDto> Handle(AssignPlanCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
            try
            {
                var subscription = await _subscriptionService.AssignPlanAsync(
                    merchantId: request.MerchantId,
                    planId: dto.PlanId,
                    paymentMethod: PaymentMethod.System_Grant,
                    overrideAmount: 0,
                    adminNote: dto.AdminNote ?? "Super Admin cấp gói thủ công",
                    cancellationToken: cancellationToken
                );

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return _mapper.Map<AssignPlanResponseDto>(subscription);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
