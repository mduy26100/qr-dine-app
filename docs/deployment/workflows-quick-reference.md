# GitHub Workflows - Quick Reference

Nhanh chóng tìm kiếm công việc cần làm khi triển khai với GitHub Actions.

## 📋 Nội dung workflows

- **[deploy.yml](../../.github/workflows/deploy.yml)** — Production deployment workflow
  - Triggers: Push/merge to `main` branch
  - Actions: Zero-downtime Docker deployment
  - Target: VPS with Docker Compose

## 🚀 Quick Start

Trước khi chạy workflows:

1. **Set GitHub Secrets** trong repository settings:
   - `HOST_IP` — VPS IP hoặc domain
   - `USERNAME` — SSH username
   - `SSH_PRIVATE_KEY` — Private SSH key

2. **Prepare VPS**: Setup `/root/qr-dine-app` và `.env` file

3. **Deploy**: `git push origin main` — Workflow tự động chạy

## ✅ Setup Checklist

| Item                                                    | Status | Reference                                                                            |
| ------------------------------------------------------- | ------ | ------------------------------------------------------------------------------------ |
| GitHub Secrets set (HOST_IP, USERNAME, SSH_PRIVATE_KEY) | [ ]    | [GitHub Actions - Secrets](github-actions-setup.md#-bước-1-thiết-lập-github-secrets) |
| VPS folder created (`/root/qr-dine-app`)                | [ ]    | [GitHub Actions - VPS Folder](github-actions-setup.md#-bước-2-cấu-hình-trên-vps)     |
| `.env` file created on VPS                              | [ ]    | [GitHub Actions - .env File](github-actions-setup.md#22-tạo-file-env)                |
| SSH keys configured                                     | [ ]    | [GitHub Actions - SSH Keys](github-actions-setup.md#23-cấu-hình-ssh-public-key)      |
| SSH connection tested                                   | [ ]    | [GitHub Actions - SSH Test](github-actions-setup.md#24-kiểm-tra-kết-nối-ssh)         |
| `docker-compose.yml` syntax validated                   | [ ]    | Run: `docker compose config`                                                         |
| `.github/workflows/deploy.yml` path correct             | [ ]    | Verify: `cd /root/qr-dine-app`                                                       |

## 🔧 Common Tasks

| Task                       | How To                                                                       |
| -------------------------- | ---------------------------------------------------------------------------- |
| Monitor deployment         | GitHub repo → **Actions** tab → Click workflow run                           |
| View deployment logs       | Actions tab → Select run → Expand SSH step output                            |
| Troubleshoot SSH issues    | [Troubleshooting - SSH](github-actions-setup.md#ssh-connection-failed)       |
| Troubleshoot Docker issues | [Troubleshooting - Docker](github-actions-setup.md#docker-compose-not-found) |
| Troubleshoot Health Check  | [Troubleshooting - Health](github-actions-setup.md#health-check-failed)      |
| Check VPS disk space       | SSH to VPS: `df -h`                                                          |
| View API logs              | SSH to VPS: `docker compose logs api -f`                                     |
| Rollback deployment        | Push previous commit to `main`                                               |

## 🐳 Deployment Workflow Stages

```
Push/Merge to main
         ↓
   GitHub Actions
         ↓
    SSH to VPS
         ↓
    git pull origin main
         ↓
    source .env
         ↓
    docker compose up -d --build --remove-orphans
         ↓
    Wait 10 seconds
         ↓
    Health check
         ↓
    docker image prune -f
    docker system prune -f
         ↓
    Deployment Complete ✅
```

**Duration**: ~60-120 seconds

## 🔐 Security Reminders

- ✅ Never commit `.env` file
- ✅ Never commit SSH keys
- ✅ Use SSH key auth, not passwords
- ✅ Rotate SSH keys regularly
- ✅ Restrict GitHub Secrets access
- ✅ Monitor GitHub Actions logs for suspicious activity

## 📞 Need Help?

| Issue                   | Solution                                                                      |
| ----------------------- | ----------------------------------------------------------------------------- |
| Can't connect via SSH   | [SSH Troubleshooting](github-actions-setup.md#ssh-connection-failed)          |
| Docker not found on VPS | [Docker Troubleshooting](github-actions-setup.md#docker-compose-not-found)    |
| Health check fails      | [Health Check Troubleshooting](github-actions-setup.md#health-check-failed)   |
| Out of disk space       | [Disk Space Troubleshooting](github-actions-setup.md#insufficient-disk-space) |
| Want to rollback        | [Rollback Guide](github-actions-setup.md#-rollback-nếu-có-issue)              |
| Complete guide needed   | [GitHub Actions Setup (Full)](github-actions-setup.md)                        |

---

**Complete setup guide**: See [GitHub Actions CI/CD Setup](github-actions-setup.md)
