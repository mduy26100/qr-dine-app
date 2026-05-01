namespace SharedKernel.QrCode.Services
{
    public class QrCodeService : IQrCodeService
    {
        public Task<byte[]> GenerateQrCodeAsync(string payload, string? logoPath = null, CancellationToken cancellationToken = default)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);

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

            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
            {
                using var logoData = SKData.Create(logoPath);
                using var logoBitmap = SKBitmap.Decode(logoData);

                var logoSize = size / 5;
                var xLogo = (size - logoSize) / 2;
                var yLogo = (size - logoSize) / 2;

                using var bgPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
                var padding = pixelsPerModule;
                var bgRect = new SKRect(xLogo - padding, yLogo - padding, xLogo + logoSize + padding, yLogo + logoSize + padding);

                canvas.DrawRoundRect(bgRect, 15, 15, bgPaint);

                var logoRect = new SKRect(xLogo, yLogo, xLogo + logoSize, yLogo + logoSize);
                canvas.DrawBitmap(logoBitmap, logoRect);
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            return Task.FromResult(data.ToArray());
        }
    }
}
