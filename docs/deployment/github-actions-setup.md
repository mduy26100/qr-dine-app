# GitHub Actions CI/CD Deployment

Setup GitHub Actions Workflow để tự động triển khai QRDine lên VPS Production khi push/merge vào nhánh `main`.

## 📋 Tổng quan

Workflow tự động:

1. Checkout code từ GitHub
2. SSH vào VPS
3. Pull code mới từ repository
4. Build Docker images mới
5. Deploy services (Zero-downtime)
6. Health check
7. Cleanup dangling resources

**Thời gian deploy:** ~60-120 giây

## 🔧 Bước 1: Thiết lập GitHub Secrets

Truy cập vào GitHub repository của bạn:

- Vào **Settings** > **Secrets and variables** > **Actions** > **New repository secret**

Thêm 3 secrets sau:

### `HOST_IP`

- **Mô tả**: Địa chỉ IP hoặc domain của VPS Production
- **Ví dụ**: `203.0.113.45` hoặc `api.qrdine.com`

### `USERNAME`

- **Mô tả**: Username để SSH vào VPS
- **Ví dụ**: `root` hoặc `deploy`

### `SSH_PRIVATE_KEY`

- **Mô tả**: Private SSH key (không mã hóa)
- **Cách lấy**:

  ```bash
  # Trên VPS, tạo SSH key (nếu chưa có)
  ssh-keygen -t rsa -b 4096 -f ~/.ssh/id_rsa -N ""

  # Copy nội dung private key
  cat ~/.ssh/id_rsa

  # Paste toàn bộ nội dung (bao gồm -----BEGIN RSA PRIVATE KEY----- và -----END RSA PRIVATE KEY-----)
  # vào GitHub Secret
  ```

## 🛠️ Bước 2: Cấu hình trên VPS

### 2.1 Tạo thư mục dự án

```bash
mkdir -p /root/qr-dine-app
cd /root/qr-dine-app
git init
git remote add origin https://github.com/YOUR_USERNAME/qr-dine-app.git
```

### 2.2 Tạo file `.env`

```bash
cat > .env << 'EOF'
# SQL Server
ACCEPT_EULA=Y
MSSQL_SA_PASSWORD=YourStrongPassword123!

# Redis
REDIS_PASSWORD=YourRedisPassword123!

# Application
ASPNETCORE_ENVIRONMENT=Production
MSSQL_CONNECTION_STRING=Server=sql-server;Database=QRDine;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=true;
REDIS_CONNECTION_STRING=redis://:YourRedisPassword123!@redis:6379

# JWT
JWT_SECRET=YourJWTSecretKey123456789012345
JWT_VALID_ISSUER=https://qrdine.com
JWT_VALID_AUDIENCE=qrdine-app
JWT_ACCESS_TOKEN_EXPIRY_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRY_DAYS=7

# Cloudinary
CLOUDINARY_CLOUD_NAME=your_cloudinary_cloud
CLOUDINARY_API_KEY=your_cloudinary_api_key
CLOUDINARY_API_SECRET=your_cloudinary_api_secret

# Frontend & CORS
FRONTEND_BASE_URL=https://qrdine.com
CORS_ALLOWED_ORIGIN_1=https://qrdine.com
CORS_ALLOWED_ORIGIN_2=https://www.qrdine.com
CORS_ALLOWED_ORIGIN_3=http://localhost:3000

# Security
SECURITY_TOKEN_HASH_SECRET=YourTokenHashSecret123456789

# Email
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_SENDER_EMAIL=your-email@gmail.com
EMAIL_SENDER_NAME=QRDine
EMAIL_SENDER_PASSWORD=your_app_password

# Payment
PAYOS_CLIENT_ID=your_payos_client_id
PAYOS_API_KEY=your_payos_api_key
PAYOS_CHECKSUM_KEY=your_payos_checksum_key

# Nginx
NGINX_HTTP_PORT=80
EOF
```

### 2.3 Cấu hình SSH Public Key

```bash
# Trên máy local, copy public key
cat ~/.ssh/id_rsa.pub

# Trên VPS, thêm vào authorized_keys
echo "YOUR_PUBLIC_KEY_CONTENT" >> ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys
chmod 700 ~/.ssh
```

