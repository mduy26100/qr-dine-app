# Troubleshooting

Solutions to common issues encountered during development and deployment.

## Database Issues

### Error: "Cannot connect to SQL Server"

**Symptoms:**

- `SqlClient.SqlException: A network-related or instance-specific error occurred`
- Connection timeout errors

**Solutions:**

1. **Verify SQL Server is running:**

   ```bash
   sqlcmd -S (local) -E
   ```

   If connection fails, start SQL Server service.

2. **Check connection string syntax:**

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QRDine;Trusted_Connection=True;"
     }
   }
   ```

3. **For named instance:**

   ```
   Server=COMPUTERNAME\INSTANCENAME;Database=QRDine;
   ```

4. **For remote SQL Server:**

   ```
   Server=tcp:db.example.com,1433;Database=QRDine;User Id=sa;Password=****;
   ```

5. **Test connection with SSMS:**
   - Open SQL Server Management Studio
   - Connect to your server
   - Verify database exists

### Error: "Duplicate key value" during migration

**Symptoms:**

- `SqlException: Cannot insert duplicate key row in object`

**Solutions:**

1. **Recreate database (development):**

   ```bash
   dotnet ef database drop -f --startup-project src/QRDine.API --project src/QRDine.Infrastructure
   dotnet ef database update --startup-project src/QRDine.API --project src/QRDine.Infrastructure
   ```

2. **Check unique constraint:**
   - Examine `Persistence/Configurations/` for duplicate indexes
   - Review recent migrations for constraint changes

3. **Clean data (if applicable):**
   ```sql
   DELETE FROM [catalog].[Products];
   DELETE FROM [catalog].[Categories];
   DBCC CHECKIDENT ('[catalog].[Categories]', RESEED, 0);
   ```

### Error: "Invalid column name" after migration

**Symptoms:**

- `SqlException: Invalid column name`
- Migration applied but schema mismatch

**Solutions:**

1. **Revert migration:**

   ```bash
   dotnet ef migrations remove --startup-project src/QRDine.API --project src/QRDine.Infrastructure
   ```

2. **Check migration files:**
   - Verify migration code in `Migrations/` folder
   - Ensure column names match entity properties

3. **Regenerate migration:**
   ```bash
   dotnet ef migrations add MigrationName --startup-project src/QRDine.API --project src/QRDine.Infrastructure
   ```

## Authentication Issues

### Error: "JWT signature validation failed"

**Symptoms:**

- `SecurityTokenSignatureKeyNotFoundException`
- 401 Unauthorized on protected endpoints

**Solutions:**

1. **Verify JWT secret matches:**
   - Check `appsettings.json` → `Jwt.Secret`
   - Must be identical for signing and validation
   - If changed, all existing tokens become invalid

2. **Regenerate secret if lost:**

   ```bash
   dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 32)"
   ```

3. **Check token format in header:**
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
   ```
   Must include `Bearer ` prefix and valid token.

### Error: 401 Unauthorized on valid token

**Symptoms:**

- Login successful, token obtained
- Token works in Swagger but not in direct requests
- 401 returned despite valid JWT

**Solutions:**

1. **Check token expiration:**

   ```json
   {
     "Jwt": {
       "AccessTokenExpiryMinutes": 10
     }
   }
   ```

   Token expires after 10 minutes; use refresh token to get new one.

2. **Check token claims:**
   - Decode token at https://jwt.io
   - Verify `iss`, `aud`, `exp` match configuration

3. **Verify `ICurrentUserService` extracts merchant:**
   ```bash
   curl -H "Authorization: Bearer <token>" https://localhost:7288/api/v1/management/products
   ```

### Error: "Refresh token invalid or expired"

**Symptoms:**

- Can't refresh token even though it should be valid
- `ForbiddenException: Refresh token invalid or expired`

**Solutions:**

1. **Verify refresh token stored in database:**

   ```sql
   SELECT * FROM [identity].[RefreshTokens] WHERE [Token] = '<refresh-token>';
   ```

