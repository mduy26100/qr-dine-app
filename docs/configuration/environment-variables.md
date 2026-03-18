# Environment Variables Reference

Complete reference of all configurable environment variables for QRDine.

## Core API Configuration

| Variable                                              | Type   | Examples                                       | Required  | Notes                         |
| ----------------------------------------------------- | ------ | ---------------------------------------------- | --------- | ----------------------------- |
| `ASPNETCORE_ENVIRONMENT`                              | string | `Development`, `Staging`, `Production`         | Yes       | Determines config file loaded |
| `ASPNETCORE_URLS`                                     | string | `http://localhost:5000;https://localhost:5001` | Yes       | Server binding addresses      |
| `ASPNETCORE_Kestrel__Certificates__Default__Path`     | string | `/path/to/cert.pfx`                            | No (dev)  | HTTPS certificate path        |
| `ASPNETCORE_Kestrel__Certificates__Default__Password` | string | `cert-password`                                | No (prod) | Certificate password          |

## Database Configuration

| Variable                                 | Type   | Examples                                        | Required      | Notes                           |
| ---------------------------------------- | ------ | ----------------------------------------------- | ------------- | ------------------------------- |
| `ConnectionStrings__DefaultConnection`   | string | `Server=localhost;Database=QRDine;...`          | Yes           | SQL Server connection string    |
| `ConnectionStrings__AuditConnection`     | string | `Server=audit-server;Database=QRDine_Audit;...` | No            | Separate audit database         |
| `DatabaseSettings__Port`                 | int    | `1433`                                          | No            | SQL Server port (default: 1433) |
| `DatabaseSettings__Timeout`              | int    | `30`                                            | No            | Connection timeout in seconds   |
| `DatabaseSettings__EnableDetailedErrors` | bool   | `false`                                         | No (dev only) | EF Core detailed errors         |

**Development Example:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QRDine_Dev;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

**Production Example:**

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=qrdine-db.database.windows.net,1433;Database=QRDine;User Id=admin;Password=SecurePassword123!;"
```

## JWT Configuration

| Variable                        | Type   | Examples                             | Required | Notes                                |
| ------------------------------- | ------ | ------------------------------------ | -------- | ------------------------------------ |
| `Jwt__Secret`                   | string | `64-char-random-string-min-256-bits` | Yes      | `dotnet user-secrets set`            |
| `Jwt__ValidIssuer`              | string | `https://qrdine.com`                 | Yes      | Token issuer claim                   |
| `Jwt__ValidAudience`            | string | `https://qrdine.com`                 | Yes      | Token audience claim                 |
| `Jwt__AccessTokenExpiryMinutes` | int    | `10`                                 | No       | Access token lifetime (default: 10)  |
| `Jwt__RefreshTokenExpiryDays`   | int    | `7`                                  | No       | Refresh token lifetime (default: 7)  |
| `Jwt__SecureTokens`             | bool   | `true`                               | No       | Use HttpOnly cookies (default: true) |

**Generation Command:**

```bash
# Generate secure JWT secret
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 32)"
```

## Cloudinary Configuration

| Variable                      | Type   | Examples                | Required | Notes                            |
| ----------------------------- | ------ | ----------------------- | -------- | -------------------------------- |
| `Cloudinary__CloudName`       | string | `your-cloud-id`         | Yes      | Cloudinary account identifier    |
| `Cloudinary__ApiKey`          | string | `123456789`             | Yes      | Cloudinary API key               |
| `Cloudinary__ApiSecret`       | string | `your-api-secret`       | Yes      | Cloudinary API secret            |
| `Cloudinary__UploadFolder`    | string | `qrdine-dev/`           | No       | Folder prefix for uploads        |
| `Cloudinary__AllowedFormats`  | string | `jpg,jpeg,png,gif,webp` | No       | Comma-separated formats          |
| `Cloudinary__MaxFileSize`     | int    | `5242880`               | No       | Max size in bytes (default: 5MB) |
| `Cloudinary__Transformations` | string | `q_auto,f_auto,w_800`   | No       | Default transformations          |

**Development Setup:**

