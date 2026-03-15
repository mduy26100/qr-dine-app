namespace QRDine.Application.Common.Constants
{
    public class RegistrationCacheKeys
    {
        public const string RegisterMerchantPrefix = "RegToken_";

        public static string GetMerchantRegistrationKey(string token)
        {
            return $"{RegisterMerchantPrefix}{token}";
        }
    }
}
