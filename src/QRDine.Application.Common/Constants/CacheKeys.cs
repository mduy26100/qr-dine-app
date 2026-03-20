namespace QRDine.Application.Common.Constants
{
    public static class CacheKeys
    {
        public static string StorefrontMenu(Guid merchantId) => $"StorefrontMenu_{merchantId}";
        public static string FeatureLimit(string planCode) => $"FeatureLimit_{planCode}";
        public static string MerchantActiveStatus(Guid merchantId) => $"MerchantActiveStatus_{merchantId}";

        public static string ProductPerformance(DateTime startDate, DateTime endDate, int sortBy, int top) 
            => $"ProductPerformance_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{sortBy}_{top}";

        public static string RevenueChart(DateTime startDate, DateTime endDate, int grouping) 
            => $"RevenueChart_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{grouping}";

        public static string RevenueSummary(DateTime startDate, DateTime endDate) 
            => $"RevenueSummary_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}";

        public static string ToppingAnalytics(DateTime startDate, DateTime endDate) 
            => $"ToppingAnalytics_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}";

        public static string TrafficHeatmap(DateTime startDate, DateTime endDate) 
            => $"TrafficHeatmap_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}";
    }
}
