using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Plans.Extensions
{
    public static class PlanExtensions
    {
        public static Expression<Func<Plan, PlanDto>> ToAdminPlanDto =>
            p => new PlanDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Price = p.Price,
                DurationDays = p.DurationDays
            };
    }
}
