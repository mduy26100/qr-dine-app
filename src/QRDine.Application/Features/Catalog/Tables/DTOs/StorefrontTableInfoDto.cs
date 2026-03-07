namespace QRDine.Application.Features.Catalog.Tables.DTOs
{
    public class StorefrontTableInfoDto
    {
        public Guid TableId { get; set; }
        public Guid MerchantId { get; set; }
        public string TableName { get; set; } = default!;
        public bool IsOccupied { get; set; }
        public Guid? SessionId { get; set; }

        public string MerchantName { get; set; } = default!;
        public string? MerchantAddress { get; set; }
        public string? MerchantLogoUrl { get; set; }
    }
}
