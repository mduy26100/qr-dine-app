# Build and Deployment

Complete guide to building, testing, and deploying QRDine to production environments.

## Build Process

### Local Development Build

```bash
dotnet build src/QRDine.sln
```

**Output:**

- Compiled binaries in `bin/Debug/net8.0/`
- No optimizations
- Full debug information

### Release Build

```bash
dotnet build src/QRDine.sln -c Release
```

**Output:**

- Optimized binaries in `bin/Release/net8.0/`
- Dead code elimination
- Performance optimizations
- Smaller file size

### Publishing for Deployment

```bash
# Self-contained (includes .NET runtime)
dotnet publish src/QRDine.API/QRDine.API.csproj -c Release -o ./publish --self-contained

# Framework-dependent (requires .NET 8 installed on target)
dotnet publish src/QRDine.API/QRDine.API.csproj -c Release -o ./publish
```

**Artifacts:**

- Compiled DLLs
- Configuration files
- Static assets

## Pre-Deployment Checklist

### Code Quality

- [ ] All tests pass: `dotnet test src/QRDine.sln`
- [ ] No compilation warnings: `dotnet build /warnaserror`
- [ ] Code analysis passes: Run StyleCop, FxCop
- [ ] Dependency audit: `dotnet list package --vulnerable`

### Security

- [ ] JWT secret changed from default
- [ ] Database password strong
- [ ] Cloudinary API secret rotated
- [ ] Email credentials configured
- [ ] CORS origins whitelisted
- [ ] HTTPS enforced

### Performance

- [ ] Database indexes optimized
- [ ] N+1 queries eliminated (via specifications)
- [ ] Caching configured (Redis, in-memory)
- [ ] Connection pooling enabled
- [ ] Migrations optimized

### Documentation

- [ ] API endpoints documented
- [ ] Breaking changes noted
- [ ] Deployment instructions verified
- [ ] Rollback procedure documented

## Deployment Environments

### Development

**Machine:** Local developer workstation

**Configuration:**

- `appsettings.Development.json`
- LocalDB or local SQL Server Express
- localhost:7288

**Secrets:** User secrets or local appsettings (gitignored)

### Staging

**Infrastructure:** Staging server or Azure App Service

**Configuration:** `appsettings.Staging.json`

**Purpose:** Pre-production testing, performance validation

**Database:** Separate staging database with production-like schema

**Secrets:** Environment variables or Azure Key Vault

### Production

**Infrastructure:** Azure App Service, Windows Server, or Docker

**Configuration:** `appsettings.Production.json` + environment variables

**Purpose:** Live customer environment

**Database:** Production SQL Server with backups, HA

**Secrets:** Azure Key Vault or secure environment variables

## Azure App Service Deployment

### Prerequisites

- Azure subscription
- Azure App Service plan (Windows)
- SQL Server (Azure SQL Database, Azure SQL Managed Instance, or VM)
- Redis (Azure Cache for Redis, optional)
- Cloudinary account

### Create Resources

**1. Resource Group:**

```bash
az group create --name qrdine-prod --location eastus
```

**2. App Service Plan:**

```bash
az appservice plan create --name qrdine-plan --resource-group qrdine-prod --sku B2
```

**3. Web App:**

```bash
az webapp create --name qrdine-api --resource-group qrdine-prod --plan qrdine-plan --runtime "DOTNET|8.0"
```

**4. SQL Server & Database:**

```bash
az sql server create --name qrdine-sql --resource-group qrdine-prod --location eastus --admin-user adminuser
az sql db create --name QRDine_Prod --server qrdine-sql --resource-group qrdine-prod
```

**5. Azure Key Vault:**

```bash
az keyvault create --name qrdine-vault --resource-group qrdine-prod --location eastus
```

### Configure Secrets in Key Vault

```bash
az keyvault secret set --vault-name qrdine-vault --name "Jwt--Secret" --value "<your-secret>"
az keyvault secret set --vault-name qrdine-vault --name "Cloudinary--ApiSecret" --value "<api-secret>"
az keyvault secret set --vault-name qrdine-vault --name "ConnectionStrings--DefaultConnection" --value "<connection-string>"
```

### Configure App Settings

```bash
az webapp config appsettings set --name qrdine-api --resource-group qrdine-prod \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    Jwt__ValidIssuer=https://qrdine.com \
    Jwt__ValidAudience=https://qrdine.com \
    FrontendSettings__BaseUrl=https://qrdine.com \
    Cors__AllowedOrigins__0=https://qrdine.com \
    Cors__AllowedOrigins__1=https://admin.qrdine.com
```

### Connect to Key Vault

Update `Program.cs`:

```csharp
if (!app.Environment.IsDevelopment())
{
    var keyVaultUrl = Environment.GetEnvironmentVariable("KEYVAULT_URL");
    var credential = new DefaultAzureCredential();
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        credential);
}
```

### Deploy Application

**Via Git integration (recommended):**

```bash
az webapp deployment source config-zip --resource-group qrdine-prod --name qrdine-api --src Deploy.zip
```

**Via CI/CD (GitHub Actions, Azure Pipelines):**

See below for automated deployment setup.

### Verify Deployment