```json
{
  "Cloudinary": {
    "CloudName": "dev-cloud",
    "ApiKey": "dev-api-key",
    "ApiSecret": "dev-api-secret",
    "UploadFolder": "qrdine-dev/"
  }
}
```

## Email Configuration

### Email Provider Selection

| Variable          | Value       | When to Use                      |
| ----------------- | ----------- | -------------------------------- |
| `Email__Provider` | `Gmail`     | Dev/Small projects               |
| `Email__Provider` | `Office365` | Enterprise Microsoft environment |
| `Email__Provider` | `SendGrid`  | Production scale                 |
| `Email__Provider` | `SMTP`      | Custom SMTP server               |

### Generic SMTP

| Variable                 | Type   | Examples               | Required | Notes                        |
| ------------------------ | ------ | ---------------------- | -------- | ---------------------------- |
| `Email__Provider`        | string | `SMTP`                 | Yes      | SMTP configuration           |
| `Email__SmtpServer`      | string | `smtp.gmail.com`       | Yes      | SMTP hostname                |
| `Email__SmtpPort`        | int    | `587`                  | Yes      | SMTP port (587 for TLS)      |
| `Email__SmtpUsername`    | string | `noreply@qrdine.com`   | Yes      | SMTP authentication username |
| `Email__SmtpPassword`    | string | `app-password`         | Yes      | SMTP authentication password |
| `Email__EnableSSL`       | bool   | `true`                 | No       | Use TLS/SSL (default: true)  |
| `Email__FromAddress`     | string | `noreply@qrdine.com`   | Yes      | Sender email address         |
| `Email__FromDisplayName` | string | `QRDine Notifications` | No       | Sender display name          |

**SMTP (Gmail) Example:**

```json
{
  "Email": {
    "Provider": "SMTP",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromAddress": "noreply@qrdine.com",
    "FromDisplayName": "QRDine"
  }
}
```

### Gmail

```json
{
  "Email": {
    "Provider": "Gmail",
    "FromAddress": "your-email@gmail.com",
    "AppPassword": "16-char-app-specific-password"
  }
}
```

**Note:** Requires 2FA enabled and app-specific password generated.

### Office 365

```json
{
  "Email": {
    "Provider": "Office365",
    "FromAddress": "noreply@company.onmicrosoft.com",
    "Username": "email@company.com",
    "Password": "azure-password"
  }
}
```

### SendGrid

```json
{
  "Email": {
    "Provider": "SendGrid",
    "ApiKey": "SG.xxx-sendgrid-api-key",
    "FromAddress": "noreply@qrdine.com",
    "FromDisplayName": "QRDine"
  }
}
```

## Redis Configuration (Caching)

| Variable                    | Type   | Examples         | Required               | Notes                           |
| --------------------------- | ------ | ---------------- | ---------------------- | ------------------------------- |
| `Redis__ConnectionString`   | string | `localhost:6379` | Yes (if cache enabled) | Redis server address:port       |
| `Redis__InstanceName`       | string | `qrdine_`        | No                     | Key prefix for cache entries    |
| `Redis__DefaultExpiry`      | int    | `3600`           | No                     | Default TTL in seconds (1 hour) |
| `Redis__AbortOnConnectFail` | bool   | `false`          | No                     | Fail fast on connection error   |

