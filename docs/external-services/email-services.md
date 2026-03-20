# Email Services Documentation

## Overview

QRDine implements a flexible email service architecture supporting multiple email providers:

- **Brevo API** (Primary - Active)
- **MailKit SMTP** (Alternative/Fallback)

The implementation follows the **Onion Architecture** pattern with abstraction in the Application layer and concrete implementations in the Infrastructure layer.

---

## 1. Architecture & Design

### Layer Organization

```
┌─────────────────────────────────────────────────────────────┐
│ Application Layer (Abstraction)                             │
│ ├── IEmailService (interface)                               │
│ └── EmailTemplates (static templates)                       │
└─────────────────────────┬───────────────────────────────────┘
                          │ Injected into Handlers
┌─────────────────────────▼───────────────────────────────────┐
│ Infrastructure Layer (Implementation)                       │
│ ├── BrevoApiEmailService ✅ Currently Active               │
│ ├── MailKitEmailService (Alternative)                       │
│ ├── EmailSettings (Configuration Model)                     │
│ └── EmailServiceCollectionExtensions (DI)                   │
└─────────────────────────────────────────────────────────────┘
```

### Complete File Locations

| Component                  | Path                                                                                    | Purpose                   |
| -------------------------- | --------------------------------------------------------------------------------------- | ------------------------- |
| **Interface**              | `src/QRDine.Application.Common/Abstractions/Email/IEmailService.cs`                     | Email service abstraction |
| **Brevo Implementation**   | `src/QRDine.Infrastructure/Email/BrevoApiEmailService.cs`                               | REST API email provider   |
| **MailKit Implementation** | `src/QRDine.Infrastructure/Email/MailKitEmailService.cs`                                | SMTP email provider       |
| **Configuration Model**    | `src/QRDine.Infrastructure/Configuration/EmailSettings.cs`                              | Settings container        |
| **DI Extension**           | `src/QRDine.API/DependencyInjection/Infrastructure/EmailServiceCollectionExtensions.cs` | Service registration      |
| **Email Templates**        | `src/QRDine.Application.Common/Templates/EmailTemplates.cs`                             | HTML email templates      |
| **Entry Point**            | `src/QRDine.API/Program.cs`                                                             | DI registration call      |

---

## 2. Service Interface

### IEmailService

**Location**: `src/QRDine.Application.Common/Abstractions/Email/IEmailService.cs`

```csharp
namespace QRDine.Application.Common.Abstractions.Email
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject line</param>
        /// <param name="htmlMessage">HTML-formatted message body</param>
        /// <param name="cancellationToken">Cancellation token for async operation</param>
        /// <returns>Completed task on success; throws exception on failure</returns>
        Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlMessage,
            CancellationToken cancellationToken = default);
    }
}
```

**Method Signature**:

- **toEmail** (string): Recipient email address. Must be valid RFC 5322 format.
- **subject** (string): Email subject line. No length restrictions at interface level.
- **htmlMessage** (string): HTML-formatted message body. Supports full HTML/CSS.
- **cancellationToken** (CancellationToken): Optional. Defaults to `CancellationToken.None`.

**Return Type**: `Task` - Async operation. No return value on success.

**Exception Behavior**: Throws exceptions on failure. Caller responsible for error handling.

---

## 3. Email Service Implementations

### Implementation 1: Brevo API (Primary - Active)

**Location**: `src/QRDine.Infrastructure/Email/BrevoApiEmailService.cs`

#### Overview

Brevo is a cloud-based email marketing and transactional email platform. The implementation uses Brevo's REST API for sending emails over HTTP.

#### Complete Implementation Code

```csharp
using QRDine.Application.Common.Abstractions.Email;
using QRDine.Infrastructure.Configuration;
using System.Net.Http.Headers;

namespace QRDine.Infrastructure.Email
{
    public class BrevoApiEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor - Dependencies injected by DI container.
        /// </summary>
        /// <param name="options">Email configuration containing Brevo API key</param>
        /// <param name="httpClient">HttpClient factory provided instance</param>
        public BrevoApiEmailService(IOptions<EmailSettings> options, HttpClient httpClient)
        {
            _settings = options.Value;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Sends email via Brevo REST API.
        /// </summary>
        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlMessage,
            CancellationToken cancellationToken = default)
        {
            // 1. Brevo API endpoint
            var url = "https://api.brevo.com/v3/smtp/email";

            // 2. Build request payload matching Brevo API spec
            var payload = new
            {
                sender = new
                {
                    name = _settings.SenderName,        // e.g., "QRDine Restaurant"
                    email = _settings.SenderEmail       // e.g., "noreply@qrdine.com"
                },
                to = new[]
                {
                    new { email = toEmail }             // Recipient email
                },
                subject = subject,                       // Subject line
                htmlContent = htmlMessage                // HTML body
            };

            // 3. Serialize payload to JSON
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(
                jsonPayload,
                Encoding.UTF8,
                "application/json");

            // 4. Configure HTTP request headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "api-key",                              // Brevo API key header
                _settings.Password);                    // API key value
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // 5. Send POST request to Brevo API
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            // 6. Check if request succeeded
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Lỗi gửi mail qua Brevo API: {error}");
            }
        }
    }
}
```

