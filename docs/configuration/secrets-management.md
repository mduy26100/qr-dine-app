# Secrets Management

JWT secret generation, API key management, and secure credential storage strategies.

## Secrets Overview

QRDine uses multiple secrets across different purposes:

| Secret                 | Use                        | Length                | Rotation       |
| ---------------------- | -------------------------- | --------------------- | -------------- |
| **JWT Secret**         | Sign access/refresh tokens | 64+ chars (256+ bits) | Yearly minimum |
| **API Keys**           | Cloudinary, SendGrid, etc. | Provider-specific     | Quarterly      |
| **Database Password**  | SQL Server authentication  | 12+ chars, complex    | Every 6 months |
| **Connection Strings** | Database access            | Full connection URL   | With password  |
| **Encryption Key**     | At-rest data encryption    | 256-bit               | Yearly minimum |

## JWT Secret Generation

Access tokens are signed with a secret key:

```csharp
var token = new JwtSecurityToken(
    signingCredentials: new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
        SecurityAlgorithms.HmacSha256)
);
```

### Generate Secure Secret

**Using .NET Secret Manager (Development):**

```bash
# Initialize if not done
dotnet user-secrets init --project src/QRDine.API

# Generate and store random 64-char secret
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -hex 32)"

# Verify it's set
dotnet user-secrets list
```

**Using PowerShell (Windows):**

```powershell
# Generate random bytes and convert to base64
$bytes = [System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
$secret = [Convert]::ToBase64String($bytes)
Write-Host "Your JWT Secret: $secret"

# Store in user-secrets
dotnet user-secrets set "Jwt:Secret" $secret
```

**Using Python (Cross-platform):**

```bash
python3 -c "import secrets; print(secrets.token_urlsafe(48))"
```

### Secret Requirements

✅ **Valid:**

- Minimum 64 characters (256 bits for HMAC-SHA256)
- Random, cryptographically secure
- No patterns or dictionary words
- URL-safe or base64-encoded

❌ **Invalid:**

- `MySecretPassword123` (too short, predictable)
- `secret` (way too short)
- Sequential characters (1234567...)
- User-generated passwords (not random enough)

### Sample Valid Secrets

```
# Generated via openssl
/r1+2X8vN9kL0pQ3sT4uV5wX6yZ7aB8cD9eF0gH1iJ2kL3mN4oP5qR6sT7uV8wX9y

# Generated via .NET
8Ej9Kl0Mn1Pq2Rs3Tu4Vv5Ww6Xx7Yy8Zz9Aa0Bb1Cc2Dd3Ee4Ff5Gg6Hh7Ii8Jj

# Generated via Python
iZnQXdN7pKmL5RjVsWyTuVxYaBcDeFgHiJkLmNoPqRstuVwxyZaBcDeFgHiJk
```

## API Key Management

Third-party API keys (Cloudinary, SendGrid, etc.) are credentials that should never be committed.

### Storing API Keys

**Never in Source Code:**

```csharp
// ❌ NEVER DO THIS
public class AppSettings
{
    public string CloudinaryApiSecret { get; set; } = "cldnry_secret_abc123xyz"; // EXPOSED!
}
```

**Always via Configuration:**

```csharp
// ✅ Correct
public class AppSettings
{
    public CloudinarySettings Cloudinary { get; set; }
}

public class CloudinarySettings
{
    public string ApiSecret { get; set; } // Loaded from secure source
}

// In appsettings.json (with placeholders)
{
  "Cloudinary": {
    "ApiSecret": "${CLOUDINARY_API_SECRET}"
  }
}
```

### Rotating API Keys

When a key is compromised:

1. **Immediately generate new key** in provider's dashboard
2. **Update secret storage** (Key Vault, env var)
3. **Test with new key** before removing old
4. **Revoke old key** in provider's dashboard

**Process (SendGrid example):**

```bash
# 1. Generate new key in SendGrid dashboard → copy key

# 2. Update Key Vault
az keyvault secret set \
  --vault-name qrdine-keyvault \
  --name "SendGrid-ApiKey" \
  --value "SG.new_key_here"

# 3. Deploy update (app uses new key automatically)
# 4. Verify sends working
# 5. Delete old key in SendGrid dashboard
```

## Production Secrets Storage

### ❌ Avoid in Production

```json
// appsettings.Production.json (if exists)
{
  "Jwt": {
    "Secret": "production-secret-here"  // ❌ In code/config
  }
}

// .env file
JWT_SECRET=production-secret  // ❌ In file system
```

### ✅ Use in Production

**Azure Key Vault (Recommended):**

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());

// appsettings.Production.json (empty or with placeholders)
{
  "KeyVault": {
    "Enabled": true,
    "Name": "qrdine-keyvault"
  }
}
```

**Environment Variables (Simple):**

```bash
#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Server=db.com;Database=QRDine;User Id=admin;Password=$DB_PASSWORD;"
export Jwt__Secret="$JWT_SECRET"

dotnet QRDine.API.dll
```

**Docker Secrets (Swarm/K8s):**

```yaml
version: "3.8"
services:
  api:
    image: qrdine:latest
    environment:
      Jwt__Secret_FILE: /run/secrets/jwt_secret
      ConnectionStrings__DefaultConnection_FILE: /run/secrets/db_connection
    secrets:
      - jwt_secret
      - db_connection

