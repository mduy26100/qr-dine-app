namespace QRDine.Infrastructure.Identity.Constants
{
    public static class SystemRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Merchant = "Merchant";
        public const string Staff = "Staff";
        public const string Guest = "Guest";

        public static readonly IReadOnlyCollection<string> AllRoles = new[]
        {
            SuperAdmin, Merchant, Staff, Guest
        };
    }
}