**Local Development:**

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "qrdine_dev_"
  }
}
```

**Production (Azure Cache for Redis):**

```bash
$env:Redis__ConnectionString = "qrdine.redis.cache.windows.net:6380"
$env:Redis__Password = "your-redis-access-key"
$env:Redis__SSL = "true"
```

## Rate Limiting

| Variable                      | Type | Examples | Required           | Notes                                |
| ----------------------------- | ---- | -------- | ------------------ | ------------------------------------ |
| `RateLimit__Enabled`          | bool | `true`   | No (default: true) | Enable rate limiting                 |
| `RateLimit__LoginAttempts`    | int  | `5`      | No                 | Failed login attempts before lockout |
| `RateLimit__LoginWindow`      | int  | `15`     | No                 | Lockout window in minutes            |
| `RateLimit__RegisterAttempts` | int  | `3`      | No                 | Registration attempts per window     |
| `RateLimit__RegisterWindow`   | int  | `60`     | No                 | Registration window in minutes       |

## CORS Configuration

| Variable                 | Type  | Examples                                              | Required | Notes                     |
| ------------------------ | ----- | ----------------------------------------------------- | -------- | ------------------------- |
| `Cors__AllowedOrigins`   | array | `["http://localhost:3000", "https://app.qrdine.com"]` | Yes      | Whitelisted origins       |
| `Cors__AllowedMethods`   | array | `["GET", "POST", "PUT", "DELETE"]`                    | No       | Allowed HTTP methods      |
| `Cors__AllowedHeaders`   | array | `["Content-Type", "Authorization"]`                   | No       | Allowed headers           |
| `Cors__AllowCredentials` | bool  | `true`                                                | No       | Allow cookies in requests |

**Development:**

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"],
    "AllowCredentials": true
  }
}
```

**Production:**

```json
{
  "Cors": {
    "AllowedOrigins": ["https://app.qrdine.com", "https://admin.qrdine.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowCredentials": true
  }
}
```

## Logging Configuration

| Variable                                  | Type   | Examples                               | Required | Notes                          |
| ----------------------------------------- | ------ | -------------------------------------- | -------- | ------------------------------ |
| `Logging__LogLevel__Default`              | string | `Information`                          | No       | Default log level              |
| `Logging__LogLevel__Microsoft`            | string | `Warning`                              | No       | Framework logging              |
| `Logging__LogLevel__QRDine`               | string | `Debug`                                | No       | Application logging (dev only) |
| `ApplicationInsights__InstrumentationKey` | string | `00000000-0000-0000-0000-000000000000` | No       | Azure Application Insights key |

**Development:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "QRDine": "Debug"
    }
  }
}
```

**Production:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "QRDine": "Information"
    }
  },
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}
```

## Swagger/OpenAPI

| Variable           | Type   | Examples                | Required      | Notes                   |
| ------------------ | ------ | ----------------------- | ------------- | ----------------------- |
| `Swagger__Enabled` | bool   | `true`                  | No (dev only) | Enable Swagger UI       |
| `Swagger__Title`   | string | `QRDine Restaurant API` | No            | API documentation title |
| `Swagger__Version` | string | `v1`                    | No            | API version             |

**Development:**

```json
{
  "Swagger": {
    "Enabled": true,
    "Title": "QRDine API",
    "Version": "v1"
  }
}
```

**Production:**

```json
{
  "Swagger": {
    "Enabled": false
  }
}
```

## Setting Environment Variables

### appsettings.{Environment}.json

```bash
# Use environment-specific files (committed to repo)
# Development: appsettings.Development.json (in .gitignore)
# Production: appsettings.Production.json (empty, use env vars only)
```

### Command Line

```bash
dotnet run --configuration Release Jwt:Secret="your-secret"
```

### Environment Variables (Recommended for Production)

```bash
# Windows
set ConnectionStrings__DefaultConnection="Server=...;Database=...;"
set Jwt__Secret="your-secret"
dotnet run

# Linux/Mac
export ConnectionStrings__DefaultConnection="Server=...;Database=...;"
export Jwt__Secret="your-secret"
dotnet run
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Server=db;..."
ENV Jwt__Secret="production-secret"

COPY . .
ENTRYPOINT ["dotnet", "QRDine.API.dll"]
```

## Configuration Hierarchy

Priority order (highest to lowest):

1. **Command-line arguments** — `--property value`
2. **Environment variables** — `VAR_NAME=value`
3. **appsettings.{Environment}.json** — Environment-specific overrides
4. **appsettings.json** — Default values

**Example:**

```
appsettings.json (defaults)
  ↓
appsettings.Development.json (dev overrides)
  ↓
Environment variables (prod overrides)
  ↓
Command-line args (immediate overrides)
```

## Related Documentation

- [Configuration Overview](overview.md) — Hierarchy and concepts
- [Secrets Management](secrets-management.md) — Key vault setup
- [Connection Strings](connection-strings.md) — Database connection examples
