namespace QRDine.Application.Common.Constants
{
    public static class CacheDurations
    {
        public static readonly TimeSpan StorefrontMenu = TimeSpan.FromHours(12);
        public static readonly TimeSpan FeatureLimits = TimeSpan.FromHours(24);
        public static readonly TimeSpan MerchantActiveStatus = TimeSpan.FromHours(1);
        
        public static readonly TimeSpan Reports = TimeSpan.FromMinutes(30);
    }
}