### 2.4 Kiểm tra kết nối SSH

```bash
# Chạy từ máy local để test
ssh -i /path/to/private/key USERNAME@HOST_IP "echo 'SSH test successful!'"
```

## 📝 Bước 3: Cập nhật Workflow

File workflow nằm tại `.github/workflows/deploy.yml`

Đảm bảo đường dẫn VPS đúng (bỏ mục "🚀 Các điểm chỉnh sửa" nếu bạn dùng `/root/qr-dine-app`):

```yaml
cd /root/qr-dine-app
```

Nếu bạn dùng đường dẫn khác, hãy cập nhật tương ứng.

## 🚀 Bước 4: Deploy!

### Trigger tự động

1. Push code vào nhánh `main`:

   ```bash
   git push origin main
   ```

2. Hoặc merge PR vào `main`

Workflow sẽ tự động chạy.

### Theo dõi deployment

1. Vào GitHub repository
2. Click tab **Actions**
3. Xem workflow run hiện tại
4. Click vào run để xem chi tiết logs

## 📊 Workflow Flow

```
┌─────────────────────┐
│  Push/Merge to main │
└──────────┬──────────┘
           │
           v
┌─────────────────────────────────────────┐
│  GitHub Actions Workflow (ubuntu-latest)│
└──────────┬──────────────────────────────┘
           │
           v
┌─────────────────────────────────────────┐
│  Checkout code                          │
└──────────┬──────────────────────────────┘
           │
           v
┌─────────────────────────────────────────┐
│  SSH to VPS (appleboy/ssh-action)       │
│  - cd /root/qr-dine-app                 │
│  - git pull origin main                 │
│  - Load .env variables                  │
│  - docker compose up -d --build         │ (Zero-downtime)
│  - Health check & verify                │
│  - Cleanup dangling images              │
└──────────┬──────────────────────────────┘
           │
           v
┌─────────────────────────────────────────┐
│  ✅ Deployment Complete                 │
│  Services running (sql-server, redis,   │
│  api, nginx)                            │
└─────────────────────────────────────────┘
```

## 🐳 Docker Compose Structure

Workflow deploy sử dụng `docker-compose.yml` của bạn:

| Service        | Image                                        | Purpose                       |
| -------------- | -------------------------------------------- | ----------------------------- |
| **sql-server** | `mcr.microsoft.com/mssql/server:2022-latest` | Database (SQL Server 2022)    |
| **redis**      | `redis:7-alpine`                             | Cache & session storage       |
| **api**        | Built from `Dockerfile`                      | .NET 8 API Application        |
| **nginx**      | `nginx:1.27-alpine`                          | Reverse proxy & load balancer |

Tất cả kết nối qua network `qrdine-network`

### Build Process

Dockerfile sử dụng multi-stage build:

1. **Build stage**: SDK .NET 8 - compile code
2. **Publish stage**: Publish release binaries
3. **Runtime stage**: ASP.NET runtime + curl (health check)

Workflow chỉ build image `api`, các images khác được pull từ registry.

## 🔐 Deploy Strategy - Zero Downtime

**Khác biệt từ deploy cách:**

❌ **Cách xấu (Downtime):**

```bash
docker compose down                  # Tắt tất cả services - ❌ DOWNTIME
docker compose up -d --build         # Build & start lại
```

✅ **Cách tốt (Zero-downtime):**

```bash
docker compose up -d --build --remove-orphans  # Docker xử lý thông minh
```

Docker sẽ:

1. Build image mới ngầm
2. Khi image ready, chớp mắt replace container cũ
3. Hệ thống gần như khôn g có downtime

## 🧹 Cleanup Strategy

Workflow thực hiện cleanup an toàn:

```bash
docker image prune -f              # Xóa chỉ dangling images
                                   # Giữ lại base images (dotnet/sdk, nginx, redis)
docker system prune -f             # Xóa unused containers, networks
                                   # ❌ KHÔNG xóa volumes (--volumes)
```

❌ **Không dùng:**