#### Request/Response Details

**Request Payload** (JSON):

```json
{
  "sender": {
    "name": "QRDine Restaurant",
    "email": "noreply@qrdine.com"
  },
  "to": [
    {
      "email": "customer@example.com"
    }
  ],
  "subject": "Kích hoạt tài khoản QRDine của bạn",
  "htmlContent": "<html>...</html>"
}
```

**HTTP Headers**:

- Content-Type: `application/json`
- Accept: `application/json`
- api-key: `{BREVO_API_KEY}` (from `EmailSettings.Password`)

**Response**:

- **Success (200-299)**: Empty response body. Email queued for delivery.
- **Error (4xx-5xx)**: JSON error response. Exception thrown with error details.

#### Characteristics

| Feature               | Detail                                 |
| --------------------- | -------------------------------------- |
| **Protocol**          | HTTP REST API                          |
| **Authentication**    | API Key in header                      |
| **Connection**        | Stateless, no SMTP connection required |
| **Scalability**       | Cloud-managed, high throughput         |
| **Retry Logic**       | Brevo handles retries internally       |
| **Required Port**     | 443 (HTTPS only)                       |
| **Configuration Key** | `EmailSettings.Password` (API key)     |

#### Advantages

✅ No local SMTP server required  
✅ HTTP-based (better for containerized deployments)  
✅ Brevo handles deliverability and bounce management  
✅ Built-in analytics and reporting  
✅ Automatic retry logic  
✅ No rate limiting in typical usage

#### Dependencies

- `System.Net.Http` (HttpClient)
- `System.Text.Json` (JSON serialization)
- `System.Net.Http.Headers`

---

### Implementation 2: MailKit SMTP (Alternative)

**Location**: `src/QRDine.Infrastructure/Email/MailKitEmailService.cs`

#### Overview

MailKit is a popular .NET library for SMTP email sending. This implementation connects directly to an SMTP server for sending emails.

#### Complete Implementation Code

```csharp
using QRDine.Application.Common.Abstractions.Email;
using QRDine.Infrastructure.Configuration;

namespace QRDine.Infrastructure.Email
{
    public class MailKitEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        /// <summary>
        /// Constructor - Receives email configuration.
        /// </summary>
        public MailKitEmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        /// <summary>
        /// Sends email via SMTP using MailKit.
        /// </summary>
        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlMessage,
            CancellationToken cancellationToken = default)
        {
            // 1. Create MIME message
            var email = new MimeMessage();

            // 2. Set sender (From header)
            email.From.Add(
                new MailboxAddress(
                    _settings.SenderName,              // Display name
                    _settings.SenderEmail));           // Email address

            // 3. Set recipient (To header)
            email.To.Add(MailboxAddress.Parse(toEmail));

            // 4. Set subject
            email.Subject = subject;

            // 5. Build email body with HTML content
            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage              // HTML-formatted message
            };
            email.Body = builder.ToMessageBody();

            // 6. Send via SMTP connection
            using var smtp = new SmtpClient();
            try
            {
                // Connect to SMTP server with TLS security
                // SecureSocketOptions.StartTls: Upgrade connection to TLS after connect
                await smtp.ConnectAsync(
                    _settings.SmtpServer,           // e.g., "smtp.gmail.com"
                    _settings.Port,                 // e.g., 587
                    SecureSocketOptions.StartTls,   // TLS security option
                    cancellationToken);

                // Authenticate with SMTP server
                await smtp.AuthenticateAsync(
                    _settings.SenderEmail,          // Username
                    _settings.Password,             // Password
                    cancellationToken);

                // Send the email
                await smtp.SendAsync(email, cancellationToken);
            }
            finally
            {
                // Disconnect gracefully (clean up connection)
                // Parameter 'true' ensures graceful disconnection
                await smtp.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
```

#### Connection Details

**SMTP Connection Parameters**:

- **SmtpServer** (string): Hostname of SMTP server
  - Example: `smtp.gmail.com`, `smtp-relay.brevo.com`, `mail.example.com`
