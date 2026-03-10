using QRDine.Application.Features.Billing.FeatureLimits.DTOs;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.FeatureLimits.Extensions
{
    public class FeatureLimitExtensions
    {
        public static Expression<Func<FeatureLimit, FeatureLimitCheckDto>> ToFeatureLimitCheckDto =>
            fl => new FeatureLimitCheckDto
            {
                MaxTables = fl.MaxTables,
                MaxProducts = fl.MaxProducts,
                MaxStaffMembers = fl.MaxStaffMembers,
                AllowAdvancedReports = fl.AllowAdvancedReports
            };
    }
}
