# GitHub Actions Workflows

## Contents

- **[deploy.yml](deploy.yml)** — Production deployment workflow
  - Triggers: Push/merge to `main` branch
  - Actions: Zero-downtime Docker deployment
  - Target: VPS with Docker Compose

## Quick Start

Before running workflows:

1. Set GitHub Secrets: `HOST_IP`, `USERNAME`, `SSH_PRIVATE_KEY`
2. Prepare VPS: Setup `/root/qr-dine-app` and `.env`
3. Deploy: `git push origin main`

## Documentation

For complete setup and deployment guide, see:
👉 **[GitHub Actions CI/CD Setup](../../docs/deployment/github-actions-setup.md)**

This includes:

- Step-by-step GitHub Secrets configuration
- VPS preparation guide
- Workflow explanation
- Troubleshooting
- Rollback procedures
