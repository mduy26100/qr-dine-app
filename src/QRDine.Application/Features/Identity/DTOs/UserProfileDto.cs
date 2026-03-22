using QRDine.Application.Features.Billing.Subscriptions.DTOs;
using QRDine.Application.Features.Tenant.Merchants.DTOs;

namespace QRDine.Application.Features.Identity.DTOs
{
    public class UserProfileDto
    {
        public Guid UserId { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PersonalPhoneNumber { get; set; }
        public string AvatarUrl { get; set; } = default!;
        public IList<string> Roles { get; set; } = new List<string>();

        public StoreInfoDto? StoreInfo { get; set; }

        public SubscriptionInfoDto? SubscriptionInfo { get; set; }
    }
}