- **Port** (int): SMTP port number
  - 25: Plain SMTP (legacy, usually blocked)
  - 587: SMTP with STARTTLS (recommended, encrypted)
  - 465: SMTPS (implicit SSL, encrypted from start)
- **SecureSocketOptions**: Security protocol
  - `None`: Unencrypted (port 25 - not recommended)
  - `StartTls`: TLS after connect (port 587 - recommended)
  - `SslOnConnect`: SSL/TLS from start (port 465)

**Authentication**:

- Username: Usually `SenderEmail` or dedicated SMTP username
- Password: SMTP password (application-specific for Gmail)

#### Email Message Structure

**MIME Message Properties**:

- **From**: Sender address and display name
- **To**: Recipient email address(es)
- **Subject**: Email subject line
- **Body**: HTML-formatted message (BodyBuilder)

**MIME Types**:

- MailKit automatically sets `Content-Type: text/html` for HTML messages
- Charset: UTF-8 default

#### Characteristics

| Feature                | Detail                               |
| ---------------------- | ------------------------------------ |
| **Protocol**           | SMTP with optional TLS               |
| **Authentication**     | Username/Password                    |
| **Connection**         | Direct TCP connection to SMTP server |
| **Port Flexibility**   | Supports 25, 465, 587                |
| **Connection Pooling** | None - new connection per send       |
| **Retry Logic**        | Application must implement           |
| **Delivery Guarantee** | None - depends on SMTP server        |

#### Advantages

✅ Works with any SMTP provider  
✅ Full control over email sending  
✅ No external API/service required  
✅ Lower latency for single emails  
✅ Good for development/testing

#### Disadvantages

❌ Requires SMTP server access  
❌ New connection per email (slower at scale)  
❌ No built-in retry/handling  
❌ Delivery issues not visible  
❌ More configuration needed

#### Dependencies (NuGet Package)

- `MailKit` (v4.15.1+)
- `MimeKit` (included with MailKit)

---

## 4. Configuration

### EmailSettings Model

**Location**: `src/QRDine.Infrastructure/Configuration/EmailSettings.cs`

```csharp
namespace QRDine.Infrastructure.Configuration
{
    /// <summary>
    /// Email service configuration container.
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// SMTP server hostname or Brevo endpoint.
        /// Examples: "smtp.gmail.com", "smtp-relay.brevo.com"
        /// </summary>
        public string SmtpServer { get; set; } = default!;

        /// <summary>
        /// SMTP port number.
        /// - 25: Plain SMTP
        /// - 465: SMTPS (implicit SSL)
        /// - 587: SMTP with STARTTLS
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Sender email address (From header).
        /// </summary>
        public string SenderEmail { get; set; } = default!;

        /// <summary>
        /// Sender display name (shown to recipients).
        /// </summary>
        public string SenderName { get; set; } = default!;

        /// <summary>
        /// SMTP password or Brevo API key.
        /// For Brevo: xsmtppwd_XXXX or XXXX-YYYY-ZZZZ format
        /// For SMTP: actual password
        /// For Gmail: 16-character app-specific password
        /// </summary>
        public string Password { get; set; } = default!;
    }
}
```

### appsettings.json Configuration

**Location**: `src/QRDine.API/appsettings.json`

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp-relay.brevo.com",
    "Port": 2525,
    "SenderEmail": "your-email@example.com",
    "SenderName": "QR Dine",
    "Password": "<your-brevo-smtp-key>"
  }
}
```

### Configuration by Provider

#### Brevo (Current)

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp-relay.brevo.com",
    "Port": 2525,
    "SenderEmail": "your-registered-email@company.com",
    "SenderName": "QRDine Notifications",
    "Password": "<your-brevo-smtp-key>"
  }
}
```

**Getting Brevo API Key**:

1. Sign up at https://www.brevo.com/
2. Go to Settings → SMTP & API
3. Copy SMTP API key (starts with `xkeysib-`)
4. Paste in `Password` field

#### Gmail (SMTP Alternative)

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "QRDine",
    "Password": "xxxx xxxx xxxx xxxx"
  }
}
```

**Setup**:

1. Enable 2-Factor Authentication on Gmail account
2. Create App Password (not your Gmail password)
3. Use 16-character App Password in `Password` field

#### Office 365

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.office365.com",
    "Port": 587,
    "SenderEmail": "noreply@company.onmicrosoft.com",
    "SenderName": "QRDine Company",
    "Password": "azure-account-password"
  }
}
```

#### Custom SMTP Server

```json
{
  "EmailSettings": {
    "SmtpServer": "mail.yourdomain.com",
    "Port": 587,
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "Your Organization",
    "Password": "smtp-password"
  }
}
```

### Environment Variables Setup

