# Docker Command Reference

100+ useful Docker commands for QRDine management, debugging, and operations.

---

## 🚀 Service Management

### Start / Stop

```bash
# Start services in background
docker-compose up -d

# Start in foreground (see logs directly)
docker-compose up

# Stop services
docker-compose stop

# Remove services & containers
docker-compose down

# Remove services & volumes (DELETE DATA)
docker-compose down -v

# Restart all services
docker-compose restart

# Restart specific service
docker-compose restart api
docker-compose restart nginx
```

### Build & Update

```bash
# Build all images
docker-compose build

# Build without cache
docker-compose build --no-cache

# Build specific service
docker-compose build api

# Pull latest base images
docker-compose pull
```

---

## 📊 Status & Monitoring

### Service Status

```bash
# Show running containers
docker-compose ps

# Show detailed container status
docker-compose ps -a

# Real-time resource usage
docker stats

# Specific container resource usage
docker stats qrdine-api

# Disk space usage
docker system df
```

### Health & Logs

```bash
# All service logs (follow mode)
docker-compose logs -f

# Specific service logs
docker-compose logs -f api
docker-compose logs -f sql-server
docker-compose logs -f redis
docker-compose logs -f nginx

# Last N lines
docker-compose logs -f --tail=100 api

# Logs since specific time
docker-compose logs --since=10m

# Search in logs
docker-compose logs api | grep -i error
```

---

## 🔧 Debugging & Troubleshooting

### Container Inspection

```bash
# Container detailed info
docker inspect qrdine-api

# Container IP address
docker inspect -f '{{range.NetworkSettings.Networks}}{{.IPAddress}}{{end}}' qrdine-api

# Container health status
docker inspect qrdine-api --format='{{.State.Health.Status}}'

# Container variables
docker exec qrdine-api env | sort

# Container processes
docker ps --all --format "table {{.ID}}\t{{.Names}}\t{{.Status}}"
```

### Execute Commands

```bash
# Run command in container
docker exec qrdine-api ls -la /app

# Interactive shell
docker exec -it qrdine-api /bin/bash

# Run shell as specific user
docker exec -it -u appuser qrdine-api /bin/bash

# View container file system
docker exec qrdine-api pwd
```

### Network Diagnostics

```bash
# Check network
docker network ls
docker network inspect qrdine-network

# Test DNS resolution (inside container)
docker exec qrdine-api nslookup redis
docker exec qrdine-api nslookup sql-server
docker exec qrdine-api nslookup api

# Test connectivity
docker exec qrdine-api ping -c 3 redis
docker exec qrdine-api telnet sql-server 1433

# View network configuration
docker network inspect qrdine-network | grep -i "ipaddress"
```

---

## 💾 Database Operations

### SQL Server

```bash
# Connect to SQL Server
docker exec -it qrdine-sqlserver /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa

# Run query (with password prompt)
docker exec -it qrdine-sqlserver /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P "PASSWORD" -Q "SELECT 1"

# List databases
docker exec qrdine-sqlserver /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P "PASSWORD" \
  -Q "SELECT name FROM sys.databases"

# Backup database
docker exec qrdine-sqlserver /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P "PASSWORD" \
  -Q "BACKUP DATABASE QRDine TO DISK = '/var/opt/mssql/backup/qrdine.bak'"

# Check SQL Server health
docker exec qrdine-sqlserver /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P "PASSWORD" -Q "SELECT 1"

# View error log
docker logs qrdine-sqlserver
```

### Redis

```bash
# Test connection
docker exec qrdine-redis redis-cli ping

# Test with password
docker exec qrdine-redis redis-cli -a "PASSWORD" ping

# List all keys
docker exec qrdine-redis redis-cli SCAN 0

# Get specific key
docker exec qrdine-redis redis-cli GET "key_name"

# Delete key
docker exec qrdine-redis redis-cli DEL "key_name"

# Clear all cache
docker exec qrdine-redis redis-cli FLUSHALL

# Memory info
docker exec qrdine-redis redis-cli INFO memory

# Key count
docker exec qrdine-redis redis-cli DBSIZE

# Monitor commands
docker exec qrdine-redis redis-cli MONITOR

# Export data
docker exec qrdine-redis redis-cli --rdb /data/dump.rdb
```

---

## 📋 Health Checks

### API Health

```bash
# Via Nginx (port 80, external)
curl http://localhost/health

# Liveness probe (via Nginx)
curl http://localhost/health/live

# Readiness probe (via Nginx)
curl http://localhost/health/ready

# Verbose output (via Nginx)
curl -v http://localhost/health

# With JSON parsing (via Nginx)
curl http://localhost/health | jq

# Direct API test (internal, port 8080)
docker exec qrdine-api curl http://localhost:8080/health/live
```

### Service Health

```bash
# API container health
docker inspect qrdine-api --format='{{.State.Health.Status}}'

# SQL Server health
docker inspect qrdine-sqlserver --format='{{.State.Health.Status}}'

# Redis health
docker inspect qrdine-redis --format='{{.State.Health.Status}}'

# Nginx health
docker inspect qrdine-nginx --format='{{.State.Health.Status}}'
```

### Connectivity Tests

```bash
# API → Database
docker exec qrdine-api curl http://sql-server:1433

# API → Redis
docker exec qrdine-api curl http://redis:6379

# Nginx → API
docker exec qrdine-nginx curl -I http://api:8080

# External → Nginx
curl -I http://localhost/health
```

---

## 📝 Log Management

### View Logs