2. **Check expiry date:**

   ```sql
   SELECT * FROM [identity].[RefreshTokens] WHERE [ExpiryDate] < GETUTCDATE();
   ```

3. **Ensure token not revoked:**
   ```sql
   SELECT * FROM [identity].[RefreshTokens] WHERE [IsRevoked] = 1;
   ```

## API Issues

### Error: "Invalid model state" (400 Bad Request)

**Symptoms:**

- Request returns 400 with validation errors
- Can't create/update resources

**Example response:**

```json
{
  "error": {
    "type": "validation-error",
    "message": "The following validation errors occurred.",
    "errors": [{ "field": "name", "message": "Name must not be empty" }]
  }
}
```

**Solutions:**

1. **Check request body:**
   - Verify JSON syntax
   - Ensure required fields present
   - Match field names exactly (case-sensitive)

2. **Verify field constraints:**
   - Max length: 255 chars for Name
   - Price must be > 0
   - Email must be valid format

3. **Refer to validator:**
   - Check `AbstractValidator<T>` in feature folder
   - Implements all validation rules

### Error: 403 Forbidden on management endpoint

**Symptoms:**

- Can access public endpoints
- Management endpoints return 403
- Even with valid token

**Solutions:**

1. **Check user role:**

   ```csharp
   [Authorize(Roles = "Merchant")]
   ```

   User must have Merchant or SuperAdmin role.

2. **Verify merchant ownership:**

   ```sql
   SELECT [Id], [MerchantId] FROM [tenant].[Merchants];
   ```

   Ensure current user's MerchantId matches resource's MerchantId.

3. **Check JWT claims:**
   - Decode token to verify `merchant_id` claim present
   - Compare with resource's MerchantId

### Error: "Rate limit exceeded" (429)

**Symptoms:**

- `429 Too Many Requests`
- After multiple requests in short time
- Login endpoint blocks after 5 attempts in 15 min

**Solutions:**

1. **Wait before retrying:**
   - Login: 15 minutes
   - Register: 1 hour
   - Implement exponential backoff in client

2. **Clear rate limit (development only):**
   - Restart application (in-memory rate limiter resets)
   - Or change IP address if testing

3. **Configure limits in appsettings:**
   ```json
   {
     "RateLimitPolicies": {
       "Login": "5/15m",
       "Register": "3/1h"
     }
   }
   ```

## Application Runtime Issues

### Error: "Dependency injection failed"

**Symptoms:**

- `InvalidOperationException` on startup
- "Unable to resolve service"

**Example:**

```
Unable to resolve service for type 'IRepository<Category>'
while attempting to activate 'CreateCategoryCommandHandler'
```

**Solutions:**

1. **Check service registration:**

   ```csharp
   // src/QRDine.API/DependencyInjection/ServiceCollectionExtensions.cs
   services.AddFeatures(); // Must include all feature registrations
   ```

2. **Verify interface vs implementation:**

   ```csharp
   // Correct
   services.AddScoped<IRepository<Category>, Repository<Category>>();

   // Wrong
   services.AddScoped(typeof(Repository<>)); // Missing interface
   ```

3. **Check registration order:**
   - Infrastructure must be registered before Application
   - Application before Features
   - See `AddApplicationServices` method

### Error: Applied migration with context type different

**Symptoms:**

- `InvalidOperationException`during database update
- Different EF Core DbContext detected

**Solutions:**

1. **Verify DbContext type hasn't changed:**
   - Should always be `ApplicationDbContext`

2. **Check connection string context:**
   - Ensure targeting same database

3. **Remove conflicting migration:**
   ```bash
   dotnet ef migrations remove --startup-project src/QRDine.API --project src/QRDine.Infrastructure
   ```

## Deployment Issues

### Error: "Application fails to start on Azure App Service"

**Symptoms:**

- 500 Internal Server Error
- Homepage returns "Running on Azure App Service"

**Solutions:**

1. **Check logs in Azure Portal:**
   - App Service → Log stream
   - Or: `az webapp log tail --name qrdine-api --resource-group qrdine-prod`

