# External Services

Third-party service integrations and configurations.

## Contents

- **[PayOS Payment Integration](payos-integration.md)** — Vietnamese payment gateway for subscriptions
  - Complete service architecture
  - Configuration setup
  - Payment flow documentation
  - Webhook handling & verification
  - Error handling strategies
  - Testing guide & FAQ

- **[PayOS Architecture Diagrams](payos-architecture-diagrams.md)** — Visual reference
  - Application architecture
  - Payment processing flow
  - Data model relationships
  - Webhook signature verification
  - State machines & error flows

- **[PayOS API Reference](payos-api-reference.md)** — Developer guide
  - REST API endpoints
  - Request/response examples (cURL, JavaScript, Python)
  - Frontend integration patterns (React, Vue)
  - Integration testing
  - Monitoring & logging

- **[PayOS Complete Reference](payos-complete-reference.md)** — Quick start guide
  - Executive summary
  - Quick start checklist
  - File locations & structure
  - Common Q&A

- **[Cloudinary Integration](cloudinary.md)** — Image upload service documentation
  - Cloudinary account setup
  - Configuration in appsettings.json
  - Image upload implementation
  - Usage examples in controllers

- **[Email Services](email-services.md)** — Transactional email implementation
  - Brevo API implementation (primary)
  - MailKit SMTP implementation (alternative)
  - Email configuration setup
  - Email templates and workflows
  - Error handling & resilience
  - Configuration by provider (Brevo, Gmail, Office 365)

## Available Integrations

| Service        | Purpose               | Configuration                 | Status                           | Docs                                                  |
| -------------- | --------------------- | ----------------------------- | -------------------------------- | ----------------------------------------------------- |
| **PayOS**      | Subscription payments | ClientId, ApiKey, ChecksumKey | ✅ Production Ready              | [📖](payos-integration.md)                            |
| **Cloudinary** | Image/media upload    | CloudName, ApiKey, ApiSecret  | ✅ Implemented                   | [📖](cloudinary.md)                                   |
| **Email**      | Transactional emails  | SmtpServer, Port, ApiKey      | ✅ Implemented (Brevo + MailKit) | [📖](email-services.md)                               |
| **MailKit**    | SMTP email sending    | SMTP credentials              | ✅ Alternative provider          | [📖](email-services.md#implementation-2-mailkit-smtp) |
| **SignalR**    | Real-time updates     | WebSocket configuration       | ✅ Implemented                   | [📖](#)                                               |
| **Redis**      | Cache backend         | Connection string             | ✅ Implemented                   | [📖](#)                                               |

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
