using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Billing.Subscriptions.DTOs;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;
using QRDine.Application.Features.Tenant.Merchants.DTOs;
using QRDine.Application.Features.Tenant.Merchants.Specifications;
using QRDine.Application.Features.Tenant.Repositories;
using QRDine.Infrastructure.Identity.Constants;
using QRDine.Infrastructure.Identity.Models;

namespace QRDine.Infrastructure.Identity.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMerchantRepository _merchantRepository;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            IMerchantRepository merchantRepository)
        {
            _userManager = userManager;
            _merchantRepository = merchantRepository;
        }

        public async Task<UserProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new NotFoundException("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            var profile = new UserProfileDto
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email!,
                PersonalPhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                Roles = roles
            };

            if (roles.Contains(SystemRoles.SuperAdmin))
            {
                return profile;
            }

            if (!user.MerchantId.HasValue)
            {
                throw new BadRequestException("User does not belong to any merchant.");
            }

            var spec = new MerchantProfileSpec(user.MerchantId.Value);
            var merchantData = await _merchantRepository.FirstOrDefaultAsync(spec, cancellationToken);

            if (merchantData == null) throw new NotFoundException("Store information not found or inactive.");

            profile.StoreInfo = new StoreInfoDto
            {
                MerchantId = merchantData.Id,
                Name = merchantData.Name,
                Slug = merchantData.Slug,
                Address = merchantData.Address,
                PhoneNumber = merchantData.PhoneNumber,
                LogoUrl = merchantData.LogoUrl
            };

            if (roles.Contains(SystemRoles.Merchant))
            {
                int daysRemaining = 0;
                if (merchantData.PlanEndDate.HasValue)
                {
                    var timeSpan = merchantData.PlanEndDate.Value - DateTime.UtcNow;
                    daysRemaining = timeSpan.Days > 0 ? timeSpan.Days : 0;
                }

                profile.SubscriptionInfo = new SubscriptionInfoDto
                {
                    PlanName = merchantData.PlanName ?? "Free/Trial",
                    Status = merchantData.SubscriptionStatus ?? "None",
                    EndDate = merchantData.PlanEndDate ?? DateTime.MinValue,
                    DaysRemaining = daysRemaining
                };
            }

            return profile;
        }
    }
}
