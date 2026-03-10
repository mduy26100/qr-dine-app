using QRDine.Domain.Billing;
using QRDine.Domain.Constants;

namespace QRDine.Infrastructure.Persistence.Seeding
{
    public static class PlanSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            var existingCodes = await context.Plans
                .Select(p => p.Code)
                .ToListAsync();

            var plansToAdd = new List<Plan>();

            var targetPlans = new List<Plan>
            {
                // TRIAL
                CreatePlan($"{PlanTiers.Trial}_14D", "Dùng Thử Miễn Phí", 0, 14, LimitedFeature(10, 30, 1, false)),

                // STANDARD
                CreatePlan($"{PlanTiers.Standard}_1M", "Tiêu Chuẩn (1 Tháng)", 79000, 30, LimitedFeature(30,100,3, false)),
                CreatePlan($"{PlanTiers.Standard}_3M", "Tiêu Chuẩn (3 Tháng)", 239000, 90, LimitedFeature(30,100,3, false)),
                CreatePlan($"{PlanTiers.Standard}_6M", "Tiêu Chuẩn (6 Tháng) - Tặng 1 tháng - Tiết kiệm 5%", 449000, 210, LimitedFeature(30,100,3, false)),
                CreatePlan($"{PlanTiers.Standard}_12M", "Tiêu Chuẩn (1 Năm) - Tặng 2 Tháng - Tiết kiệm 10%", 849000, 425, LimitedFeature(30,100,3, false)),

                // PREMIUM
                CreatePlan($"{PlanTiers.Premium}_1M", "Cao Cấp (1 Tháng)", 119000, 30, LimitedFeature(100,500,10, true)),
                CreatePlan($"{PlanTiers.Premium}_6M", "Cao Cấp (6 Tháng) - Tặng 2 Tháng - Tiết kiệm 10%", 649000, 240, LimitedFeature(100,500,10, true)),
                CreatePlan($"{PlanTiers.Premium}_12M", "Cao Cấp (1 Năm) - Tặng 4 Tháng - Tiết kiệm 20%", 1149000, 485, LimitedFeature(100,500,10, true)),

                // BUSINESS
                CreatePlan($"{PlanTiers.Business}_1M", "Doanh Nghiệp (1 Tháng)", 179000, 30, UnlimitedFeature()),
                CreatePlan($"{PlanTiers.Business}_6M", "Doanh Nghiệp (6 Tháng) - Tặng 2 Tháng - Tiết kiệm 10%", 969000, 240, UnlimitedFeature()),
                CreatePlan($"{PlanTiers.Business}_12M", "Doanh Nghiệp (1 Năm) - Tặng 4 Tháng - Tiết kiệm 20%", 1719000, 485, UnlimitedFeature())
            };

            foreach (var plan in targetPlans)
            {
                if (!existingCodes.Contains(plan.Code))
                {
                    plansToAdd.Add(plan);
                }
            }

            if (plansToAdd.Any())
            {
                await context.Plans.AddRangeAsync(plansToAdd);
                await context.SaveChangesAsync();
            }
        }

        private static Plan CreatePlan(string code, string name, decimal price, int durationDays, FeatureLimit feature)
        {
            return new Plan
            {
                Code = code,
                Name = name,
                Price = price,
                DurationDays = durationDays,
                IsActive = true,
                FeatureLimit = feature
            };
        }

        private static FeatureLimit LimitedFeature(int tables, int products, int staff, bool allowReports = false)
        {
            return new FeatureLimit
            {
                MaxTables = tables,
                MaxProducts = products,
                MaxStaffMembers = staff,
                AllowAdvancedReports = allowReports
            };
        }

        private static FeatureLimit UnlimitedFeature()
        {
            return new FeatureLimit
            {
                MaxTables = null,
                MaxProducts = null,
                MaxStaffMembers = null,
                AllowAdvancedReports = true
            };
        }
    }
}
