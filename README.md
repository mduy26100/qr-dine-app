# QR Dine App

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)
![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis)
![License](https://img.shields.io/badge/License-Private-red)

**QR Dine API** is a modern **multi-tenant SaaS backend** built to power QR-based digital menu systems for restaurants, cafés, and retail stores.

The system allows multiple businesses to **register, manage their profiles, and generate unique QR codes** that link to fully customizable online menus. Each store operates independently within a secure multi-tenant architecture, ensuring data isolation and scalability.

Through a RESTful API, the platform supports:

    - 📱 Digital QR menu management
    - 🛠 Menu and category CRUD operations
    - 🏪 Tenant-scoped store management
    - 📊 Basic management and reporting capabilities
    - 🚀 Scalable architecture for growing businesses

The backend is built using **.NET 8 Web API with Onion Architecture**, focusing on maintainability, separation of concerns, and production-ready design.

QR Dine API provides a **scalable and extensible SaaS backend foundation** that can evolve into a comprehensive digital restaurant management system.

---

## 🚀 Tech Stack & Architecture

| Project | Responsibility | Description |
|----------|---------------|-------------|
| **QRDine.API** | Presentation Layer | Exposes RESTful endpoints, API versioning, Swagger documentation, request/response handling |
| **QRDine.Application** | Application Layer | CQRS implementation with MediatR, business use cases, validation, DTOs |
| **QRDine.Application.Common** | Shared Abstractions | Common interfaces, base abstractions, cross-cutting concerns |
| **QRDine.Domain** | Domain Layer | Core entities, value objects, business rules |
| **QRDine.Infrastructure** | Infrastructure Layer | EF Core persistence, Identity, repository implementations, external services |

---

## 🧩 Core Features (MVP)

- Merchant registration & authentication (JWT-based)
- Tenant-scoped store management
- QR code generation per tenant
- Category & menu item management (CRUD APIs)
- Public menu retrieval endpoint (QR access)
- Role-based authorization (Admin / Merchant)
- Basic management & reporting endpoints

---

## 🏗 Project Structure

```txt
qr-dine-app/
├── 📁 src/
├── 📁 docs/
├── 📄 README.md        # This file
```
---

## 📌 Project Status

Initial project setup in progress.

---

## 👤 Author

**Do Manh Duy (Mark)**  
Full-stack Developer (.NET & React)

- 📧 Email: manhduy261000@gmail.com
- 🐙 GitHub: [github.com/mduy26100](https://github.com/mduy26100)
- 💼 LinkedIn: [linkedin.com/in/duy-do-manh-1a44b42a4](https://www.linkedin.com/in/duy-do-manh-1a44b42a4/)
- 📘 Facebook: [facebook.com/zuynuxi](https://www.facebook.com/zuynuxi/)

---

## 📄 License

This project is licensed under the MIT License — feel free to use, modify, and distribute it in accordance with the license terms.

---

<div align="center">

**⭐ If you find this project helpful, please give it a star! ⭐**

</div>