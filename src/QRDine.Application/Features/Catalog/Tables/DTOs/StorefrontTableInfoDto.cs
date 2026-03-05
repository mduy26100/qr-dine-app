namespace QRDine.Application.Features.Catalog.Tables.DTOs
{
    public class StorefrontTableInfoDto
    {
        public Guid TableId { get; set; }
        public Guid MerchantId { get; set; }
        public string TableName { get; set; } = default!;
        public bool IsOccupied { get; set; }
        public Guid SessionId { get; set; }
    }
}