2. **Verify startup command:**

   ```bash
   ASPNETCORE_URLS=http://+:80 dotnet QRDine.API.dll
   ```

3. **Check environment variables:**
   - Configuration → Application settings
   - Verify all required settings present

4. **Test locally first:**
   ```bash
   dotnet publish -c Release -o ./publish
   cd publish
   dotnet QRDine.API.dll
   ```

### Error: "File not found" in production

**Symptoms:**

- DLL or asset missing in deployed package
- `FileNotFoundException` on specific features

**Solutions:**

1. **Clean and rebuild:**

   ```bash
   dotnet clean src/QRDine.sln
   dotnet build src/QRDine.sln -c Release
   ```

2. **Verify all projects in publish:**

   ```bash
   dotnet publish src/QRDine.API/QRDine.API.csproj -c Release -v detailed
   ```

3. **Check project file references:**
   - Ensure all `.csproj` files have correct `<ProjectReference>` paths

### Error: "Connection timeout" in production

**Symptoms:**

- Requests hang then fail
- Azure SQL connection pooling exhausted

**Solutions:**

1. **Increase connection pool size:**

   ```csharp
   services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(
           connectionString,
           sqlOptions => sqlOptions.CommandTimeout(60)));
   ```

2. **Monitor active connections:**

   ```sql
   SELECT COUNT(*) FROM sys.dm_exec_connections;
   ```

3. **Enable query timeout:**
   - Azure SQL Database → Connection strings
   - Add "Connection Timeout=30"

## Performance Issues

### Slow API responses

**Symptoms:**

- Requests take > 1 second
- High database query count

**Solutions:**

1. **Check for N+1 queries in EF:**
   - Use `.Include()` for related entities
   - Review Specifications for eager loading

2. **Add indexes:**

   ```sql
   CREATE INDEX IX_Category_MerchantId ON [catalog].[Categories]([MerchantId]);
   CREATE INDEX IX_Product_CategoryId ON [catalog].[Products]([CategoryId]);
   ```

3. **Enable query cache:**
   - Configure Redis in `appsettings.json`
   - Use `ICacheService` in queries

4. **Profile with Application Insights:**
   - Azure Portal → Application Insights
   - Check slowest dependencies and queries

### High memory usage

**Symptoms:**

- Request memory keeps growing
- Out of Memory exception after running

**Solutions:**

1. **Dispose resources properly:**

   ```csharp
   using (var scope = app.Services.CreateScope())
   {
       // Scope disposed, resources released
   }
   ```

2. **Avoid loading all data:**

   ```csharp
   // ❌ Bad: Loads all products into memory
   var allProducts = _db.Products.ToList();

   // ✅ Good: Uses pagination
   var spec = new ProductSpecification(merchantId).WithPagination(1, 20);
   var products = await _repository.ListAsync(spec);
   ```

3. **Set CORS properly:**
   - Don't allow `*` (causes header bloat)
   - Whitelist specific origins

## Logging & Debugging

### Enable detailed logging

Development:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

### View EF Core SQL

```csharp
// In Program.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)); // Log queries
```

### Debug with Visual Studio

1. Press F5 to start debugging
2. Set breakpoints with F9
3. View variables in Debug window
4. Continue with F10 (step over) or F11 (step into)

## Getting Help

1. **Check existing documentation:**
   - Review Architecture, Features, API guides
   - Consult Security and Database docs

2. **Review error message:**
   - Stack trace often indicates root cause
   - Exception type suggests category

3. **Reproduce locally:**
   - Test issue in development first
   - Easier to debug locally

4. **Check logs:**
   - Application Insights in production
   - Console output in development
   - Event Viewer on Windows servers

5. **Search similar issues:**
   - GitHub issues for .NET projects
   - Stack Overflow (tagged with dotnet, entity-framework)

6. **Escalate with details:**
   - Full error message and stack trace
   - Steps to reproduce
   - Environment (OS, .NET version, SQL Server version)
   - Recent changes made
