# Getting Started

Complete guide to setting up and running QRDine locally.

## Prerequisites

Ensure you have the following installed:

- **.NET 8 SDK** [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** (LocalDB, ExpressDB, or full edition) [Download](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **Git** [Download](https://git-scm.com/)
- **Redis** (optional, for advanced caching) [Download](https://redis.io/download)
- **Cloudinary Account** (for image uploads) [Sign up](https://cloudinary.com/)

For development tooling:

- **Visual Studio 2022** (Community, Professional) or **VS Code** with C# extensions
- **SQL Server Management Studio (SSMS)** for database administration

## Installation & Setup

### Step 1: Clone the Repository

```bash
git clone https://github.com/mduy26100/qr-dine-app.git
cd qr-dine-app
```

### Step 2: Verify .NET Installation

```bash
dotnet --version
```

Should display version 8.0 or higher.

### Step 3: Restore Dependencies

```bash
dotnet restore
```

This downloads all NuGet packages referenced in the solution.

### Step 4: Configure Application Settings

QRDine use configuration hierarchy: `appsettings.json` (base) → `appsettings.Development.json` (override).

Edit `src/QRDine.API/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QRDine;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "<256-bit-base64-encoded-secret>",
    "ValidIssuer": "http://localhost:7288",
    "ValidAudience": "http://localhost:7288",
    "AccessTokenExpiryMinutes": 10,
    "RefreshTokenExpiryDays": 7
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "FrontendSettings": {
    "BaseUrl": "http://localhost:5173"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

**Configuration Details:**

| Setting                               | Purpose                                                                                               |
| ------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `ConnectionStrings.DefaultConnection` | SQL Server connection string                                                                          |
| `Jwt.Secret`                          | 256-bit secret for signing JWT tokens (generate with `dotnet user-secrets` or a strong random string) |
| `Jwt.ValidIssuer/ValidAudience`       | JWT token validation claims (set to API base URL)                                                     |
| `Jwt.AccessTokenExpiryMinutes`        | Access token lifetime in minutes                                                                      |
| `Jwt.RefreshTokenExpiryDays`          | Refresh token lifetime in days                                                                        |
| `Cloudinary.*`                        | Image upload service credentials                                                                      |
| `FrontendSettings.BaseUrl`            | CORS-whitelisted frontend URL                                                                         |
| `Redis.ConnectionString`              | Redis server for caching (`localhost:6379` for local Redis)                                           |

### Step 5: Create & Migrate Database

Navigate to the API project and apply migrations:

```bash
cd src/QRDine.API
dotnet ef database update --startup-project . --project ../QRDine.Infrastructure
```

This command:

1. Connects to SQL Server using the connection string
2. Creates the `QRDine` database if it doesn't exist
3. Applies all pending migrations from `src/QRDine.Infrastructure/Persistence/Migrations/`
4. Seeds initial data (roles, SuperAdmin user)

**Verify database creation:**

- Open SQL Server Management Studio
- Connect to your SQL Server instance
- Expand "Databases" — you should see "QRDine"
- Tables are organized in schemas: `identity`, `tenant`, `catalog`, `sales`, `billing`

### Step 6: Run the Application

```bash
cd ../QRDine.API
dotnet run
```

Expected output:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7288
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to stop.
```

Application is running on `https://localhost:7288`.

### Step 7: Access the Application

- **Swagger UI:** https://localhost:7288/swagger
  - Four API documentation groups:
    - Identity API
    - Admin API
    - Management API
    - Storefront API

### Step 8: Authenticate

Default SuperAdmin credentials seeded on first run:

```
Email: admin@qrdine.com
Password: Admin@123!
```

**Login via Swagger:**

1. Navigate to `/api/v1/auth/login` endpoint
2. Execute with request body:
   ```json
   {
     "email": "admin@qrdine.com",
     "password": "Admin@123!"
   }
   ```
3. Copy the `accessToken` from response
4. Click "Authorize" button in Swagger UI
5. Paste token as `Bearer <token>`

Now all authenticated endpoints are accessible.

## Development Workflow

### Building the Solution

```bash
dotnet build src/QRDine.sln
```

### Running Tests

```bash
dotnet test src/QRDine.sln
```

### Adding a New Migration

When modifying domain entities:

```bash
cd src/QRDine.API
dotnet ef migrations add MigrationName --startup-project . --project ../QRDine.Infrastructure
```

Then apply:

```bash
dotnet ef database update
```

### Debugging

In **Visual Studio 2022:**

1. Open `QRDine.sln`
2. Set `QRDine.API` as startup project
3. Press F5 to debug

In **VS Code:**

1. Install C# Dev Kit extension
2. Press F5 to debug (launches based on `.vscode/launch.json`)

### Rapid Iteration

For hot-reload during development:

```bash
dotnet watch run --project src/QRDine.API
```

Code changes automatically trigger recompilation and restart.

## Common Issues & Troubleshooting

### Issue: "Unable to connect to SQL Server"

**Solution:**

- Verify SQL Server is running:
  ```bash
  sqlcmd -S (local) -E
  ```
- Check connection string in `appsettings.Development.json`
- For LocalDB: `Server=(localdb)\mssqllocaldb;...`
- For named instance: `Server=YOUR_INSTANCE;...`

### Issue: "Migration duplicate key error"

**Solution:** Drop and recreate the database:

```bash
dotnet ef database drop --startup-project src/QRDine.API --project src/QRDine.Infrastructure --force
dotnet ef database update --startup-project src/QRDine.API --project src/QRDine.Infrastructure
```

### Issue: "Cloudinary credentials invalid"

**Solution:**

- Verify credentials in `appsettings.Development.json`
- Check Cloudinary account settings at https://cloudinary.com/console
- If not using Cloudinary, you can disable upload features during development

### Issue: "JWT token validation failed"

**Solution:**

- Ensure `Jwt.Secret` matches across login and validation
- Verify token hasn't expired (check `expiresIn` in login response)
- Confirm token format in Swagger is `Bearer <token>` (not just `<token>`)

### Issue: "CORS errors when calling from frontend"

**Solution:**

- Add frontend URL to `Cors.AllowedOrigins` in `appsettings.Development.json`
- Ensure frontend sends credentials with requests if needed
- Check browser developer console for detailed CORS error

## Next Steps

1. **Read Architecture Guide** — Understand system design and layers
2. **Explore Project Structure** — Familiarize yourself with folder organization
3. **Review API Conventions** — Learn request/response formats and authentication
4. **Study a Feature** — Examine how Catalog module is implemented
5. **Write a Feature** — Create a new endpoint following established patterns

## Resources

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Onion Architecture](https://jeffreypalermo.com/2008/07/the-onion-architecture-nut-shell/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
