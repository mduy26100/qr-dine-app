# .github Directory

Contains GitHub-specific configuration and workflows.

## Folders

- **[workflows/](workflows/)** — GitHub Actions Workflows
  - `deploy.yml` — Production deployment to VPS

## Setup & Documentation

For comprehensive deployment guide:
👉 **[docs/deployment/github-actions-setup.md](../docs/deployment/github-actions-setup.md)**

This guide covers:

1. GitHub Secrets configuration
2. VPS preparation
3. Workflow deployment process
4. Troubleshooting
5. Security best practices
6. Rollback procedures

## Quick Reference

| Task                       | Reference                                                                                               |
| -------------------------- | ------------------------------------------------------------------------------------------------------- |
| Setup CI/CD for first time | [GitHub Actions Setup](../docs/deployment/github-actions-setup.md)                                      |
| Configure GitHub Secrets   | [GitHub Actions - Secrets](../docs/deployment/github-actions-setup.md#-bước-1-thiết-lập-github-secrets) |
| Prepare VPS                | [GitHub Actions - VPS Setup](../docs/deployment/github-actions-setup.md#-bước-2-cấu-hình-trên-vps)      |
| Understand deployment flow | [GitHub Actions - Workflow Flow](../docs/deployment/github-actions-setup.md#-workflow-flow)             |
| Troubleshoot issues        | [GitHub Actions - Troubleshooting](../docs/deployment/github-actions-setup.md#-troubleshooting)         |
| Rollback deployment        | [GitHub Actions - Rollback](../docs/deployment/github-actions-setup.md#-rollback-nếu-có-issue)          |

## Key Deployment Strategy

✅ **Zero-downtime deployment** using `docker compose up -d --build`
✅ **Safe cleanup** with `docker image prune -f` (keeps base images)
✅ **VPS based** deployment with SSH authentication
✅ **Automatic CI/CD** on push to `main` branch
