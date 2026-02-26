namespace QRDine.Application.Features.Catalog.Tables.Services
{
    public interface ITableQrGeneratorService
    {
        Task<string> GenerateAndUploadQrAsync(Guid merchantId, string tableToken, string tableName, CancellationToken cancellationToken = default);
    }
}
