namespace QRDine.Application.Common.Constants
{
    public static class CacheKeys
    {
        public static string StorefrontMenu(Guid merchantId) => $"StorefrontMenu_{merchantId}";
        public static string FeatureLimit(string planCode) => $"FeatureLimit_{planCode}";
    }
}
