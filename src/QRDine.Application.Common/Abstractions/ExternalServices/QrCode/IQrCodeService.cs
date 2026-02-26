namespace QRDine.Application.Common.Abstractions.ExternalServices.QrCode
{
    public interface IQrCodeService
    {
        Task<byte[]> GenerateQrCodeAsync(string payload, CancellationToken cancellationToken = default);
    }
}