```bash
# Check if app is running
az webapp show --name qrdine-api --resource-group qrdine-prod --query "state"

# View application logs
az webapp log tail --name qrdine-api --resource-group qrdine-prod
```

Browse to `https://qrdine-api.azurewebsites.net/swagger` to verify.

## Docker Deployment

### Create Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /app
COPY . .
RUN dotnet build src/QRDine.sln -c Release
RUN dotnet publish src/QRDine.API/QRDine.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=builder /app/publish .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "QRDine.API.dll"]
```

### Build Docker Image

```bash
docker build -t qrdine-api:latest .
```

### Run Container Locally

```bash
docker run -d \
  -p 8080:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=db;Initial Catalog=QRDine;..." \
  qrdine-api:latest
```

### Push to Registry

```bash
# Tag image
docker tag qrdine-api:latest myregistry.azurecr.io/qrdine-api:latest

# Login to Azure Container Registry
az acr login --name myregistry

# Push image
docker push myregistry.azurecr.io/qrdine-api:latest
```

### Deploy to Azure Container Instances

```bash
az container create \
  --resource-group qrdine-prod \
  --name qrdine-api-container \
  --image myregistry.azurecr.io/qrdine-api:latest \
  --ports 80 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="..." \
  --registry-login-server myregistry.azurecr.io \
  --registry-username <username> \
  --registry-password <password>
```

## Database Migrations in Production

### Apply Migrations on Deployment

```bash
# Option 1: Via EF Core CLI
dotnet ef database update --project src/QRDine.Infrastructure --startup-project src/QRDine.API

# Option 2: Via code (in Program.cs)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
```

**Recommendation:** Use option 2 (code-based) for automatic migrations on startup.

### Rollback Plan

If migration fails:

1. **Revert to previous release** (maintain separate database backups per version)
2. **Restore database from backup** (pre-migration snapshot)
3. **Deploy previous code version** with working migrations

## CI/CD Pipeline (GitHub Actions)

### Create Workflow

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  test-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release

      - name: Run tests
        run: dotnet test --no-build --verbosity normal

      - name: Publish
        run: dotnet publish src/QRDine.API/QRDine.API.csproj -c Release -o ./publish

      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: qrdine-api
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish
```

### Configure Secrets

Add to GitHub repository:

1. Go to Settings → Secrets and variables → Actions
2. Add `AZURE_WEBAPP_PUBLISH_PROFILE` (from Azure Portal)

### Trigger Deployment

- Merges to `main` automatically deploy to production
- Pull requests run tests but don't deploy

## Monitoring & Logging

### Application Insights (Azure)

```csharp
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
```

**Monitors:**

- Request rates
- Error rates
- Response times
- Dependency issues
- Custom events

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
}
```

Send logs to both console and Application Insights.

### Health Checks

Implement health check endpoint:

```csharp
app.MapHealthChecks("/health");

services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRedis(Configuration["Redis:ConnectionString"]);
```

Monitor at `https://qrdine-api.azurewebsites.net/health`

## Performance Optimization

### Connection Pooling

EF Core connection pooling (automatic):

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptions =>
        sqlServerOptions.CommandTimeout(60)));
```

### Caching Strategy

```json
"Redis": {
    "ConnectionString": "your-redis.redis.cache.windows.net:6379,password=***,ssl=True"
}
```

Cache frequently accessed:

- Product listings (5 min TTL)
- Pricing plans (1 hour TTL)
- Feature limits (1 hour TTL)

### Scaling

**Horizontal Scaling (multiple instances):**

```bash
az appservice plan update --name qrdine-plan --resource-group qrdine-prod --sku S1
```

Load balances across instances automatically.

**Vertical Scaling (larger instance):**

```bash
az appservice plan update --name qrdine-plan --resource-group qrdine-prod --sku P1V2
```

## Backup & Disaster Recovery

### Database Backups

**Automated (Azure SQL Database):**

- Daily backups retained 7-35 days
- Configurable in Azure Portal

**Manual backup:**

```bash
az sql db create-backup --server qrdine-sql --database QRDine_Prod --backup-name manual-20260224
```

### Restore from Backup

```bash
az sql db restore --destination-server qrdine-sql-restore --destination-name QRDine_Restore \
  --resource-group qrdine-prod --name QRDine_Prod --backup-id <backup-id>
```

### Geo-Replication (High Availability)

```bash
az sql db replica create --name QRDine_Prod_Replica \
  --server qrdine-sql-secondary --resource-group qrdine-prod \
  --partner-server qrdine-sql --partner-resource-group qrdine-prod
```

Enables automatic failover in case of datacenter outage.

## Production Checklist

- [ ] HTTPS configured with valid SSL certificate
- [ ] All secrets stored in Key Vault
- [ ] Database backups scheduled
- [ ] Logging and monitoring enabled
- [ ] Health checks operational
- [ ] Rate limiting configured
- [ ] CORS origins whitelisted
- [ ] Security headers configured
- [ ] Load balancing tested
- [ ] Disaster recovery plan documented
- [ ] Incident response contacts defined

## Related Documentation

- [Security Overview](../security/overview.md)
- [Configuration Guide](../configuration/environment.md)
- [Database Schema](../database/schema.md)
