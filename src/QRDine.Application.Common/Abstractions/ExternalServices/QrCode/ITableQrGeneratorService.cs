namespace QRDine.Application.Common.Abstractions.ExternalServices.QrCode
{
    public interface ITableQrGeneratorService
    {
        Task<string> GenerateAndUploadQrAsync(Guid merchantId, string tableToken, string tableName, CancellationToken cancellationToken = default);
    }
}
