# Docker Deployment Guide

Complete production deployment documentation for QRDine using Docker, Docker Compose, and Nginx.

## Tổng Quan

Hướng dẫn này cung cấp setup production-ready với:

- **QRDine.API** (.NET 8 API) - Port 8080
- **SQL Server** (Database) - Port 1433
- **Redis** (Cache) - Port 6379
- **Nginx** (Reverse Proxy) - Port 80/443

---

## 🚀 Quick Start

### Prerequisites

- Docker Desktop (Windows/Mac) or Docker + Docker Compose (Linux)
- OpenSSL (optional, for SSL certificates)

### Setup

**Windows**

```bash
setup.bat
```

**Linux/Mac**

```bash
chmod +x setup.sh
./setup.sh
```

### Manual Setup

```bash
# 1. Copy environment
cp .env.example .env

# 2. Update .env với credentials
vim .env

# 3. Generate certificates (optional)
mkdir -p certs
openssl req -x509 -newkey rsa:4096 -nodes \
  -out certs/server.crt -keyout certs/server.key -days 365 \
  -subj "/C=VN/ST=HCM/L=Ho Chi Minh/O=QRDine/CN=localhost"

# 4. Start application
docker-compose up -d

# 5. View logs
docker-compose logs -f api
```

---

## 📁 File Structure & Cấu Hình

### 1. Dockerfile

**Architecture**: Multi-stage build để tối ưu image size

```dockerfile
# Stage 1 (build): Contains .NET SDK, compiles code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Stage 2 (publish): Creates release build
FROM build AS publish

# Stage 3 (final): Minimal runtime image only
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# - Non-root user (appuser:1000)
# - Health checks
# - EXPOSE 8080
```

**Key Features**:

- ✅ Image size: ~200MB (vs 1GB+ without optimization)
- ✅ Non-root user security
- ✅ Health check endpoint
- ✅ No build tools in final image

### 2. docker-compose.yml

#### Services

**sql-server** (Database)

```yaml
Image: mcr.microsoft.com/mssql/server:2022-latest
Port: 1433 (mapped to host)
Volume: sqlserver_data (persistent)
Health: SQL query-based health check
```

**redis** (Cache)

```yaml
Image: redis:7-alpine
Port: 6379 (mapped to host)
Volume: redis_data (persistent)
Auth: Password from .env
Persistence: AOF enabled (appendonly yes)
```

**api** (.NET Application)

```yaml
Image: Custom (built from Dockerfile)
Port: 8080 (internal only, no external mapping)
Health Check: /health/live endpoint (30s interval, 30s start period)
depends_on: sql-server, redis (condition: service_healthy)
Environment: All config from .env file
Volume: logs (logs directory for application output)
Network: qrdine-network (internal)
Restart: unless-stopped
```

**nginx** (Reverse Proxy)

```yaml
Image: nginx:1.27-alpine
Port: 80 (HTTP only, Cloudflare handles HTTPS)
Volumes: nginx.conf, logs
depends_on: api (condition: service_healthy)
Network: qrdine-network (internal)
Restart: unless-stopped
```

````

#### Network & Volumes

```yaml
networks:
  qrdine-network:
    driver: bridge # Internal DNS resolution

volumes:
  sqlserver_data: # Database persistence (/var/opt/mssql)
  redis_data: # Cache persistence (/data)
  nginx_logs: # Access/error logs
````

### 3. nginx.conf

Production-grade HTTP reverse proxy configuration (HTTPS via Cloudflare):

```nginx
# Compression
gzip on;
gzip_types text/plain text/css application/json;
gzip_comp_level 6;
gzip_min_length 1000;

# Upstream API
upstream api {
    least_conn;
    server api:8080;
}