secrets:
  jwt_secret:
    file: ./secrets/jwt_secret.txt
  db_connection:
    file: ./secrets/db_connection.txt
```

## Azure Key Vault Integration

Centralized, audited secret storage:

### Setup

```bash
# Create Key Vault resource
az keyvault create \
  --name qrdine-keyvault \
  --resource-group qrdine-rg \
  --location eastus

# Grant app access (Managed Identity)
az keyvault set-policy \
  --name qrdine-keyvault \
  --object-id $(az webapp identity show -g qrdine-rg -n qrdine-api --query principalId -o tsv) \
  --secret-permissions get list
```

### Store Secrets

```bash
# Add JWT secret
az keyvault secret set \
  --vault-name qrdine-keyvault \
  --name "Jwt-Secret" \
  --value "your-random-secret-here"

# Add Cloudinary API secret
az keyvault secret set \
  --vault-name qrdine-keyvault \
  --name "Cloudinary-ApiSecret" \
  --value "cldnry_secret_123"

# List all secrets
az keyvault secret list --vault-name qrdine-keyvault
```

### Application Code

```csharp
// Program.cs
var keyVaultUrl = new Uri($"https://qrdine-keyvault.vault.azure.net/");
var credential = new DefaultAzureCredential();

builder.Configuration.AddAzureKeyVault(
    keyVaultUrl,
    credential);

// Now access like normal config
var jwtSecret = configuration["Jwt:Secret"];  // From Key Vault
```

### Key Vault Naming Convention

Customize `--` separator in Key Vault:

```bash
# When storing hierarchical config in Key Vault:
Jwt--Secret         # Maps to config["Jwt:Secret"] in .NET
Cloudinary--ApiKey  # Maps to config["Cloudinary:ApiKey"]
Database--Password  # Maps to config["Database:Password"]
```

## Encryption at Rest

Sensitive configuration values encrypted in database:

```csharp
public class EncryptionService
{
    private readonly string _encryptionKey = _configuration["Encryption:Key"];

    public string Encrypt(string plaintext)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plaintext);
                    }
                }

                var encrypted = ms.ToArray();
                var result = new byte[aes.IV.Length + encrypted.Length];
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

                return Convert.ToBase64String(result);
            }
        }
    }
}
```

## Local Development Secrets

### Using .NET User Secrets

```bash
# Initialize secrets
dotnet user-secrets init --project src/QRDine.API

# Store development secrets
dotnet user-secrets set "Jwt:Secret" "dev-secret-key-here"
dotnet user-secrets set "Cloudinary:ApiSecret" "dev-cloudinary-secret"
dotnet user-secrets set "Email:SmtpPassword" "dev-email-password"

# View all secrets
dotnet user-secrets list

# Clear all secrets
dotnet user-secrets clear
```

**Storage Location:**

- **Windows:** `%APPDATA%\Microsoft\UserSecrets\<user-id>\secrets.json`
- **Linux/Mac:** `~/.microsoft/usersecrets/<user-id>/secrets.json`

### .gitignore for Secrets

```gitignore
# Development secrets (never commit)
appsettings.Development.json
appsettings.*.json
.env
.env.local
.env.*.local
*.pfx
*.pem
secrets/
.secrets/
```

## Backup & Rotation Schedule

### Quarterly - API Keys

```bash
#!/bin/bash
# Rotate Cloudinary API key
echo "Time to rotate Cloudinary API key"
# 1. Generate new key in Cloudinary dashboard
# 2. Update Key Vault
# 3. Test with new key
# 4. Revoke old key
```

### Annually - JWT Secret & Encryption Keys

```bash
#!/bin/bash
# Generate new JWT secret
NEW_JWT_SECRET=$(openssl rand -hex 32)

# Update in Key Vault
az keyvault secret set \
  --vault-name qrdine-keyvault \
  --name "Jwt-Secret" \
  --value "$NEW_JWT_SECRET"

# All new tokens signed with new key
# Old tokens still valid until expiry (10 mins for access, 7 days for refresh)
```

### Semi-Annually - Database Passwords

```bash
# Reset SQL Server password via Azure Portal
# 1. Go to SQL Database resource
# 2. Reset admin password
# 3. Update connection string in Key Vault
# 4. Update appsettings files
# 5. Test connection
# 6. Alert team of new password
```

## Audit Logging for Secrets Access

Monitor who accesses production secrets:

```csharp
_logger.LogWarning(
    "Secrets accessed by user {UserId} at {Time}",
    _currentUserService.UserId,
    DateTime.UtcNow);
```

**Key Vault Audit:**

```bash
# View Key Vault access logs
az monitor log-analytics query \
  --workspace id \
  -q "AzureDiagnostics | where ResourceType == 'VAULTS'"
```

## Related Documentation

- [Configuration Overview](overview.md) — Configuration hierarchy
- [Environment Variables](environment-variables.md) — All env var reference
- [Connection Strings](connection-strings.md) — Database connection setup
- [Security - Data Protection](../security/data-protection.md) — Encryption strategies