**Option 1: System Environment Variables** (High Priority)

```powershell
# PowerShell
$env:EmailSettings__SmtpServer = "smtp-relay.brevo.com"
$env:EmailSettings__Port = "2525"
$env:EmailSettings__SenderEmail = "noreply@qrdine.com"
$env:EmailSettings__SenderName = "QRDine"
$env:EmailSettings__Password = "xkeysib-..."
```

**Option 2: .env File** (with `dotenv` loader)

```
EmailSettings__SmtpServer=smtp-relay.brevo.com
EmailSettings__Port=2525
EmailSettings__SenderEmail=noreply@qrdine.com
EmailSettings__SenderName=QRDine
EmailSettings__Password=xkeysib-...
```

**Option 3: Docker Environment**

```yaml
# docker-compose.yml
environment:
  - EmailSettings__SmtpServer=smtp-relay.brevo.com
  - EmailSettings__Port=2525
  - EmailSettings__SenderEmail=noreply@qrdine.com
  - EmailSettings__SenderName=QRDine
  - EmailSettings__Password=${BREVO_API_KEY}
```

---

## 5. Dependency Injection Setup

### Service Registration

**Location**: `src/QRDine.API/DependencyInjection/Infrastructure/EmailServiceCollectionExtensions.cs`

```csharp
using QRDine.Application.Common.Abstractions.Email;
using QRDine.Infrastructure.Configuration;
using QRDine.Infrastructure.Email;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    /// <summary>
    /// Extension methods to register email services in DI container.
    /// </summary>
    public static class EmailServiceCollectionExtensions
    {
        /// <summary>
        /// Adds email service to the DI container.
        /// Registers EmailSettings configuration and selects implementation.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddEmailService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Bind EmailSettings from configuration section
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // 2. Register implementation (switch as needed)
            // Currently active: BrevoApiEmailService
            services.AddTransient<IEmailService, BrevoApiEmailService>();

            // Alternative: Uncomment to use MailKit
            // services.AddTransient<IEmailService, MailKitEmailService>();

            return services;
        }
    }
}
```

**Lifetime**: `Transient`

- New instance created per injection request
- Appropriate for stateless services
- Each email send gets fresh service instance

### Registration Call Chain

**Location**: `src/QRDine.API/Program.cs`

```csharp
builder.Services
    .AddApplicationServices(builder.Configuration)  // Main entry point

// Inside ServiceCollectionExtensions.AddApplicationServices():
    .AddInfrastructure(configuration)               // Infrastructure layer

// Inside ServiceCollectionExtensions.AddInfrastructure():
    .AddEmailService(configuration)                 // Email service registration
    .AddPayOS(configuration)
    .AddCaching(configuration)
    // ... other infrastructure services
```

**Full Chain**:

```
Program.cs
  ↓ Calls AddApplicationServices()
  ↓ Calls AddInfrastructure()
  ↓ Calls AddEmailService()
    ├─ Configure<EmailSettings>()
    └─ AddTransient<IEmailService, BrevoApiEmailService>()
```

### Dependencies Injected

When using email service in a handler:

```csharp
public class RegisterMerchantCommandHandler : IRequestHandler<RegisterMerchantCommand, bool>
{
    private readonly IEmailService _emailService;

    // ✅ IEmailService automatically injected by DI container
    public RegisterMerchantCommandHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }
}
```

**DI Resolution**:

1. Container looks for `IEmailService` registration
2. Finds `BrevoApiEmailService` (or `MailKitEmailService`)
3. Resolves `IOptions<EmailSettings>` from configuration
4. For Brevo: Resolves `HttpClient` from factory
5. Creates instance with all dependencies
6. Injects into constructor

---

## 6. Email Templates

### EmailTemplates Static Class

**Location**: `src/QRDine.Application.Common/Templates/EmailTemplates.cs`