# HTTP only (Cloudflare provides HTTPS at edge)
server {
    listen 80;
    server_name _;

    # Proxy Configuration with Cloudflare Headers
    location / {
        proxy_pass http://api;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $http_cf_connecting_ip;  # Cloudflare client IP
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $http_x_forwarded_proto;  # HTTPS signal from CF
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # WebSocket Support (SignalR)
    location /hubs/ {
        proxy_pass http://api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $http_cf_connecting_ip;
        proxy_set_header X-Forwarded-Proto $http_x_forwarded_proto;
        proxy_connect_timeout 7d;
        proxy_send_timeout 7d;
        proxy_read_timeout 7d;
    }
}
```

### 4. .env Configuration

Tất cả configuration phải được định nghĩa trong `.env` (from .env.example):

```env
# Application
ASPNETCORE_ENVIRONMENT=Production

# Database
Server=sql-server,1433
SA_PASSWORD=YourSecurePassword123!@#
MSSQL_CONNECTION_STRING=Server=sql-server,1433;Database=QRDine;User Id=sa;Password=YourSecurePassword123!@#;TrustServerCertificate=True;

# Cache
REDIS_PASSWORD=YourSecureRedisPassword123!@#
REDIS_CONNECTION_STRING=redis:6379,password=YourSecureRedisPassword123!@#

# JWT
JWT_VALID_ISSUER=https://api.qrdine.me
JWT_VALID_AUDIENCE=https://api.qrdine.me

# External Services
CLOUDINARY_CLOUD_NAME=your_cloud_name
EMAIL_SENDER_EMAIL=your_email@gmail.com
PAYOS_CLIENT_ID=your_payos_client_id

# CORS & Frontend
FRONTEND_BASE_URL=https://qrdine.me
CORS_ALLOWED_ORIGIN_1=https://qrdine.me
CORS_ALLOWED_ORIGIN_2=https://app.qrdine.me
```

**Note**: All ports are internal-only (no external mappings):

- SQL Server 1433 (qrdine-network only)
- Redis 6379 (qrdine-network only)
- API 8080 (qrdine-network only, behind nginx)
- Nginx 80 (external, HTTP only - Cloudflare provides HTTPS)

---

## ⚙️ Environment Setup

### Development

```env
ASPNETCORE_ENVIRONMENT=Development
JWT_VALID_ISSUER=http://localhost:5000
FRONTEND_BASE_URL=http://localhost:5173
CORS_ALLOWED_ORIGIN_1=http://localhost:5173
```

### Production

```env
ASPNETCORE_ENVIRONMENT=Production
JWT_VALID_ISSUER=https://api.qrdine.me
FRONTEND_BASE_URL=https://qrdine.me
CORS_ALLOWED_ORIGIN_1=https://qrdine.me
```

---

## 🚀 Operating Application

### Start

```bash
docker-compose up -d
```

### Stop

```bash
docker-compose down          # Stop containers (keep data)
docker-compose down -v       # Stop and remove volumes (DELETE DATA)
```

### Logs

```bash
docker-compose logs -f             # All services
docker-compose logs -f api         # API only
docker-compose logs -f --tail=100  # Last 100 lines
```

### Health Check

All services include automated health checks for orchestration and monitoring:

**API Health Endpoints**

```bash
# Liveness probe (service is running)
curl http://localhost:8080/health/live

# Readiness probe (service ready to handle requests)
curl http://localhost:8080/health/ready

# Full health report
curl http://localhost:8080/health
```

**Health Check Strategy**

| Service        | Check Command                | Interval | Timeout | Start Period | Retries |
| -------------- | ---------------------------- | -------- | ------- | ------------ | ------- |
| **sql-server** | `sqlcmd -U sa -Q "SELECT 1"` | 10s      | 5s      | 30s          | 10      |
| **redis**      | `redis-cli ping`             | 10s      | 5s      | —            | 5       |
| **api**        | HTTP GET `/health/live`      | 30s      | 5s      | 30s          | 3       |
| **nginx**      | Waits for api healthy        | —        | —       | —            | —       |

**View Health Status**

```bash
# Check all containers
docker-compose ps

# Watch real-time status
docker-compose ps --no-trunc

# View health details
docker inspect qrdine-api | grep -A 8 '"Health"'
```

**Rebuild Containers (Clear Health State)**

````bash
docker-compose down -v
docker-compose up -d

```bash
docker-compose build              # Build images
docker-compose build --no-cache   # Rebuild without cache
````

---

## 🐛 Troubleshooting

### Container won't start

```bash
# View logs
docker-compose logs api

# Check image
docker images | grep qrdine

# Full clean rebuild
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

### Database connection failed

```bash
# Test SQL Server health
docker exec qrdine-sqlserver /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P $SA_PASSWORD -Q "SELECT 1"

# Verify connection string format
# Should be: Server=sql-server,1433;Database=QRDine;User Id=sa;Password=...;TrustServerCertificate=True;
```

### Redis connection failed

```bash
# Test Redis
docker exec qrdine-redis redis-cli ping

# Test with auth
docker exec qrdine-redis redis-cli -a "PASSWORD" ping
```

### Nginx 502 Bad Gateway

```bash
# Check API is running
docker-compose ps api

# Test connectivity to API
docker exec qrdine-nginx curl -v http://api:8080

# Check Nginx logs
docker exec qrdine-nginx tail -f /var/log/nginx/error.log
```

### Port conflicts

```bash
# Windows - Check if port 80 (Nginx) is in use
netstat -ano | findstr :80

# Linux/Mac
lsof -i :80

# Note: Internal ports (1433, 6379, 8080) are NOT exposed externally
# They exist only on qrdine-network (internal overlay network)
```

---

## 📊 Monitoring & Logs

### Service Status

```bash
# All services
docker-compose ps

# Detailed info
docker inspect qrdine-api
```

### Resource Usage

```bash
# Real-time usage
docker stats

# Specific container
docker stats qrdine-api
```

### Log Analysis

```bash
# Error logs
docker-compose logs api | grep -i error

# Access logs
docker exec qrdine-nginx tail -f /var/log/nginx/access.log

# Application logs
docker exec qrdine-api ls -la /app/logs
```

---

## 📈 Performance Tuning

### Nginx

```nginx
# Auto-detect CPU cores
worker_processes auto;

# Connections per worker
worker_connections 2048;

# Compression level (1-9)
gzip_comp_level 6;

# Connection pooling
keepalive 32;
```

### Database

- SQL Server Express: Max 10GB database, 1GB RAM
- Production: Use Standard Edition or Azure SQL
- Enable backups and monitoring

### Redis

```bash
# Check memory usage
docker exec qrdine-redis redis-cli INFO memory

# Enable persistence
# Update redis command in docker-compose: redis-server --appendonly yes
```

---

## 🔐 Security Best Practices

✅ **Applied**:

- Non-root user (appuser:1000)
- Environment variables for secrets
- HTTPS/TLS configuration
- Security headers
- Health checks with auto-restart
- Network isolation (bridge)

❌ **Add Later**:

- Database backup automation
- Secret management (Vault, AWS Secrets Manager)
- Monitoring & alerting (Prometheus, Grafana)
- Centralized logging (ELK Stack)

---

## ✅ Production Checklist

### Pre-Production

- [ ] Environment variables secure
- [ ] SSL certificates obtained (real, not self-signed)
- [ ] Database backups configured
- [ ] Nginx caching tuned
- [ ] Health checks verified
- [ ] Logging configured
- [ ] Backups scheduled

### Post-Deployment

- [ ] All services healthy
- [ ] API responding to requests
- [ ] Database connectivity verified
- [ ] Redis cache working
- [ ] Nginx routing correct
- [ ] HTTPS working
- [ ] CORS properly configured

---

## 🔄 Scaling

For horizontal scaling:

1. **Multiple API instances**: Configure in docker-compose.yml
2. **Load balancer**: Update upstream in nginx.conf
3. **Database replication**: SQL Server AG
4. **Cache cluster**: Redis Sentinel or Redis Cluster

---

**Return to:** [Docker Documentation](README.md) | [Command Reference](command-reference.md)