```bash
# Docker container logs
docker logs qrdine-api

# Follow logs
docker logs -f qrdine-api

# Last N lines
docker logs --tail=100 qrdine-api

# Since specific time
docker logs --since="2024-01-15T10:00:00" qrdine-api

# Until specific time
docker logs --until="2024-01-15T15:00:00" qrdine-api
```

### Nginx Logs

```bash
# Access log
docker exec qrdine-nginx tail -f /var/log/nginx/access.log

# Error log
docker exec qrdine-nginx tail -f /var/log/nginx/error.log

# View recent requests
docker exec qrdine-nginx tail -20 /var/log/nginx/access.log

# Count requests
docker exec qrdine-nginx wc -l /var/log/nginx/access.log

# View specific status codes
docker exec qrdine-nginx grep "HTTP/1" /var/log/nginx/access.log | grep "200"
```

### Application Logs

```bash
# List log files
docker exec qrdine-api ls -la /app/logs

# View application logs
docker exec qrdine-api tail -f /app/logs/*

# Search for errors
docker exec qrdine-api grep -r "ERROR" /app/logs
```

---

## 🔐 Security & Configuration

### View Configuration

```bash
# View .env
cat .env | head -20

# View env vars in container
docker exec qrdine-api env | grep ASPNETCORE
docker exec qrdine-api env | grep ConnectionString
docker exec qrdine-api env | grep REDIS

# Compare .env files
diff .env .env.example
```

### File System

```bash
# Check container user
docker exec qrdine-api id
# Expected: uid=1000(appuser) gid=1000(appuser)

# File permissions
docker exec qrdine-api ls -la /app
# Expected: appuser:appuser ownership

# Check working directory
docker exec qrdine-api pwd

# System info
docker exec qrdine-api cat /etc/os-release
```

---

## 🧹 Cleanup & Maintenance

### Remove Unused Resources

```bash
# Remove unused images
docker image prune

# Remove unused images (including dangling)
docker image prune -a

# Remove unused containers
docker container prune

# Remove unused volumes
docker volume prune

# Remove unused networks
docker network prune

# Full cleanup (warning: removes everything unused)
docker system prune -a --volumes
```

### Image Management

```bash
# List images
docker images

# List QRDine images
docker images | grep qrdine

# Image size details
docker images --format "table {{.Repository}}\t{{.Size}}"

# Remove image
docker rmi image_id

# Remove image (force)
docker rmi -f image_id

# Tag image
docker tag qrdine-api:latest myregistry.azurecr.io/qrdine-api:v1.0
```

### Volume Management

```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect qrdine_sqlserver_data

# Volume mount point
docker volume inspect -f '{{.Mountpoint}}' qrdine_sqlserver_data

# Backup volume
docker run --rm -v qrdine_sqlserver_data:/data -v $(pwd):/backup \
  alpine tar czf /backup/backup.tar.gz /data

# Delete volume
docker volume rm qrdine_sqlserver_data
```

---

## 🔄 Advanced Operations

### Port Management

```bash
# Find process using port 80 (Nginx, external)
# Windows
netstat -ano | findstr :80

# Linux/Mac
lsof -i :80

# Internal ports (8080 API, 1433 Database, 6379 Redis) are NOT exposed externally
# They exist only on qrdine-network overlay network

# View Docker port mappings
docker port qrdine-nginx
docker port qrdine-api
```

### Running Multiple Commands

```bash
# Execute multiple commands
docker exec qrdine-api /bin/bash -c "cd /app && dotnet --version && ls -la"

# Chain commands with &&
docker exec qrdine-api /bin/bash -c "cmd1 && cmd2 && cmd3"

# Pipe commands
docker exec qrdine-api /bin/bash -c "ps aux | grep dotnet"
```

### Monitoring Script

```bash
# Full system health check
echo "=== Services ===" && docker-compose ps && \
echo -e "\n=== Health ===" && \
docker inspect qrdine-api --format='API: {{.State.Health.Status}}' && \
echo "=== Resources ===" && docker stats --no-stream
```

---

## 📚 Useful Combinations

### Development Workflow

```bash
# Terminal 1: Watch logs
docker-compose logs -f api

# Terminal 2: Monitor resources
docker stats qrdine-api

# Terminal 3: Test API (every second)
watch -n 1 'curl -s http://localhost:5000/health | jq'
```

### Debugging Session

```bash
# Get shell
docker exec -it qrdine-api /bin/bash

# Inside container:
# View logs
tail -f /app/logs/*

# Check configuration
env | grep -i sql

# Test connectivity
nslookup redis
```

### Performance Analysis

```bash
# Before making changes
docker stats --no-stream > before.txt

# After making changes
docker stats --no-stream > after.txt

# Compare
diff before.txt after.txt
```

---

## 🆘 Troubleshooting Quick Reference

### Container Won't Start

```bash
# View error
docker-compose logs api

# Check image exists
docker images | grep qrdine-api

# Rebuild
docker-compose build --no-cache api
```

### Connection Issues

```bash
# Check network
docker network inspect qrdine-network

# Test DNS
docker exec qrdine-api nslookup redis

# Test port
docker exec qrdine-api telnet redis 6379
```

### Performance Issues

```bash
# Resource usage
docker stats

# Disk space
docker system df

# Memory pressure
docker exec qrdine-api free -m
```

---

## 📖 Help & Information

```bash
# Docker version
docker --version
docker-compose --version

# Docker help
docker --help
docker-compose --help

# System information
docker info

# Display logs with timestamps
docker-compose logs --timestamps

# Pretty-print JSON
docker inspect qrdine-api | jq
```

---

**Return to:** [Docker Documentation](README.md) | [Deployment Guide](deployment-guide.md)
