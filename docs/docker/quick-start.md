# Docker Quick Start

Get QRDine Docker running in 5 minutes.

## 🚀 Quick Start (5 minutes)

### Prerequisites

- Docker Desktop (Windows/Mac) or Docker + Docker Compose (Linux)
- Git (to clone repository)

### Step 1: Run Setup

**Windows**

```bash
setup.bat
```

**Linux / Mac**

```bash
chmod +x setup.sh
./setup.sh
```

### Step 2: Configure

Edit `.env` with your actual credentials:

```bash
# Edit the .env file
nano .env
# or your preferred editor (code .env, vim .env, etc.)
```

**Update these critical values:**

- `MSSQL_SA_PASSWORD` - Strong database password
- `REDIS_PASSWORD` - Strong cache password
- `JWT_SECRET` - Unique JWT signing key
- `CLOUDINARY_*` - Your Cloudinary API keys
- `EMAIL_SENDER_*` - Your email credentials
- `PAYOS_*` - Your PayOS credentials
- `CORS_ALLOWED_ORIGIN_*` - Your frontend URLs

### Step 3: Start Services

```bash
docker-compose up -d
```

### Step 4: Verify

```bash
# Check all services are running and healthy
docker-compose ps

# API health check (Nginx routes through port 80)
curl http://localhost/health/live

# View logs
docker-compose logs -f api
```

✅ **Everything running!** Nginx is listening on port 80.

---

## 📋 What Gets Set Up

### Services

- **Nginx** (Reverse Proxy) - Port 80 (HTTP only, external)
  - Cloudflare provides HTTPS at edge
- **API** (.NET 8) - Port 8080 (internal only, behind Nginx)
- **SQL Server** 2022 Express - Port 1433 (internal only)
- **Redis** 7 Alpine - Port 6379 (internal only)

### Network Architecture

```
Internet (HTTPS via Cloudflare)
    ↓ (HTTP port 80)
  Nginx (1.27 Alpine, Reverse Proxy)
    ↓ (Internal port 8080)
  API (.NET 8, Health Check /health/live)
    ↓
  SQL Server (Port 1433, in qrdine-network)
  Redis (Port 6379, in qrdine-network)
```

### Configuration

- **Environment Variables** - All config from `.env`
- **.NET Configuration** - Automatic mapping (\_\_ → : in env vars)
- **Health Checks** - All 4 services auto-validated on startup
- **Data Persistence** - sqlserver_data, redis_data volumes

---

## 🔧 Manual Setup (Alternative)

If you prefer manual setup instead of scripts:

```bash
# 1. Copy environment template
cp .env.example .env

# 2. Edit with your values
nano .env

# 3. Pull latest images
docker-compose pull

# 4. Build custom API image
docker-compose build

# 5. Start services
docker-compose up -d

# 6. Verify health
docker-compose ps
```

**Note**: SSL/TLS is handled by Cloudflare (not Docker). Nginx only listens on port 80 (HTTP).

---

## 📊 Architecture

```
          Cloudflare (HTTPS Edge)
                  ↓ (HTTP port 80)
          ┌───────────────────┐
          │     Nginx         │
          │   HTTP Reverse    │
          │   Proxy + Gzip    │
          └─────────┬─────────┘
                    ↓ (Internal port 8080)
        ┌───────────────────────────┐
        │      .NET 8 API           │
        │    (Health Check)          │
        │  /health/live(ready)      │
        └─┬───────────────────────┬─┘
          │                       │
    ┌─────▼──────┐        ┌──────▼──────┐
    │ SQL Server │        │   Redis     │
    │ (1433)     │        │   (6379)    │
    │ qrdine-net │        │ qrdine-net  │
    └────────────┘        └─────────────┘
```

**Note**: All internal services communicate via `qrdine-network` overlay network. Only Nginx port 80 is externally exposed.

---

## ☑️ Quick Verification

After `docker-compose up -d`, verify each component:

```bash
# 1. Check all services are running and HEALTHY
docker-compose ps
# Expected: All services showing "Up" with health status (if applicable)

# 2. API liveness probe (via Nginx on port 80)
curl http://localhost/health/live
# Expected: {"status":"Healthy",...}

# 3. API readiness probe (checks DB + Cache)
curl http://localhost/health/ready
# Expected: {"status":"Healthy",...}

# 4. Full health report
curl http://localhost/health
# Expected: Complete health report with all checks

# 5. View service logs (check for errors)
docker-compose logs api
# Expected: No critical errors, "Starting application..." message

# 6. Verify internal connection (from API to DB)
docker exec qrdine-api curl -f http://localhost:8080/health/live
# Expected: 200 OK (API responding on internal port 8080)
```

**Health Check Status**:

- 🟢 All services "Up" = System ready
- 🟡 "Up (health: starting)" = Wait 30 seconds, then retry
- 🔴 "Exited" or "Health: unhealthy" = Check `docker-compose logs`

---

## 🔄 Common Operations

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api      # API logs
docker-compose logs -f sql-server  # Database logs
docker-compose logs -f redis    # Cache logs
docker-compose logs -f nginx    # Reverse proxy logs

# Last 100 lines
docker-compose logs -f --tail=100 api
```

### Stop Services

```bash
# Stop (keep data)
docker-compose stop

# Stop and remove (delete data)
docker-compose down -v

# Restart specific service
docker-compose restart api
```

### Update & Rebuild

```bash
# Update code in src/
# Then rebuild API image
docker-compose build --no-cache api

# Restart service
docker-compose restart api
```

---

## 🐛 Troubleshooting

### Services won't start

```bash
# Check logs
docker-compose logs

# Rebuild without cache
docker-compose build --no-cache

# Clean and restart
docker-compose down -v
docker-compose up -d
```

### Port already in use

```bash
# Find what's using the port (Windows)
netstat -ano | findstr :5000

# Or (Linux/Mac)
lsof -i :5000

# Either stop that process or change port in .env
# API_PORT=5001
```

### Connection refused

```bash
# Check if service is running
docker-compose ps

# Check service health
docker inspect qrdine-api --format='{{.State.Health.Status}}'

# View service logs
docker-compose logs sql-server
docker-compose logs redis
```

### Docker image won't build

```bash
# Clean images
docker system prune -a

# Rebuild with full output
docker-compose build --no-cache api

# Check Docker resources
docker system df
```

---

## 📈 Performance Tips

- **Gzip**: Enabled by default, reduces payload 60-80%
- **Caching**: 10 minute TTL for API responses
- **Connection Pooling**: 32 keepalive connections per Nginx worker
- **Workers**: Auto-tuned to CPU cores
- **Image Size**: ~200MB (optimized multi-stage build)

---

## 🔐 Security Notes

✅ **Production-ready security:**

- Non-root user (appuser)
- HTTPS/TLS at Nginx layer
- Security headers (HSTS, X-Frame-Options, CSP)
- Environment variables for all secrets
- Auto-restart on failure
- Network isolation

⚠️ **Before production:**

- Use real SSL certificates (not self-signed)
- Update all passwords to strong values
- Configure real CORS origins
- Setup proper backups
- Enable Redis persistence

---

## 📞 Next Steps

- **Need more details?** → [Deployment Guide](deployment-guide.md)
- **Want all commands?** → [Command Reference](command-reference.md)
- **Understanding architecture?** → [Architecture & Implementation](architecture.md)
- **Having issues?** → [Deployment Guide - Troubleshooting](deployment-guide.md#troubleshooting)

---

**Return to:** [Docker Documentation](README.md) | [Main Documentation](../README.md)
