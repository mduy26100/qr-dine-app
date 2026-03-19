# Docker & Containerization

Complete production-ready Docker setup for QRDine with containerization, orchestration, and deployment guides.

## Contents

- **[Quick Start Guide](quick-start.md)** — Get Docker running in 5 minutes:
  - Setup scripts (Windows, Linux, Mac)
  - Configuration steps
  - Service verification
  - Health checks

- **[Deployment Guide](deployment-guide.md)** — Comprehensive deployment documentation:
  - Architecture overview (services, networking, volumes)
  - Configuration details (Dockerfile, docker-compose.yml, nginx.conf, .env)
  - Running application (start, stop, rebuild, logs)
  - Monitoring & logging
  - Security best practices
  - Troubleshooting (30+ common issues)
  - Performance tuning
  - Scaling strategies

- **[Architecture & Implementation](architecture.md)** — Technical deep-dive:
  - Docker configuration breakdown
  - Multi-stage build optimization
  - Environment variables mapping (.env → .NET settings)
  - Service dependencies & health checks
  - Performance metrics
  - Security checklist
  - Common mistakes to avoid

- **[Command Reference](command-reference.md)** — 100+ Docker commands:
  - Basic operations (start, stop, logs)
  - Building & rebuilding
  - Debugging (shell, exec, inspect)
  - Database operations (SQL Server, Redis)
  - Health & connectivity
  - Monitoring & resource tracking
  - Useful combinations

## Quick Links

- Want to start immediately? → [Quick Start](quick-start.md)
- Deploying for first time? → [Deployment Guide - Setup](deployment-guide.md#quick-start)
- Need Docker commands? → [Command Reference](command-reference.md)
- Understanding the setup? → [Architecture & Implementation](architecture.md)
- Having database issues? → [Troubleshooting - Database](deployment-guide.md#troubleshooting)
- Need performance tuning? → [Performance Tuning](deployment-guide.md#performance-tuning)
- Production deployment? → [Production Checklist](deployment-guide.md#production-checklist)

## 🐳 Services Overview

| Service        | Image                        | Port   | Purpose                         | Health Check     |
| -------------- | ---------------------------- | ------ | ------------------------------- | ---------------- |
| **api**        | Custom (.NET 8)              | 8080   | QRDine API (Kestrel)            | `/health/live` ✓ |
| **sql-server** | mcr.microsoft.com/mssql:2022 | 1433\* | Database (SQL Server Express)   | SQL query ✓      |
| **redis**      | redis:7-alpine               | 6379\* | Cache layer with persistence    | PING ✓           |
| **nginx**      | nginx:1.27-alpine            | 80     | Reverse proxy (HTTP→Cloudflare) | Waits for API ✓  |

\*Internal only (no external port exposure)

## 🚀 One Command Deployment

```bash
# Setup (automatic environment, images, network)
setup.bat                    # Windows
./setup.sh                   # Linux/Mac

# Configure
nano .env                    # Update credentials

# Deploy
docker-compose up -d

# Verify
docker-compose ps           # Check services (all should be "up")
curl http://localhost/health/live  # Liveness check via Nginx port 80
```

## 📊 Architecture

```
Cloudflare (HTTPS Edge)
    ↓ (HTTP port 80)
[Nginx] - HTTP Reverse Proxy, Gzip, Security Headers
    ↓ (internal port 8080)
[.NET 8 API] - Health Check (/health/live, /health/ready)
    ├→ [SQL Server] - Database (port 1433, internal)
    ├→ [Redis] - Cache (port 6379, internal)
    └→ External APIs (Cloudinary, PayOS, Email, etc.)
```

## 🔐 Security Features

✅ Non-root container user (appuser:1000)  
✅ HTTPS/TLS at Cloudflare edge (flexible SSL mode)  
✅ HTTP-only Nginx (port 80, internal API connections)  
✅ Security headers (X-Real-IP from Cloudflare, X-Forwarded-Proto)  
✅ All environment variables in `.env` (no hardcoded secrets)  
✅ Health checks with automatic restart on failure  
✅ Internal network isolation (bridge overlay network)  
✅ Memory limits on containers (SQL Server 1536MB, Redis 200MB)  
✅ Database password escaping in healthcheck commands

- **Image size**: ~200MB (multi-stage optimized)
- **Startup time**: 10-15 seconds
- **Gzip compression**: 60-80% reduction
- **Response caching**: 10 minutes TTL
- **Connection pooling**: 32 keepalive per Nginx

## 📝 File Structure

```
qr-dine-app/
├── Dockerfile                 # Multi-stage build
├── docker-compose.yml         # Orchestration
├── nginx.conf                 # Reverse proxy config
├── .env                       # Runtime secrets
├── .env.example               # Template
├── .dockerignore              # Build context
├── setup.bat / setup.sh       # Automation
├── src/
│   └── QRDine.API/
│       └── Controllers/
│           └── HealthController.cs
└── docs/docker/               # This folder
    ├── README.md
    ├── quick-start.md
    ├── deployment-guide.md
    ├── architecture.md
    └── command-reference.md
```

## ✅ Deployment Environments

| Environment     | Purpose              | Configuration            | Scale      |
| --------------- | -------------------- | ------------------------ | ---------- |
| **Development** | Local Docker testing | Dev passwords, logs      | Single     |
| **Production**  | Live deployment      | Strong secrets, real SSL | Multi-zone |

---

**Return to:** [Main Documentation](../README.md)
