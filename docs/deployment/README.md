# Deployment

Build process, deployment procedures, CI/CD pipelines, and operations.

## Contents

- **[Build & Deployment Guide](build-and-deploy.md)** — Complete deployment documentation including:
  - Build process (Debug vs Release)
  - Pre-deployment checklist (code quality, security, performance)
  - Azure App Service deployment (step-by-step)
  - Docker containerization with Dockerfile
  - Database migrations in production
  - CI/CD with GitHub Actions (workflow example)
  - Monitoring and logging (Application Insights)
  - Performance optimization (connection pooling, caching, scaling)
  - Backup and disaster recovery (automated, manual, geo-replication)
  - Production checklist

- **[Troubleshooting](troubleshooting.md)** — Solutions for common issues:
  - Database connection and migration errors
  - Authentication and JWT token issues
  - API validation and authorization errors
  - Runtime dependency injection failures
  - Deployment and Azure-specific issues
  - Performance and memory problems
  - Logging and debugging tips

## Quick Links

- What to check before deploying? → [Build & Deployment - Pre-Deployment Checklist](build-and-deploy.md#pre-deployment-checklist)
- Deploying to Azure? → [Build & Deployment - Azure App Service](build-and-deploy.md#azure-app-service-deployment)
- Using Docker? → [Build & Deployment - Docker](build-and-deploy.md#docker-deployment)
- CI/CD setup? → [Build & Deployment - GitHub Actions](build-and-deploy.md#github-actions)
- Database migrations? → [Build & Deployment - Migrations](build-and-deploy.md#database-migrations)
- Backup strategy? → [Build & Deployment - Backup & DR](build-and-deploy.md#backup-and-disaster-recovery)
- Having deployment issues? → [Troubleshooting](troubleshooting.md)

## Deployment Environments

| Environment     | Purpose                | Configuration                       | Scale           |
| --------------- | ---------------------- | ----------------------------------- | --------------- |
| **Development** | Local development      | LocalDB, debug settings             | 1 instance      |
| **Staging**     | Pre-production testing | SQL Express/Azure SQL, Shared Redis | 2 instances     |
| **Production**  | Live users             | Azure SQL Premium, Premium Redis    | Auto-scale 3-10 |

## Deployment Overview

```
┌─────────────────────┐
│  GitHub Repo        │
│  (push to main)     │
└──────────┬──────────┘
           │
           v
┌─────────────────────────────────────────┐
│  GitHub Actions (CI/CD Pipeline)        │
│  - Build & test                         │
│  - Run security scans                   │
│  - Create Docker image                  │
│  - Push to container registry           │
└──────────┬──────────────────────────────┘
           │
           v
┌──────────────────────────────────────────┐
│  Deploy to Azure App Service             │
│  - Pull Docker image                     │
│  - Run EF Core migrations                │
│  - Update environment variables          │
│  - Restart instances                     │
└──────────┬───────────────────────────────┘
           │
           v
┌──────────────────────────────────────────┐
│  Production Environment                  │
│  - Azure App Service (3-10 instances)    │
│  - Azure SQL (Premium, geo-replicated)   │
│  - Application Insights (monitoring)     │
└──────────────────────────────────────────┘
```

## Quick Deployment Steps

1. **Build Release:** `dotnet publish -c Release -o ./publish`
2. **Run tests:** `dotnet test`
3. **Run migrations:** `dotnet ef database update --project src/QRDine.Infrastructure`
4. **Deploy:** Push to main branch → GitHub Actions → Azure App Service
5. **Monitor:** Check Application Insights for errors and performance

See [Build & Deployment Guide](build-and-deploy.md) for complete documentation with all commands and configurations.

---

**Reference:** See also [Configuration](../configuration/) for environment setup and [Troubleshooting](../troubleshooting.md) for deployment issues.
