# Connection Strings

Database connection string examples for different environments and scenarios.

## Connection String Format

**SQL Server Standard Format:**

```
Server=<hostname>;Database=<database_name>;User Id=<username>;Password=<password>;
```

**Parameters:**

| Parameter                | Value               | Required        | Example                                             |
| ------------------------ | ------------------- | --------------- | --------------------------------------------------- |
| `Server`                 | Hostname:port or IP | Yes             | `localhost`, `db.example.com:1433`, `192.168.1.100` |
| `Database`               | Database name       | Yes             | `QRDine`, `QRDine_Staging`                          |
| `User Id`                | SQL login           | Yes (auth mode) | `admin`, `sa`                                       |
| `Password`               | SQL password        | Yes (auth mode) | `SecurePassword123!`                                |
| `Integrated Security`    | true/false          | No              | `true` (Windows auth)                               |
| `TrustServerCertificate` | true/false          | No              | `false` (production)                                |
| `Encrypt`                | true/false          | No              | `true` (production)                                 |
| `Connection Timeout`     | Seconds             | No              | `30` (default)                                      |
| `Pooling`                | true/false          | No              | `true` (default)                                    |
| `Min Pool Size`          | Count               | No              | `5` (default)                                       |
| `Max Pool Size`          | Count               | No              | `100` (default)                                     |

## Scenario 1: Local Development (Windows)

**Integrated Windows Authentication** (no password needed):

```
Server=.;Database=QRDine_Dev;Integrated Security=true;TrustServerCertificate=true;
```

**Explanation:**

- `Server=.` — Local machine (SQL Server default instance)
- `Integrated Security=true` — Use Windows credentials (no password)
- `TrustServerCertificate=true` — Accept self-signed certificates (dev only)

**Configuration:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QRDine_Dev;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

**Command Line:**

```bash
dotnet run ConnectionStrings__DefaultConnection="Server=.;Database=QRDine_Dev;Integrated Security=true;"
```

## Scenario 2: Local Development (SQL Server Password Auth)

For non-Windows systems or when SQL Server login is preferred:

```
Server=localhost;Database=QRDine_Dev;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
```

**Explanation:**

- `Server=localhost` — Local machine with explicit hostname
- `User Id=sa` — SQL Server system admin account
- `Password=YourPassword123!` — SQL login password
- `TrustServerCertificate=true` — Dev certificate (development only)

**Setup:**

```xml
<!-- docker-compose.yml for local SQL Server -->
version: '3.8'
services:
  mssql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - mssql_data:/var/opt/mssql
volumes:
  mssql_data:
```

**Start SQL Server:**

```bash
docker-compose up -d

# Wait for startup (~30 seconds)
dotnet run
```

## Scenario 3: Staging Environment

**Azure SQL Database** with enforced encryption:

```
Server=tcp:qrdine-staging-db.database.windows.net,1433;Database=QRDine_Staging;User Id=admin@qrdine-staging-db;Password=StrongPassword123!@#;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

**Explanation:**

- `Server=tcp:...database.windows.net,1433` — Azure SQL endpoint with TCP protocol
- `User Id=admin@qrdine-staging-db` — Azure AD integrated username
- `Encrypt=true` — Enforce TLS encryption (required for Azure)
- `TrustServerCertificate=false` — Validate certificate (staging best practice)
- `Connection Timeout=30` — Full 30-second timeout for cloud latency

**Configuration (Staging):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:qrdine-staging-db.database.windows.net,1433;Database=QRDine_Staging;User Id=admin@qrdine-staging-db;Password=${DATABASE_PASSWORD};Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"
  }
}
```

**Environment Variable:**

```bash
export ConnectionStrings__DefaultConnection="Server=tcp:qrdine-staging-db.database.windows.net,1433;Database=QRDine_Staging;User Id=admin@qrdine-staging-db;Password=StrongPassword123!@#;Encrypt=true;TrustServerCertificate=false;"
```

## Scenario 4: Production Environment

**Azure SQL Database with Managed Identity** (no password in connection string):

