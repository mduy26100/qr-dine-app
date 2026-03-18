# External Services

Third-party service integrations and configurations.

## Contents

- **[Cloudinary Integration](cloudinary.md)** — Image upload service documentation including:
  - Cloudinary account setup
  - Configuration in appsettings.json
  - Image upload implementation
  - Usage examples in controllers

## Available Integrations

| Service        | Purpose            | Configuration                | Status         |
| -------------- | ------------------ | ---------------------------- | -------------- |
| **Cloudinary** | Image/media upload | CloudName, ApiKey, ApiSecret | ✅ Implemented |
| **MailKit**    | Email sending      | SMTP, credentials            | ✅ Implemented |
| **SignalR**    | Real-time updates  | WebSocket configuration      | ✅ Implemented |
| **Redis**      | Cache backend      | Connection string            | ✅ Implemented |

## Cloudinary Setup

Cloudinary handles all image uploads for:

- Product images
- Merchant logos
- Marketing materials

### Quick Setup

1. **Create Cloudinary account** at [cloudinary.com](https://cloudinary.com/)
2. **Get credentials** (CloudName, ApiKey, ApiSecret)
3. **Configure in appsettings.json:**
   ```json
   "Cloudinary": {
     "CloudName": "your-cloud-name",
     "ApiKey": "your-api-key",
     "ApiSecret": "your-api-secret"
   }
   ```
4. **Use in controllers** - `IFileUploadService` handles upload logic

See [Cloudinary Integration](cloudinary.md) for complete documentation and usage examples.

## Service Dependencies Graph

```
QRDine.API
├── Cloudinary (image uploads)
├── MailKit (email)
├── SignalR (real-time)
├── Redis (caching)
└── SQL Server (data)
```

---

**Reference:** See also [Configuration](../configuration/) for service credentials setup and [Deployment](../deployment/) for service infrastructure.
