namespace QRDine.Application.Features.Catalog.Tables.DTOs
{
    public class TableResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public bool IsOccupied { get; set; }
        public string QrCodeToken { get; set; } = default!;
        public string QrCodeImageUrl { get; set; } = default!;
    }
}