```csharp
namespace QRDine.Application.Common.Templates
{
    /// <summary>
    /// Static class containing HTML email templates.
    /// Each method returns a formatted HTML string for a specific email type.
    /// </summary>
    public static class EmailTemplates
    {
        /// <summary>
        /// Merchant account activation email template.
        /// Sent when merchant completes registration to verify email.
        /// </summary>
        /// <param name="firstName">Merchant first name</param>
        /// <param name="lastName">Merchant last name</param>
        /// <param name="merchantName">Store/merchant business name</param>
        /// <param name="verifyLink">Account activation link (valid 15 minutes)</param>
        /// <returns>HTML-formatted email body</returns>
        public static string GetMerchantActivationTemplate(
            string firstName,
            string lastName,
            string merchantName,
            string verifyLink)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #2563eb; text-align: center;'>Chào mừng bạn đến với QRDine!</h2>

                    <p style='font-size: 16px; color: #333;'>Xin chào <strong>{firstName} {lastName}</strong>,</p>

                    <p style='font-size: 16px; color: #333;'>
                        Cảm ơn bạn đã đăng ký mở cửa hàng <strong>{merchantName}</strong> trên nền tảng của chúng tôi.
                        Để hoàn tất, vui lòng nhấn vào nút bên dưới để kích hoạt tài khoản:
                    </p>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{verifyLink}'
                           style='background-color: #2563eb; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px;'>
                            Kích Hoạt Tài Khoản
                        </a>
                    </div>

                    <p style='font-size: 14px; color: #666;'>
                        <em>* Lưu ý: Link này chỉ có hiệu lực trong vòng 15 phút và chỉ sử dụng được 1 lần.</em>
                    </p>

                    <hr style='border: none; border-top: 1px solid #eaeaea; margin: 20px 0;' />

                    <p style='font-size: 12px; color: #999; text-align: center;'>
                        Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.
                    </p>
                </div>";
        }
    }
}
```

### Template Variables & Substitution

| Variable       | Type   | Source          | Example                                          |
| -------------- | ------ | --------------- | ------------------------------------------------ |
| `firstName`    | string | User profile    | "Duy"                                            |
| `lastName`     | string | User profile    | "Duy"                                            |
| `merchantName` | string | Tenant data     | "Quán Cơm Tấm"                                   |
| `verifyLink`   | string | Generated token | `https://qrdine.me/register/verify?token=abc123` |

### Template HTML Features

**Styling**:

- Inline CSS for email client compatibility
- Max-width: 600px (mobile-friendly)
- Color scheme: Blue (#2563eb) primary brand color
- Responsive padding and margins

**Components**:

- Header: Welcome message with brand color
- Body: Personalized greeting and explanation
- CTA Button: Action link (blue, rounded)
- Footer: Disclaimers and safety notes
- Border styling for email container

**Security Notes**:

- All variables properly interpolated (no XSS with string input)
- Link validation should occur before sending
- Token should be validated server-side

### Template Usage Example

**In RegisterMerchantCommandHandler**:

```csharp
var htmlMessage = EmailTemplates.GetMerchantActivationTemplate(
    request.Dto.FirstName,
    request.Dto.LastName,
    request.Dto.MerchantName,
    verifyLink);

await _emailService.SendEmailAsync(
    request.Dto.Email,
    "Kích hoạt tài khoản QRDine của bạn",  // Subject
    htmlMessage,                             // HTML body
    cancellationToken);
```

### Future Template Methods

Based on architecture, these templates can be added:

```csharp
public static class EmailTemplates
{
    // Order notifications
    public static string GetOrderConfirmationTemplate(string orderNumber, decimal total) { }

    // Staff management
    public static string GetStaffInvitationTemplate(string staffName, string inviteLink) { }

    // Subscription
    public static string GetSubscriptionConfirmationTemplate(string planName, DateTime startDate) { }

    // Password management
    public static string GetPasswordResetTemplate(string resetLink) { }

    // Alerts
    public static string GetLowInventoryAlertTemplate(string productName) { }
}
```

---

## 7. Email Sending Workflows

### Current Email Usage

#### 1. Merchant Registration Email

**Trigger**: `RegisterMerchantCommand` executed

**Flow**:

```
User submits registration form
  ↓
RegisterMerchantCommandHandler.Handle()
  ├─ Validate merchant data
  ├─ Generate 15-min activation token
  ├─ Store token in cache
  ├─ Build activation link: {frontendUrl}/register/verify?token={token}
  ├─ Call EmailTemplates.GetMerchantActivationTemplate()
  ├─ Call IEmailService.SendEmailAsync()
  │  ├─ BrevoApiEmailService: POST to api.brevo.com/v3/smtp/email
  │  └─ MailKitEmailService: Connect SMTP, Send, Disconnect
  └─ Return success

Email receives content:
  ├─ Subject: "Kích hoạt tài khoản QRDine của bạn"
  ├─ From: noreply@qrdine.com (QRDine)
  ├─ To: {merchant_email}
  ├─ Body: HTML template with activation link
  └─ Link valid for: 15 minutes (1 use only)
```

**Sent To**: Merchant email address (from registration form)

**Subject**: "Kích hoạt tài khoản QRDine của bạn"

**Content**: Welcome email with activation CTA button

**Command Location**: `src/QRDine.Application/Features/Identity/Commands/RegisterMerchant/RegisterMerchantCommandHandler.cs`

---

## 8. Error Handling & Resilience

### Error Scenarios

#### Scenario 1: Brevo API Failure

```csharp
// In BrevoApiEmailService
if (!response.IsSuccessStatusCode)
{
    var error = await response.Content.ReadAsStringAsync(cancellationToken);
    throw new Exception($"Lỗi gửi mail qua Brevo API: {error}");
}
```

**Handling**:

1. HTTP response not in 2xx range
2. Read error details from response body
3. Throw exception with error message
4. Propagates to handler → Caught by global exception handler

**Common Errors**:

- 401 Unauthorized: Invalid API key
- 400 Bad Request: Invalid email format
- 429 Too Many Requests: Rate limit exceeded
- 500-599: Brevo server error

#### Scenario 2: SMTP Connection Failure (MailKit)

```csharp
// In MailKitEmailService
try
{
    await smtp.ConnectAsync(...);
    await smtp.AuthenticateAsync(...);
    await smtp.SendAsync(...);
}
finally
{
    await smtp.DisconnectAsync(true, cancellationToken);
}
```

**Possible Exceptions**:

- `SmtpCommandException`: SMTP protocol error
- `SmtpProtocolException`: Connection protocol issue
- `AuthenticationException`: Invalid credentials
- `IOException`: Network failure
- `OperationCanceledException`: Timeout/cancellation

#### Scenario 3: Configuration Missing

```csharp
// In Program.cs during startup
services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

// If section missing:
// EmailSettings fields are null/uninitialized
// SendEmailAsync throws NullReferenceException at runtime
```

**Validation Point Needed**:

```csharp
var settings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
if (string.IsNullOrEmpty(settings?.SmtpServer))
    throw new InvalidOperationException("EmailSettings not configured");
```

### Current Error Handling Strategy

**Location Responsibility**:

- ✅ Service Layer: Throws exceptions
- ✅ Handler Layer: Calls via try-catch or async propagation
- ✅ Middleware: Global exception handler

**Error Flow**:

```
Service Exception
  ↓ Propagates up
Handler Level
  ↓ Propagates up (no catch currently)
Middleware: ExceptionHandlingMiddleware
  ├─ Catch and log
  ├─ Format response
  └─ Return 500 error
```

### Missing Resilience Features

Currently **NOT implemented**:

- ❌ Retry logic (manual implementation needed)
- ❌ Circuit breaker pattern
- ❌ Fallback provider (Brevo → MailKit)
- ❌ Email queue/background job
- ❌ Delivery status tracking
- ❌ Request/response logging

### Recommended Enhancements

**1. Retry Logic**:

```csharp
private async Task SendWithRetryAsync(Func<Task> action, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            await action();
            return;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
        }
    }
}
```

**2. Failover to MailKit**:

```csharp
try
{
    await _breevoService.SendEmailAsync(...);
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Brevo failed, trying MailKit");
    await _mailkitService.SendEmailAsync(...);
}
```

**3. Logging**:

```csharp
_logger.LogInformation("Sending email to {Email}", toEmail);
// ... send ...
_logger.LogInformation("Email sent successfully");
```

---

## 9. Global Usings & Dependencies

### QRDine.Infrastructure Global Usings

**Location**: `src/QRDine.Infrastructure/GlobalUsings.cs`

Email-related imports:

```csharp
global using MailKit.Net.Smtp;
global using MailKit.Security;
global using MimeKit;
global using System.Text.Json;
global using Microsoft.Extensions.Options;
```

**For Email Services**:

- `MailKit.Net.Smtp.SmtpClient`: SMTP connection
- `MailKit.Security.SecureSocketOptions`: TLS/SSL options
- `MimeKit.MimeMessage`: Email message structure
- `MimeKit.BodyBuilder`: Email body builder
- `System.Text.Json`: JSON serialization (Brevo)
- `Microsoft.Extensions.Options.IOptions<T>`: Configuration injection

### NuGet Dependencies

**Location**: `src/QRDine.Infrastructure/QRDine.Infrastructure.csproj`

```xml
<PackageReference Include="MailKit" Version="4.15.1" />
```

**MailKit Package Contents**:

- `MailKit.Net.Smtp`: SMTP client
- `MimeKit`: MIME message handling
- Transitive dependency: `System.Net.Security` (TLS support)

### HTTP Client Configuration

**Location**: `src/QRDine.API/Program.cs`

```csharp
builder.Services.AddHttpClient()  // Enables HttpClient injection
```

**For Brevo**: HttpClient injected into `BrevoApiEmailService`

- Manages HTTP connection pooling
- Handles DNS caching
- Manages timeouts

---

## 10. Configuration & Deployment

### Development Environment

**File**: `src/QRDine.API/appsettings.json`

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp-relay.brevo.com",
    "Port": 2525,
    "SenderEmail": "dev@qrdine.local",
    "SenderName": "QRDine Dev",
    "Password": "dev_key_xyz"
  }
}
```

### Production Environment

**Method 1: Environment Variables** (Recommended)

```powershell
# Set in production hosting environment
$env:EmailSettings__SmtpServer = "smtp-relay.brevo.com"
$env:EmailSettings__Port = "2525"
$env:EmailSettings__SenderEmail = "noreply@qrdine.com"
$env:EmailSettings__SenderName = "QRDine"
$env:EmailSettings__Password = "<your-brevo-smtp-key>"
```

**Method 2: Docker Environment**

```yaml
services:
  api:
    environment:
      - EmailSettings__SmtpServer=smtp-relay.brevo.com
      - EmailSettings__Port=2525
      - EmailSettings__SenderEmail=noreply@qrdine.com
      - EmailSettings__SenderName=QRDine
      - EmailSettings__Password=${BREVO_API_KEY}
