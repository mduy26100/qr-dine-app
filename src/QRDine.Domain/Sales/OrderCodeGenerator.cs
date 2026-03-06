namespace QRDine.Domain.Sales
{
    public static class OrderCodeGenerator
    {
        public static string Generate()
        {
            var datePart = DateTime.UtcNow.ToString("yyMMdd");
            var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper();

            return $"ORD-{datePart}-{randomPart}";
        }
    }
}
