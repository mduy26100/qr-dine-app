using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Application.Common.Constants;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Billing.Plans.Specifications;
using QRDine.Application.Features.Billing.Repositories;
using QRDine.Application.Features.Billing.Subscriptions.DTOs;
using QRDine.Application.Features.Billing.Subscriptions.Specifications;
using QRDine.Application.Features.Tenant.Repositories;
using QRDine.Domain.Billing;
using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Billing.Subscriptions.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IPlanRepository _planRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IApplicationDbContext _context;
        private readonly ICacheService _cacheService;

        public SubscriptionService(
            IPlanRepository planRepository,
            ISubscriptionRepository subscriptionRepository,
            IMerchantRepository merchantRepository,
            ITransactionRepository transactionRepository,
            IApplicationDbContext context,
            ICacheService cacheService)
        {
            _planRepository = planRepository;
            _subscriptionRepository = subscriptionRepository;
            _merchantRepository = merchantRepository;
            _transactionRepository = transactionRepository;
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<Subscription> AssignPlanAsync(
            Guid merchantId,
            Guid planId,
            PaymentMethod paymentMethod,
            decimal? overrideAmount = null,
            string? adminNote = null,
            CancellationToken cancellationToken = default)
        {
            var merchant = await _merchantRepository.GetByIdAsync(merchantId, cancellationToken);
            if (merchant == null)
            {
                throw new NotFoundException("Cửa hàng (Merchant) không tồn tại trong hệ thống.");
            }

            var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
            if (plan == null || !plan.IsActive)
                throw new NotFoundException("Gói cước không tồn tại hoặc đã ngừng bán.");

            await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                var subSpec = new GetSubscriptionByMerchantIdSpec(merchantId);
                var subscription = await _subscriptionRepository.SingleOrDefaultAsync(subSpec, cancellationToken);

                var now = DateTime.UtcNow;

                if (subscription == null)
                {
                    subscription = new Subscription
                    {
                        MerchantId = merchantId,
                        PlanId = plan.Id,
                        Status = SubscriptionStatus.Active,
                        StartDate = now,
                        EndDate = now.AddDays(plan.DurationDays),
                        AdminNote = adminNote
                    };
                    await _subscriptionRepository.AddAsync(subscription, cancellationToken);
                }
                else
                {
                    if (subscription.PlanId == plan.Id)
                    {
                        var baseDate = subscription.EndDate > now ? subscription.EndDate : now;
                        subscription.EndDate = baseDate.AddDays(plan.DurationDays);
                    }
                    else
                    {
                        subscription.PlanId = plan.Id;
                        subscription.StartDate = now;
                        subscription.EndDate = now.AddDays(plan.DurationDays);
                    }

                    subscription.Status = SubscriptionStatus.Active;
                    subscription.AdminNote = adminNote;

                    await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
                }

                var billingTransaction = new Transaction
                {
                    MerchantId = merchantId,
                    PlanId = plan.Id,
                    Subscription = subscription,
                    Amount = overrideAmount ?? plan.Price,
                    Status = PaymentStatus.Success,
                    Method = paymentMethod,
                    PaidAt = now,
                    AdminNote = adminNote
                };

                await _transactionRepository.AddAsync(billingTransaction, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var cacheKey = CacheKeys.MerchantActiveStatus(merchantId);
                await _cacheService.RemoveAsync(cacheKey, cancellationToken);

                return subscription;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<MerchantSubscriptionInfoDto?> GetLatestSubscriptionInfoAsync(Guid merchantId, CancellationToken cancellationToken = default)
        {
            var spec = new GetLatestSubscriptionInfoSpec(merchantId);
            return await _subscriptionRepository.SingleOrDefaultAsync(spec, cancellationToken);
        }

        public async Task<bool> IsSubscriptionActiveAsync(Guid merchantId, CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.MerchantActiveStatus(merchantId);

            var isActive = await _cacheService.GetAsync<bool?>(cacheKey, cancellationToken);

            if (isActive == null)
            {
                var spec = new CheckActiveSubscriptionSpec(merchantId);
                var result = await _subscriptionRepository.AnyAsync(spec, cancellationToken);

                await _cacheService.SetAsync(cacheKey, result, CacheDurations.MerchantActiveStatus, cancellationToken);
                return result;
            }

            return isActive.Value;
        }
    }
}