```

**Method 3: Kubernetes ConfigMap**

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: email-config
data:
  EmailSettings__SmtpServer: "smtp-relay.brevo.com"
  EmailSettings__Port: "2525"
  EmailSettings__SenderEmail: "noreply@qrdine.com"
  EmailSettings__SenderName: "QRDine"
---
apiVersion: v1
kind: Secret
metadata:
  name: email-secrets
type: Opaque
stringData:
  EmailSettings__Password: <base64-encoded-api-key>
```

### Switching Implementations

**From Brevo to MailKit**:

Edit `src/QRDine.API/DependencyInjection/Infrastructure/EmailServiceCollectionExtensions.cs`:

```csharp
// Change this line:
services.AddTransient<IEmailService, BrevoApiEmailService>();

// To this:
services.AddTransient<IEmailService, MailKitEmailService>();

// And update appsettings.json to SMTP settings
```

---

## 11. Testing Email Services

### Unit Testing

```csharp
[TestClass]
public class EmailServiceTests
{
    [TestMethod]
    public async Task SendEmailAsync_WithValidInput_ShouldSucceed()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            .Respond(HttpStatusCode.OK);

        var httpClient = new HttpClient(mockHttp);
        var settings = Options.Create(new EmailSettings
        {
            SmtpServer = "smtp-relay.brevo.com",
            Port = 2525,
            SenderEmail = "test@qrdine.com",
            SenderName = "Test",
            Password = "test-key"
        });

        var service = new BrevoApiEmailService(settings, httpClient);

        // Act
        await service.SendEmailAsync(
            "recipient@example.com",
            "Test Subject",
            "<h1>Test</h1>");

        // Assert
        mockHttp.VerifyNoOutstandingExpectation();
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class EmailIntegrationTests
{
    [TestMethod]
    [Ignore("Integration test - requires Brevo API key")]
    public async Task SendMerchantActivationEmail_Success()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection()
            .Configure<EmailSettings>(configuration.GetSection("EmailSettings"))
            .AddTransient<IEmailService, BrevoApiEmailService>()
            .AddHttpClient();

        var provider = services.BuildServiceProvider();
        var emailService = provider.GetRequiredService<IEmailService>();

        // Act
        var verifyLink = "https://localhost:5173/register/verify?token=abc123";
        var html = EmailTemplates.GetMerchantActivationTemplate(
            "John", "Doe", "Quán Cơm Tấm", verifyLink);

        await emailService.SendEmailAsync(
            "test-email@example.com",
            "Kích hoạt tài khoản QRDine",
            html);

        // Assert - Check via Brevo dashboard
    }
}
```

### Manual Testing Checklist

- [ ] Email sends to valid address
- [ ] Email subject correct
- [ ] HTML renders properly
- [ ] Links work in email client
- [ ] Sender name/address displays correctly
- [ ] Configuration loads from appsettings
- [ ] Configuration loads from environment variables
- [ ] Switch between Brevo and MailKit works
- [ ] Error message clear on API failure
- [ ] Handles special characters (Vietnamese)
- [ ] Email arrives within 5 seconds

---

## 12. Monitoring & Logging

### Current Logging

**Locations**:

- ✅ Exception thrown in service
- ✅ Not explicitly logged in most cases
- ✅ Caught by global exception handler middleware

### Recommended Monitoring

**1. Email Send Metrics**:

```csharp
private readonly ILogger<BrevoApiEmailService> _logger;

public async Task SendEmailAsync(...)
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        // ... send email ...
        _logger.LogInformation(
            "Email sent to {Email} in {Elapsed}ms",
            toEmail,
            stopwatch.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        throw;
    }
}
```

**2. Health Check**:

