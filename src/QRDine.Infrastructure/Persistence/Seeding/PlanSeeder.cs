using QRDine.Domain.Billing;
using QRDine.Domain.Constants;

namespace QRDine.Infrastructure.Persistence.Seeding
{
    public static class PlanSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("PlanSeeder");

            var existingPlans = await context.Plans
                .ToDictionaryAsync(p => p.Code);

            var plansToAdd = new List<Plan>();

            var targetPlans = new List<Plan>
            {
                // =====================
                // TRIAL
                // =====================
                CreatePlan($"{PlanTiers.Trial}_14D", "Dùng Thử Miễn Phí", 0, 14, LimitedFeature(10, 30, 1, false)),

                // =====================
                // STANDARD (59k)
                // =====================
                CreatePlan($"{PlanTiers.Standard}_1M", "Tiêu Chuẩn (1 Tháng)", 59000, 30, LimitedFeature(30, 100, 3, false)),
                CreatePlan($"{PlanTiers.Standard}_3M", "Tiêu Chuẩn (3 Tháng)", 59000 * 3, 90, LimitedFeature(30, 100, 3, false)),
                CreatePlan($"{PlanTiers.Standard}_6M", "Tiêu Chuẩn (6 Tháng) - Tặng 1 tháng - Tiết kiệm 5%", (decimal)(59000 * 6 * 0.95), 210, LimitedFeature(30, 100, 3, false)),
                CreatePlan($"{PlanTiers.Standard}_12M", "Tiêu Chuẩn (1 Năm) - Tặng 2 Tháng - Tiết kiệm 10%", (decimal)(59000 * 12 * 0.9), 420, LimitedFeature(30, 100, 3, false)),

                // =====================
                // PREMIUM (99k)
                // =====================
                CreatePlan($"{PlanTiers.Premium}_1M", "Cao Cấp (1 Tháng)", 99000, 30, LimitedFeature(100, 500, 10, true)),
                CreatePlan($"{PlanTiers.Premium}_3M", "Cao Cấp (3 Tháng) - Tiết kiệm 5%", (decimal)(99000 * 3 * 0.95), 90, LimitedFeature(100, 500, 10, true)),
                CreatePlan($"{PlanTiers.Premium}_6M", "Cao Cấp (6 Tháng) - Tặng 2 tháng - Tiết kiệm 10%", (decimal)(99000 * 6 * 0.9), 240, LimitedFeature(100, 500, 10, true)),
                CreatePlan($"{PlanTiers.Premium}_12M", "Cao Cấp (1 Năm) - Tặng 4 Tháng - Tiết kiệm 15%", (decimal)(99000 * 12 * 0.85), 480, LimitedFeature(100, 500, 10, true)),

                // =====================
                // BUSINESS (139k)
                // =====================
                CreatePlan($"{PlanTiers.Business}_1M", "Doanh Nghiệp (1 Tháng)", 139000, 30, UnlimitedFeature()),
                CreatePlan($"{PlanTiers.Business}_3M", "Doanh Nghiệp (3 Tháng) - Tặng 1 tháng - Tiết kiệm 10%", (decimal)(139000 * 3 * 0.9), 120, UnlimitedFeature()),
                CreatePlan($"{PlanTiers.Business}_6M", "Doanh Nghiệp (6 Tháng) - Tặng 2 tháng - Tiết kiệm 15%", (decimal)(139000 * 6 * 0.85), 240, UnlimitedFeature()),
                CreatePlan($"{PlanTiers.Business}_12M", "Doanh Nghiệp (1 Năm) - Tặng 4 tháng - Tiết kiệm 20%", (decimal)(139000 * 12 * 0.8), 480, UnlimitedFeature())
            };

            foreach (var target in targetPlans)
            {
                if (existingPlans.TryGetValue(target.Code, out var existingPlan))
                {
                    existingPlan.Name = target.Name;
                    existingPlan.Price = target.Price;
                    existingPlan.DurationDays = target.DurationDays;

                    if (existingPlan.FeatureLimit != null && target.FeatureLimit != null)
                    {
                        existingPlan.FeatureLimit.MaxTables = target.FeatureLimit.MaxTables;
                        existingPlan.FeatureLimit.MaxProducts = target.FeatureLimit.MaxProducts;
                        existingPlan.FeatureLimit.MaxStaffMembers = target.FeatureLimit.MaxStaffMembers;
                        existingPlan.FeatureLimit.AllowAdvancedReports = target.FeatureLimit.AllowAdvancedReports;
                    }
                }
                else
                {
                    plansToAdd.Add(target);
                }
            }

            if (plansToAdd.Any())
            {
                await context.Plans.AddRangeAsync(plansToAdd);
                logger.LogInformation($"[PlanSeeder] Added {plansToAdd.Count} new plans.");
            }

            var changes = await context.SaveChangesAsync();

            if (changes > 0)
            {
                logger.LogInformation($"[PlanSeeder] Successfully synced {changes} records to Database.");
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
