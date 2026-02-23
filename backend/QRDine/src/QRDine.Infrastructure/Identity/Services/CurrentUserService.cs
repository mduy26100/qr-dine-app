using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Infrastructure.Identity.Constants;

namespace QRDine.Infrastructure.Identity.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
            }
        }

        public IEnumerable<string> Roles
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?
                    .FindAll(ClaimTypes.Role)
                    .Select(r => r.Value)
                    ?? Enumerable.Empty<string>();
            }
        }

        public Guid? MerchantId
        {
            get
            {
                var merchantIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimTypes.MerchantId)?.Value;
                return Guid.TryParse(merchantIdStr, out var merchantId) ? merchantId : null;
            }
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
