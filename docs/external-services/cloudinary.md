# Cloudinary Integration

QRDine uses **Cloudinary** as the external cloud service for image hosting. This integration is used for product image uploads.

---

## Architecture

The file upload functionality follows the Clean Architecture pattern:

```
IFileUploadService (Application.Common)
    ↓ implemented by
CloudinaryFileUploadService (Infrastructure)
    ↓ configured in
ExternalServicesRegistration (API/DI)
```

### Abstraction

**File:** `src/QRDine.Application.Common/Abstractions/ExternalServices/FileUpload/IFileUploadService.cs`

```csharp
public interface IFileUploadService
{
    Task<string> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
}
```

**File:** `src/QRDine.Application.Common/Abstractions/ExternalServices/FileUpload/FileUploadRequest.cs`

| Property | Type | Description |
|----------|------|-------------|
| `Content` | `Stream` | File content stream |
| `FileName` | `string` | Original file name |
| `ContentType` | `string` | MIME type (e.g., `image/jpeg`) |

### Implementation

**File:** `src/QRDine.Infrastructure/ExternalServices/Cloudinary/CloudinaryFileUploadService.cs`

```csharp
public class CloudinaryFileUploadService : IFileUploadService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryFileUploadService(IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    public async Task<string> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(request.FileName, request.Content),
            PublicId = Path.GetFileNameWithoutExtension(request.FileName),
            Folder = "QRDine",
            Overwrite = true,
            UseFilename = true,
            UniqueFilename = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception($"Cloudinary upload failed: {result.Error?.Message}");

        return result.SecureUrl.ToString();
    }
}
```

**Upload behavior:**
- All images are uploaded to the **`QRDine`** folder in Cloudinary.
- `PublicId` is derived from the file name (without extension).
- `Overwrite = true` — Re-uploading with the same name replaces the existing image.
- `UniqueFilename = false` — No random suffix is appended to the file name.
- Returns the **secure HTTPS URL** of the uploaded image.

---

## Configuration

### Settings

**File:** `src/QRDine.Infrastructure/ExternalServices/Cloudinary/CloudinarySettings.cs`

```csharp
public class CloudinarySettings
{
    public string? CloudName { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
}
```

### appsettings.json

Configured under the `Cloudinary` section:

```json
{
  "Cloudinary": {
    "CloudName": "<your-cloud-name>",
    "ApiKey": "<your-api-key>",
    "ApiSecret": "<your-api-secret>"
  }
}
```

### DI Registration

**File:** `src/QRDine.API/DependencyInjection/Infrastructure/ExternalServicesRegistration.cs`

```csharp
services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
services.AddScoped<IFileUploadService, CloudinaryFileUploadService>();
```

---

## Usage

Currently, `IFileUploadService` is consumed by `CreateProductCommandHandler` (`src/QRDine.Application/Features/Catalog/Products/Commands/CreateProduct/CreateProductCommandHandler.cs`):

```csharp
if (request.Dto.ImgContent != null && !string.IsNullOrWhiteSpace(request.Dto.ImgFileName))
{
    var uploadRequest = new FileUploadRequest
    {
        Content = request.Dto.ImgContent,
        FileName = request.Dto.ImgFileName,
        ContentType = request.Dto.ImgContentType ?? "image/jpeg"
    };
    imgUrl = await _fileUploadService.UploadAsync(uploadRequest, cancellationToken);
}
```

The product image is optional. If no `ImageFile` is provided in the form data, `ImageUrl` is set to `null`.

---

## NuGet Package

- **CloudinaryDotNet** — Official Cloudinary .NET SDK, referenced in `src/QRDine.Infrastructure/QRDine.Infrastructure.csproj`.
