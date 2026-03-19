# QRDine Documentation

Complete technical documentation for the QRDine multi-tenant SaaS backend.

## 📚 Documentation Structure

### Getting Started

- **[Getting Started Guide](development/getting-started.md)** — 5-step setup for local development

### Core Architecture & Design

- **[Architecture Overview](architecture/)** — System design, layers, CQRS pattern
- **[Project Structure](architecture/project-structure.md)** — Complete folder organization
- **[Database & Multi-Tenancy](database/)** — Schema design, data isolation
- **[Security](security/)** — Authentication, authorization, data protection

### Development

- **[Development Guidelines](development/)** — CQRS patterns, coding standards, testing
- **[Configuration](configuration/)** — Environment variables, secrets management
- **[API Reference](api/)** — API versioning, response envelope, endpoints

### Deployment & Operations

- **[Docker & Containerization](docker/)** — Production Docker setup, orchestration, deployment
- **[Build & Deploy](deployment/)** — Azure, Docker, CI/CD pipelines
- **[Troubleshooting](deployment/troubleshooting.md)** — 30+ common issues and solutions
- **[External Services](external-services/)** — Cloudinary, third-party integrations

### Features & Modules

- **[Features Overview](features/)** — All domain modules and their use cases
  - [Catalog Module](features/catalog/) — Categories, Products, Tables
  - [Identity Module](features/identity/) — Authentication, Roles
  - [Sales Module](features/sales/) — Orders, Real-time tracking
  - [Billing Module](features/billing/) — Plans, Subscriptions, Feature limits
  - [Tenant Module](features/tenant/) — Multi-tenancy management
  - [Staffs Module](features/staffs/) — Staff management

---

## 🎯 Quick Navigation

| Need                        | Link                                              |
| --------------------------- | ------------------------------------------------- |
| First time setup?           | [Getting Started](development/getting-started.md) |
| Understanding architecture? | [Architecture Overview](architecture/)            |
| API usage?                  | [API Reference](api/)                             |
| Setting up dev environment? | [Development Setup](development/)                 |
| Using Docker?               | [Docker & Containerization](docker/)              |
| Deploying to production?    | [Build & Deploy](deployment/)                     |
| Having issues?              | [Troubleshooting](deployment/troubleshooting.md)  |
| Understanding a feature?    | [Features Overview](features/)                    |
| Database structure?         | [Database Schema](database/)                      |

---

## 📦 What's Inside

### Projects

- **QRDine.API** (Presentation Layer) — REST endpoints, controllers, middleware
- **QRDine.Application** (Application Layer) — CQRS handlers, business logic
- **QRDine.Application.Common** (Shared Abstractions) — Interfaces, exceptions
- **QRDine.Domain** (Domain Layer) — Entities, domain rules
- **QRDine.Infrastructure** (Infrastructure Layer) — EF Core, Identity, services

### Technologies

- .NET 8, ASP.NET Core, Entity Framework Core 8
- SQL Server, Redis, SignalR
- MediatR (CQRS), FluentValidation, AutoMapper
- JWT Authentication, ASP.NET Core Identity
- Cloudinary (image uploads), MailKit (email)

### Modules

- **Catalog** — Menu and table management
- **Sales** — Order management with real-time tracking
- **Identity** — User authentication and authorization
- **Billing** — Subscription plans and feature limits
- **Tenant** — Multi-tenancy and merchant data
- **Staffs** — Staff member management

---

## 🔍 Document Index

| Document          | Purpose                     | Audience               |
| ----------------- | --------------------------- | ---------------------- |
| Getting Started   | Local setup steps           | Developers             |
| Architecture      | System design & patterns    | Architects, Developers |
| Project Structure | Folder organization         | All                    |
| Development       | Coding standards & patterns | Developers             |
| Database          | Schema & multi-tenancy      | DBAs, Developers       |
| API Reference     | Endpoint documentation      | Backend, Frontend      |
| Security          | Auth, RBAC, data isolation  | Architects, DevOps     |
| Configuration     | Environment setup           | DevOps, Developers     |
| Deployment        | Build, deploy, CI/CD        | DevOps, Developers     |
| Troubleshooting   | Common issues               | All                    |
| Features          | Module overview             | Product, Developers    |

**Reference:**

- [Getting Started](development/getting-started.md)
- [Architecture Overview](architecture/overview.md)
- [Design Patterns](architecture/patterns-and-design.md)
- [Troubleshooting](deployment/troubleshooting.md)

---

## 📝 How to Use This Documentation

1. **New to the project?** Start with [Getting Started](getting-started.md)
2. **Understanding the codebase?** Read [Architecture Overview](architecture/) first
3. **Building features?** Check [Features Overview](features/) and [Development Guidelines](development/)
4. **Deploying?** Follow [Build & Deploy](deployment/)
5. **Having issues?** Check [Troubleshooting](troubleshooting.md)

---

## 🛠️ For Specific Tasks

### I want to...

**Set up development environment** → [Getting Started](getting-started.md)

**Add a new feature** → [Features Overview](features/) + [Development Guidelines](development/)

**Deploy to production** → [Build & Deploy](deployment/)

**Create a new API endpoint** → [API Reference](api/) + [Development Guidelines](development/)

**Fix authentication issues** → [Security](security/) + [Troubleshooting](troubleshooting.md)

**Understand data isolation** → [Database & Multi-Tenancy](database/) + [Security](security/)

**Configure the application** → [Configuration](configuration/) + [Getting Started](getting-started.md)

**Add external service integration** → [External Services](external-services/)

---

## 📞 Need Help?

1. Check [Troubleshooting](troubleshooting.md) first
2. Review [Development Guidelines](development/)
3. Check relevant feature module documentation
4. Review API documentation if endpoint-related

---

**Last Updated:** March 2026  
**Maintained by:** Development Team
