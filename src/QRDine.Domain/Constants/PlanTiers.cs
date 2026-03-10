namespace QRDine.Domain.Constants
{
    public static class PlanTiers
    {
        public const string Trial = "TRIAL";
        public const string Standard = "STANDARD";
        public const string Premium = "PREMIUM";
        public const string Business = "BUSINESS";

        public static readonly string[] All = { Trial, Standard, Premium, Business };
    }
}