```csharp
services.AddHealthChecks()
    .AddCheck("email", new EmailHealthCheck(_emailService));
```

**3. Distributed Tracing**:

```csharp
using var activity = new System.Diagnostics.Activity("SendEmail");
activity.Start();
try
{
    await _emailService.SendEmailAsync(...);
}
finally
{
    activity.Stop();
}
```

---

## 13. Summary & Quick Reference

### Key Files

| File                                  | Purpose                 |
| ------------------------------------- | ----------------------- |
| `IEmailService.cs`                    | Service interface       |
| `BrevoApiEmailService.cs`             | REST API implementation |
| `MailKitEmailService.cs`              | SMTP implementation     |
| `EmailSettings.cs`                    | Configuration model     |
| `EmailServiceCollectionExtensions.cs` | DI registration         |
| `EmailTemplates.cs`                   | HTML templates          |

### Key Methods

| Method                                     | Use Case                  |
| ------------------------------------------ | ------------------------- |
| `SendEmailAsync(to, subject, html, token)` | Send email to recipient   |
| `GetMerchantActivationTemplate(...)`       | Merchant welcome template |

### Configuration Keys

| Key                         | Environment Variable         |
| --------------------------- | ---------------------------- |
| `EmailSettings:SmtpServer`  | `EmailSettings__SmtpServer`  |
| `EmailSettings:Port`        | `EmailSettings__Port`        |
| `EmailSettings:SenderEmail` | `EmailSettings__SenderEmail` |
| `EmailSettings:SenderName`  | `EmailSettings__SenderName`  |
| `EmailSettings:Password`    | `EmailSettings__Password`    |

### Common Issues & Solutions

| Issue                    | Cause                   | Solution                                           |
| ------------------------ | ----------------------- | -------------------------------------------------- |
| 401 Unauthorized (Brevo) | Invalid API key         | Verify Brevo API key in `Password` field           |
| SMTP Auth Failed         | Wrong credentials       | Check SmtpServer, Port, SenderEmail match provider |
| Port 25 blocked          | ISP/firewall limitation | Use port 587 (TLS) or 465 (SSL)                    |
| Email never arrives      | Invalid recipient       | Validate email format before sending               |
| HTML not rendering       | Wrong encoding          | Ensure UTF-8 encoding in Content-Type              |

### Provider Comparison

| Feature         | Brevo               | MailKit     |
| --------------- | ------------------- | ----------- |
| **Setup Ease**  | Simple (2 steps)    | Moderate    |
| **Performance** | Medium (HTTP)       | High (SMTP) |
| **Scalability** | Excellent           | Fair        |
| **Monitoring**  | Dashboard included  | Manual      |
| **Cost**        | Free tier available | Free        |
| **SLA/Support** | SLA provided        | Community   |

---

## 14. Future Enhancements

### Planned Features

1. **Email Queue** - Background job processing with Hangfire
2. **Delivery Tracking** - Store send status and track bounces
3. **Template Engine** - Liquid/Razor templates with variables
4. **Attachment Support** - Send files with emails
5. **Unsubscribe Management** - List-Unsubscribe header
6. **Analytics** - Track opens and clicks
7. **A/B Testing** - Multiple template variants
8. **Fallback Chain** - Automatic failover between providers
9. **Rate Limiting** - Per-user/per-domain send limits
10. **DKIM/SPF** - Email authentication setup guide

### Extensibility Points

Current architecture easily supports:

- Adding new email providers (implement `IEmailService`)
- Multiple sender addresses (extend `EmailSettings`)
- Batch sending (add `SendBatchEmailAsync`)
- Scheduled emails (add timestamp parameter)
- Template rendering (add `ITemplateEngine`)

---

## Appendix: API Reference

### Brevo API Response Codes

| Code    | Meaning                                 |
| ------- | --------------------------------------- |
| 200-299 | Success - Email queued                  |
| 400     | Bad Request - Invalid payload           |
| 401     | Unauthorized - Invalid API key          |
| 403     | Forbidden - Account suspended           |
| 422     | Unprocessable Entity - Validation error |
| 429     | Too Many Requests - Rate limited        |
| 500-599 | Server Error - Brevo issue              |

### SMTP Status Codes

| Code | Meaning                 |
| ---- | ----------------------- |
| 220  | Service ready           |
| 250  | OK                      |
| 354  | Start mail input        |
| 421  | Service unavailable     |
| 450  | Mailbox unavailable     |
| 500  | Syntax error            |
| 530  | Authentication required |

---

**Document Version**: 1.0  
**Last Updated**: March 21, 2026  
**Scope**: QRDine Email Services (Brevo & MailKit)  
**Status**: Complete Implementation
