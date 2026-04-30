namespace SharedKernel.Application.Interfaces.ExternalServices.QrCode
{
    public interface IQrCodeService
    {
        Task<byte[]> GenerateQrCodeAsync(string payload, string? logoPath = null, CancellationToken cancellationToken = default);
    }
}