```bash
docker system prune -f --volumes   # ❌ Xóa cả database volumes!!!
docker image prune -a              # ❌ Xóa tất cả images kể cả base
```

## 🐛 Troubleshooting

### SSH Connection Failed

```
Error: "Permission denied (publickey)"
```

**Giải pháp:**

1. Kiểm tra public key trên VPS:
   ```bash
   cat ~/.ssh/authorized_keys
   ```
2. Đảm bảo private key trong GitHub Secret chính xác
3. Kiểm tra SSH port (22) mở trên firewall:
   ```bash
   sudo ufw allow 22/tcp
   ```

### Docker Compose Not Found

```
Error: "command not found: docker compose"
```

**Giải pháp:**
SSH vào VPS cài Docker Compose:

```bash
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
docker compose version
```

### Health Check Failed

```
⚠️ API health check may need more time
```

**Giải pháp:**

1. Kiểm tra API logs:

   ```bash
   docker compose logs api -f
   ```

2. Kiểm tra database connection:

   ```bash
   docker compose exec api curl http://localhost:8080/health/live
   ```

3. Kiểm tra .env variables:
   ```bash
   cat .env
   ```

### Insufficient Disk Space

```
Error: "no space left on device"
```

**Giải pháp:**

```bash
# Kiểm tra disk space
df -h

# Cleanup cũ hơn
docker system prune -a -f
docker volume prune -f

# Xóa logs lớn
rm -f logs/*.log
```

### Port Already in Use

```
Error: "Bind failed: Address already in use"
```

**Giải pháp:**

```bash
# Kiểm tra port 80
sudo lsof -i :80

# Kill process nếu cần
sudo kill -9 PID

# Hoặc dừng container cũ
docker compose down
```

## ✅ Pre-Deploy Checklist

Trước khi deploy lần đầu:

- [ ] GitHub Secrets đã set: `HOST_IP`, `USERNAME`, `SSH_PRIVATE_KEY`
- [ ] VPS `/root/qr-dine-app` folder đã tạo
- [ ] VPS `.env` file đã tạo đúng
- [ ] SSH keys cấu hình xong
- [ ] `docker-compose.yml` syntax valid: `docker compose config`
- [ ] `.github/workflows/deploy.yml` đường dẫn đúng
- [ ] Repository git push origin main test

## 📞 Monitoring sau Deploy

### Logs theo dõi

```bash
# SSH vào VPS
ssh USERNAME@HOST_IP

# Kiểm tra containers
docker ps

# Kiểm tra API logs
docker compose logs api -f

# Kiểm tra Database logs
docker compose logs sql-server -f

# Kiểm tra Nginx logs
docker compose logs nginx -f
```

### Health Endpoints

```bash
# API health
curl http://localhost:8080/health/live

# Qua Nginx
curl http://localhost/health/live

# Database check
docker compose exec sql-server sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1"
```

## 🔄 Rollback nếu có issue

Nếu deployment có vấn đề:

```bash
# SSH vào VPS
ssh USERNAME@HOST_IP
cd /root/qr-dine-app

# Xem git log
git log --oneline -5

# Rollback to previous version
git revert HEAD
git push origin main

# Hoặc checkout specific commit
git checkout COMMIT_HASH
git push -f origin main

# Workflow sẽ tự động chạy để deploy version cũ
```

## 🔐 Security Best Practices

✅ **Nên làm:**

- Luôn dùng SSH keys (không password)
- Xoay SSH keys định kỳ
- Giữ `.env` file không commit vào git
- Restrict SSH access qua firewall
- Monitor GitHub Actions logs
- Backup database trước deploy lớn

❌ **Không nên làm:**

- Commit `.env` hoặc SSH keys
- Dùng SSH key có passphrase (không support)
- Share SSH private keys
- Hardcode credentials trong workflow
- Xóa volumes automatically
- Xóa tất cả images

## 📚 Xem thêm

- [Build and Deployment](build-and-deploy.md) - Hướng dẫn build
- [Troubleshooting](troubleshooting.md) - Giải quyết vấn đề
- [Configuration](../configuration/) - Environment setup
- [Docker Architecture](../docker/architecture.md) - Docker structure
