# Configuration Overview

High-level configuration architecture, hierarchy, and loading process.

## Configuration Hierarchy

QRDine loads configuration in priority order (later sources override earlier):

```
appsettings.json
     ↓ (overridden by)
appsettings.{Environment}.json
     ↓ (overridden by)
Environment Variables
     ↓ (overridden by)
User Secrets (dev only)
```

**Example:**

```json
// appsettings.json (default)
{ "Logging": { "LogLevel": { "Default": "Information" } } }

// appsettings.Production.json (prod override)
{ "Logging": { "LogLevel": { "Default": "Warning" } } }

// Environment variable (final override)
Logging__LogLevel__Default = "Error"
// Result: Application uses "Error" level
```

## Configuration File Structure

### appsettings.json (Always Committed)

Base configuration shared across environments:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["https://qrdine.com", "https://admin.qrdine.com"]
  },
  "Jwt": {
    "AccessTokenExpiryMinutes": 10,
    "RefreshTokenExpiryDays": 7,
    "ValidIssuer": "https://qrdine.com",
    "ValidAudience": "https://qrdine.com"
  },
  "Redis": {
    "ConnectionString": "placeholder"
  }
}
```

**Contains:**

- ✅ Non-sensitive defaults
- ✅ Logging levels
- ✅ Timeouts, feature flags
- ❌ No API keys, passwords, secrets

### appsettings.Development.json (Gitignored)

Local development overrides with development secrets:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QRDine_Dev;Integrated Security=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Secret": "dev-secret-key-here"
  },
  "Cloudinary": {
    "CloudName": "dev-cloud-name",
    "ApiKey": "dev-api-key",
    "ApiSecret": "dev-api-secret"
  },
  "CORS": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
  }
}
```

**Contains:**

- ⚠️ Development-only secrets (never commit)
- ✅ Local database connections
- ✅ Dev API keys (non-production)

### appsettings.Production.json (Empty or Minimal)

Production deployments use environment variables instead:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  }
}
```

**Why Empty?**

- Production secrets loaded via environment variables or Key Vault
- No hardcoded credentials in repository
- Easier to rotate secrets without redeploying

## Environment Variable Naming

Hierarchical configuration mapped to environment variables:

| Config Path                           | Environment Variable                   |
| ------------------------------------- | -------------------------------------- |
| `Jwt:Secret`                          | `Jwt__Secret`                          |
| `Jwt:ValidIssuer`                     | `Jwt__ValidIssuer`                     |
| `Logging:LogLevel:Default`            | `Logging__LogLevel__Default`           |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| `Email:Smtp:Server`                   | `Email__Smtp__Server`                  |

**Convention:** Replace `:` (nesting) with `__` (double underscore)

## Configuration Loading Process

```
1. Load appsettings.json
   ↓
2. Load appsettings.{ASPNETCORE_ENVIRONMENT}.json
   ↓
3. Load environment variables (override both files)
   ↓
4. Load user secrets (dev only, override all)
   ↓
5. Validate critical settings
   ↓
6. Application runs with resolved configuration
```

## Setting Environment on Startup

**Windows (Command Prompt):**

```cmd
set ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

**Windows (PowerShell):**

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run
```

**Linux/Mac:**

```bash
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

**Docker:**

```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
```

## Configuration Sections (By Topic)

Detailed documentation organized by configuration purpose:

### Environment Variables Reference

Complete list of all configurable settings:

- Database connection options
- JWT token lifetime, expiry
- Cloudinary API configuration
- Email provider setup (SMTP, Gmail, Office365, SendGrid)
- Redis cache connection
- Rate limiting policies
- CORS whitelist
- Logging levels
- Swagger documentation toggle

**→ See [Environment Variables Reference](environment-variables.md) for complete table**

### Secrets Management

How to generate, store, and rotate sensitive credentials:

- JWT secret generation (256-bit)
- API key management (Cloudinary, SendGrid, Azure)
- Database password rotation
- Azure Key Vault integration
- Local development secrets (user-secrets)
- Backup and rotation schedule

**→ See [Secrets Management](secrets-management.md) for implementation details**

### Connection String Examples

Database connection strings for different scenarios:

- Local development (Windows auth)
- Local development (SQL password auth)
- Staging (Azure SQL Database)
- Production (Managed Identity)
- Production (Key Vault secret reference)
- High Availability (Always-On)

**→ See [Connection Strings](connection-strings.md) for all scenarios**

## Configuration Validation

Application validates critical settings on startup:

```csharp
services
    .AddOptions<JwtSettings>()
    .Bind(configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

services
    .AddOptions<DatabaseSettings>()
    .Bind(configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

**Effect:**

- Missing critical settings → Application fails to start with clear error
- Invalid settings → Application fails with validation details

## Accessing Configuration in Code

**Via IOptions (Recommended):**

```csharp
public class OrderService
{
    private readonly IOptions<JwtSettings> _jwtSettings;

    public OrderService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public void Process()
    {
        var tokenExpiry = _jwtSettings.Value.AccessTokenExpiryMinutes;
    }
}
```

**Via IConfiguration (Direct):**

```csharp
public string GetConnectionString()
{
    var connString = _configuration.GetConnectionString("DefaultConnection");
    return connString;
}
```

## Best Practices

✅ **Do This:**

- Use strongly-typed options classes (`IOptions<T>`)
- Store secrets in Key Vault or environment variables (never in files)
- Use different secrets per environment
- Rotate sensitive credentials regularly (quarterly minimum)
- Document all configuration settings

❌ **Never Do This:**

- Commit secrets to version control
- Use same JWT secret in dev/staging/production
- Hardcode API keys in application code
- Leave default database passwords unchanged
- Store sensitive data unencrypted

## Related Documentation

- [Environment Variables Reference](environment-variables.md) — All configuration options
- [Secrets Management](secrets-management.md) — JWT, API keys, Key Vault setup
- [Connection Strings](connection-strings.md) — Database connection examples for all scenarios
- [Security - Data Protection](../security/data-protection.md) — Encryption strategies
