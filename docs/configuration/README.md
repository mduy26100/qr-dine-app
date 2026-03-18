# Configuration

Environment variables, secrets management, and configuration hierarchy.

## Contents

- **[Configuration Overview](environment.md)** — High-level architecture:
  - 4-level configuration hierarchy
  - File structure (appsettings.json, environment-specific files)
  - Configuration loading process
  - Environment variable naming convention
  - Configuration validation on startup
  - Accessing configuration in code
  - Best practices

- **[Environment Variables Reference](environment-variables.md)** — Complete variable table:
  - Core API configuration
  - Database configuration
  - JWT setup (token lifetime, issuer, audience)
  - Cloudinary API credentials
  - Email provider configuration (SMTP, Gmail, Office365, SendGrid)
  - Redis cache setup
  - Rate limiting policies
  - CORS whitelist
  - Logging configuration
  - Swagger documentation settings

- **[Secrets Management](secrets-management.md)** — Key generation and secure storage:
  - JWT secret generation (256-bit random)
  - API key rotation (quarterly schedule)
  - Production secrets storage (Azure Key Vault, env vars, Docker secrets)
  - Key Vault integration and setup
  - Encryption at rest for sensitive config
  - Local development secrets (user-secrets)
  - Backup and rotation schedule
  - Audit logging for secrets access

- **[Connection Strings](connection-strings.md)** — Database connection examples:
  - Connection string format and parameters
  - Scenario 1: Local dev with Windows auth
  - Scenario 2: Local dev with SQL password
  - Scenario 3: Staging (Azure SQL Database)
  - Scenario 4: Production (Managed Identity)
  - Scenario 5: Production (Key Vault reference)
  - Scenario 6: High Availability (Always-On)
  - Testing connections
  - Best practices

## Quick Navigation

| Need                                    | Resource                                                                                            |
| --------------------------------------- | --------------------------------------------------------------------------------------------------- |
| How does configuration load?            | [Overview - Configuration Hierarchy](environment.md#configuration-hierarchy)                        |
| What are all the environment variables? | [Environment Variables Reference](environment-variables.md)                                         |
| How to generate JWT secret?             | [Secrets - JWT Secret Generation](secrets-management.md#generate-secure-secret)                     |
| How to set up Cloudinary?               | [Environment Variables - Cloudinary](environment-variables.md#cloudinary-configuration-when-to-use) |
| How to configure email?                 | [Environment Variables - Email](environment-variables.md#email-configuration)                       |
| Which connection string to use?         | [Connection Strings - Scenarios](connection-strings.md)                                             |
| How to use Azure Key Vault?             | [Secrets - Key Vault Integration](secrets-management.md#azure-key-vault-integration)                |
| How often to rotate secrets?            | [Secrets - Backup & Rotation Schedule](secrets-management.md#backup--rotation-schedule)             |

## Configuration Hierarchy

**4-Level Priority** (later overrides earlier):

```
1. appsettings.json
   ↓ (overridden by)
2. appsettings.{ASPNETCORE_ENVIRONMENT}.json
   ↓ (overridden by)
3. Environment Variables
   ↓ (overridden by)
4. User Secrets (dev only)
```

**Example:**

```json
// appsettings.json
{ "Jwt": { "AccessTokenExpiryMinutes": 10 } }

// appsettings.Production.json
{ "Jwt": { "AccessTokenExpiryMinutes": 15 } }

// Environment variable
Jwt__AccessTokenExpiryMinutes = 20

// Result: Uses 20 (highest priority)
```

## Configuration Files Location

```
src/QRDine.API/
├── appsettings.json                    # Shared defaults (committed)
├── appsettings.Development.json        # Dev overrides (gitignored)
├── appsettings.Production.json         # Prod template (empty or minimal)
├── appsettings.Staging.json            # Staging template (optional)
└── QRDine.API.csproj
```

## Key Concepts

**appsettings.json** — Committed to repo, no secrets

```json
{
  "Logging": { "LogLevel": { "Default": "Information" } },
  "Jwt": { "AccessTokenExpiryMinutes": 10 }
}
```

**appsettings.Development.json** — Gitignored, dev secrets only

```json
{
  "ConnectionStrings": { "DefaultConnection": "..." },
  "Jwt": { "Secret": "dev-secret-here" }
}
```

**Environment Variables** — Production secrets

```bash
Jwt__Secret = production-secret-key
ConnectionStrings__DefaultConnection = "Server=prod-db;..."
```

**User Secrets** — Local dev machine only

```bash
dotnet user-secrets set "Jwt:Secret" "local-dev-secret"
```

## Common Configuration Tasks

**Set environment for development:**

```bash
# Windows
set ASPNETCORE_ENVIRONMENT=Development

# Linux/Mac
export ASPNETCORE_ENVIRONMENT=Development
```

**Generate JWT secret:**

```bash
# Store in user-secrets
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 32)"
```

**Store production secret in Key Vault:**

```bash
az keyvault secret set --vault-name qrdine-keyvault --name "Jwt-Secret" --value "..."
```

**View all configuration:**

```csharp
// In controller (debug only)
IConfiguration config;  // Injected
Debug.WriteLine(config.GetDebugView());
```

---

**Related:** [Security - Secrets Management](../security/data-protection.md) • [Database - Connection Setup](../database/schema.md) • [Development Guidelines](../development/)
"DefaultConnection": "server=production-server;..."
}
}

````

**3. Override with environment variables (highest priority)**

```bash
export ConnectionStrings__DefaultConnection="server=env-override;..."
````

## Key Configuration Sections

| Section               | Purpose                     | Example                                                  |
| --------------------- | --------------------------- | -------------------------------------------------------- |
| **ConnectionStrings** | Database access             | `Server=localhost;Database=QRDine;...`                   |
| **Jwt**               | Token generation/validation | `Secret`, `Issuer`, `Audience`, `ExpiryMinutes`          |
| **Cloudinary**        | Image upload service        | `CloudName`, `ApiKey`, `ApiSecret`                       |
| **Email**             | SMTP mail service           | `Provider`, `SmtpServer`, `Port`, `Username`, `Password` |
| **Redis**             | Cache configuration         | `ConnectionString`                                       |
| **Cors**              | Cross-origin requests       | `AllowedOrigins`                                         |

See [Environment Configuration](environment.md) for complete variable reference and setup examples.

---

**Reference:** See also [Getting Started](../getting-started.md) for initial setup and [Deployment](../deployment/) for production configuration.
