using QRDine.Application.Common.Abstractions.ExternalServices.QrCode;

namespace QRDine.Infrastructure.ExternalServices.QrCode
{
    public class QrCodeService : IQrCodeService
    {
        public Task<byte[]> GenerateQrCodeAsync(string payload, CancellationToken cancellationToken = default)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

            int pixelsPerModule = 20;
            var size = qrCodeData.ModuleMatrix.Count * pixelsPerModule;

            using var surface = SKSurface.Create(new SKImageInfo(size, size));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = false };

            for (int x = 0; x < qrCodeData.ModuleMatrix.Count; x++)
            {
                for (int y = 0; y < qrCodeData.ModuleMatrix.Count; y++)
                {
                    if (qrCodeData.ModuleMatrix[y][x])
                    {
                        var rect = SKRect.Create(x * pixelsPerModule, y * pixelsPerModule, pixelsPerModule, pixelsPerModule);
                        canvas.DrawRect(rect, paint);
                    }
                }
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            return Task.FromResult(data.ToArray());
        }
    }
}