```
Server=tcp:qrdine-prod-db.database.windows.net,1433;Database=QRDine_Production;Authentication=Active Directory Managed Identity;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

**Explanation:**

- `Authentication=Active Directory Managed Identity` — AAD authentication (no password)
- `Encrypt=true` — Mandatory TLS encryption
- `TrustServerCertificate=false` — Verify certificate
- Connection timeout: 30 seconds for cloud reliability

**Configuration (Production):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:qrdine-prod-db.database.windows.net,1433;Database=QRDine_Production;Authentication=Active Directory Managed Identity;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"
  }
}
```

**Azure Setup:**

```bash
# Enable Managed Identity on App Service
az webapp identity assign --resource-group qrdine-rg --name qrdine-api

# Grant database access to Managed Identity
az sql server ad-admin create \
  --resource-group qrdine-rg \
  --server-name qrdine-prod-db \
  --display-name "qrdine-api-identity" \
  --object-id $(az webapp identity show -g qrdine-rg -n qrdine-api --query principalId -o tsv)
```

## Scenario 5: Production with Azure Key Vault Secret

**Reference connection string from Key Vault:**

```
Server=tcp:qrdine-prod-db.database.windows.net,1433;Database=QRDine_Production;User Id=admin@qrdine-prod-db;Password=@Microsoft.KeyVault(SecretUri=https://qrdine-keyvault.vault.azure.net/secrets/DatabasePassword/);Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

**Explanation:**

- `Password=@Microsoft.KeyVault(SecretUri=...)` — Reference to Key Vault secret
- Password retrieved at runtime from secure vault
- No password exposed in config files or environment variables

**Key Vault Setup:**

```bash
# Store database password in Key Vault
az keyvault secret set \
  --vault-name qrdine-keyvault \
  --name "DatabasePassword" \
  --value "YourSecurePassword123!@#"

# Grant app access to read secret
az keyvault set-policy \
  --vault-name qrdine-keyvault \
  --object-id $(az webapp identity show -g qrdine-rg -n qrdine-api --query principalId -o tsv) \
  --secret-permissions get
```

## Scenario 6: Fail-Over / High Availability

**SQL Server Always-On with multiple replicas:**

```
Server=qrdine-listener.internal,1433;Database=QRDine;User Id=admin;Password=Password123!;Failover Partner=qrdine-replica2.internal;MultiSubnetFailover=true;Encrypt=true;Connection Timeout=30;
```

**Explanation:**

- `Server=qrdine-listener.internal` — Primary listener (handles failover automatically)
- `Failover Partner=qrdine-replica2.internal` — Secondary replica
- `MultiSubnetFailover=true` — Enable multi-subnet failover scenarios
- Automatic failover: If primary goes down, connection redirects to replica

## Environment Variable Convention

**Variable Naming:**

```bash
# Hierarchical config (colons) become double underscores in environment variables
ConnectionStrings:DefaultConnection  → ConnectionStrings__DefaultConnection
Database:Port                        → Database__Port
Email:Smtp:Server                    → Email__Smtp__Server
```

**Example - PowerShell:**

```powershell
# Linux/Mac style (for CI/CD pipeline)
$env:ConnectionStrings__DefaultConnection = "Server=tcp:...;Database=QRDine;..."

# Windows Registry (persistent)
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "...", "User")
```

## Connection String Best Practices

### ✅ Do This

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server={server};Database=QRDine;User Id={user};Password={password};..."
  }
}
```

Use placeholders, load from secure source:

```bash
export ConnectionStrings__DefaultConnection="Server=prod-db;Database=QRDine;User Id=$DB_USER;Password=$DB_PASSWORD;"
```

### ❌ Never Do This

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db;Database=QRDine;User Id=admin;Password=ActualPassword123;"
  }
}
```

Hardcoded credentials in source code!

## Testing Connection

**Verify connection before deployment:**

```bash
# Using sqlcmd (SQL Server tools)
sqlcmd -S localhost -U sa -P YourPassword123! -Q "SELECT @@VERSION"

# Using .NET
dotnet run --validate-connection
```

**Custom Validation (C#):**

```csharp
using (var connection = new SqlConnection(connectionString))
{
    try
    {
        connection.Open();
        Console.WriteLine("✓ Connection successful");
        connection.Close();
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"✗ Connection failed: {ex.Message}");
    }
}
```

## Related Documentation

- [Configuration Overview](overview.md) — Configuration hierarchy
- [Environment Variables](environment-variables.md) — All env var reference
- [Secrets Management](secrets-management.md) — Secure credential storage
