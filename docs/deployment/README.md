# Deployment

Build process, deployment procedures, CI/CD pipelines, and operations.

## Contents

- **[GitHub Actions CI/CD Setup](github-actions-setup.md)** — Automated deployment using GitHub Actions:
  - GitHub Secrets configuration (HOST_IP, USERNAME, SSH_PRIVATE_KEY)
  - VPS preparation and environment setup
  - Workflow configuration and deployment flow
  - Zero-downtime deployment strategy
  - Safe cleanup procedures
  - Troubleshooting and monitoring
  - Security best practices
  - Rollback procedures

- **[Workflows Quick Reference](workflows-quick-reference.md)** — Quick lookup for common tasks:
  - GitHub workflows overview
  - Quick start checklist
  - Common tasks reference
  - Deployment workflow stages
  - Troubleshooting quick links
  - Security reminders

- **[Build & Deployment Guide](build-and-deploy.md)** — Complete deployment documentation including:
  - Build process (Debug vs Release)
  - Pre-deployment checklist (code quality, security, performance)
  - Azure App Service deployment (step-by-step)
  - Docker containerization with Dockerfile
  - Database migrations in production
  - Monitoring and logging (Application Insights)
  - Performance optimization (connection pooling, caching, scaling)
  - Backup and disaster recovery (automated, manual, geo-replication)
  - Production checklist

- **[Troubleshooting](troubleshooting.md)** — Solutions for common issues:
  - Database connection and migration errors
  - Authentication and JWT token issues
  - API validation and authorization errors
  - Runtime dependency injection failures
  - Deployment and VPS-specific issues
  - Performance and memory problems
  - Logging and debugging tips

## Quick Links

- Setting up GitHub Actions? → [GitHub Actions Setup](github-actions-setup.md)
- GitHub Secrets configuration? → [GitHub Actions - Secrets](github-actions-setup.md#-bước-1-thiết-lập-github-secrets)
- VPS preparation? → [GitHub Actions - VPS Setup](github-actions-setup.md#-bước-2-cấu-hình-trên-vps)
- Understanding deployment flow? → [GitHub Actions - Workflow Flow](github-actions-setup.md#-workflow-flow)
- Deployment troubleshooting? → [GitHub Actions - Troubleshooting](github-actions-setup.md#-troubleshooting)
- What to check before deploying? → [Build & Deployment - Pre-Deployment Checklist](build-and-deploy.md#pre-deployment-checklist)
- Deploying to Azure? → [Build & Deployment - Azure App Service](build-and-deploy.md#azure-app-service-deployment)
- Using Docker? → [Build & Deployment - Docker](build-and-deploy.md#docker-deployment)
- Database migrations? → [Build & Deployment - Migrations](build-and-deploy.md#database-migrations)
- Backup strategy? → [Build & Deployment - Backup & DR](build-and-deploy.md#backup-and-disaster-recovery)
- Having deployment issues? → [Troubleshooting](troubleshooting.md)

## Deployment Environments

| Environment     | Platform      | Configuration                        | Auto-Deploy                    |
| --------------- | ------------- | ------------------------------------ | ------------------------------ |
| **Development** | Local machine | LocalDB, debug settings              | Manual (dotnet run)            |
| **Staging**     | VPS/Server    | SQL Express/SQL Server, Shared Redis | On push to `staging` branch    |
| **Production**  | VPS/Server    | SQL Server 2022, Redis 7, Nginx      | On push/merge to `main` branch |

## Deployment Overview

### GitHub Actions → VPS Deployment

```
┌─────────────────────┐
│  GitHub Repository  │
│  (push to main)     │
└──────────┬──────────┘
           │
           v
┌──────────────────────────────────────┐
│  GitHub Actions Workflow             │
│  (ubuntu-latest runner)              │
│  - Checkout code                     │
│  - SSH to VPS                        │
└──────────┬───────────────────────────┘
           │
           v
┌──────────────────────────────────────────┐
│  VPS Production (Linux)                  │
├──────────────────────────────────────────┤
│  ┌────────────────────────────────────┐  │
│  │  Docker Compose Network            │  │
│  ├────────────────────────────────────┤  │
│  │  1. SQL Server 2022                │  │ (Database)
│  │  2. Redis 7                        │  │ (Cache)
│  │  3. .NET 8 API (C#)                │  │ (Application)
│  │  4. Nginx 1.27                     │  │ (Reverse Proxy)
│  └────────────────────────────────────┘  │
│                                          │
│  Data Volumes:                           │
│  - sqlserver_data                        │
│  - redis_data                            │
│  - nginx_logs                            │
└──────────────────────────────────────────┘
```

**Workflow steps:**

1. Push code to `main` branch
2. GitHub Actions triggers
3. Checkout repository code
4. SSH into VPS using credentials
5. Pull latest code: `git pull origin main`
6. Load environment: `source .env`
7. Deploy: `docker compose up -d --build --remove-orphans`
8. Verify health checks
9. Cleanup dangling images

**Zero-downtime deployment:** Docker rebuilds images in background, then atomically replaces old containers.

### Traditional Azure Deployment (Alternative)

```
┌─────────────────────┐
│  GitHub Repo        │
│  (push to main)     │
└──────────┬──────────┘
           │
           v
┌──────────────────────────────┐
│  GitHub Actions              │
│  - Build & test              │
│  - Create Docker image       │
│  - Push to ACR               │
└──────────┬───────────────────┘
           │
           v
┌──────────────────────────────┐
│  Deploy to Azure App Service │
│  - Pull Docker image         │
│  - Run migrations            │
│  - Update config             │
│  - Restart instances         │
└──────────┬───────────────────┘
           │
           v
┌──────────────────────────────┐
│  Production                  │
│  - App Service (3-10)        │
│  - Azure SQL                 │
│  - Application Insights      │
└──────────────────────────────┘
```

See [GitHub Actions Setup](github-actions-setup.md) for detailed VPS deployment guide.

## Quick Deployment Steps

### For VPS with GitHub Actions (Recommended)

1. **Setup GitHub Secrets** (first time):
   ```
   HOST_IP, USERNAME, SSH_PRIVATE_KEY
   ```
2. **Prepare VPS** (first time):
   ```bash
   mkdir -p /root/qr-dine-app
   cd /root/qr-dine-app
   git init && git remote add origin https://your-repo-url
   # Create .env file with all required variables
   ```
3. **Deploy**:
   ```bash
   git push origin main  # Workflow triggers automatically
   ```
4. **Monitor**:
   - GitHub Actions → Actions tab
   - Or SSH: `ssh root@HOST_IP` → `docker compose logs -f`

See [GitHub Actions Setup](github-actions-setup.md) for complete guide.

### For Local/Development

1. **Build Release:** `dotnet publish -c Release -o ./publish`
2. **Run tests:** `dotnet test`
3. **Run migrations:** `dotnet ef database update --project src/QRDine.Infrastructure`
4. **Local testing:**
   ```bash
   docker compose up -d
   curl http://localhost/health/live
   ```

---

**First time deploying?** Start with [GitHub Actions Setup](github-actions-setup.md)

**Reference:** See also [Configuration](../configuration/) for environment setup, [Docker Architecture](../docker/architecture.md) for containerization details, and [Troubleshooting](troubleshooting.md) for deployment issues.
