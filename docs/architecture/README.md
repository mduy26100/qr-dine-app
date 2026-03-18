# Architecture

System design, architectural patterns, and design decisions.

## Contents

- **[Architecture Overview](overview.md)** — Onion Architecture fundamentals:
  - Onion Architecture layers and dependencies
  - CQRS pattern implementation
  - Multi-tenancy strategy
  - Domain layer business rules

- **[Design Patterns & Implementation](patterns-and-design.md)** — Detailed patterns and techniques:
  - 10+ design patterns used in the system
  - Extensibility examples for adding features
  - CQRS handler patterns
  - Specification and validator patterns
  - Repository pattern implementation
  - Middleware execution order
  - Testing strategies

- **[Project Structure](project-structure.md)** — Complete folder organization and file locations

## Quick Links

- Want to understand the system design? → [Architecture Overview](overview.md)
- Want implementation details? → [Design Patterns & Implementation](patterns-and-design.md)
- Want to know the folder structure? → [Project Structure](project-structure.md)
- Want to understand CQRS pattern? → [Design Patterns - CQRS Handlers](patterns-and-design.md#application-layer-features)
- Want extensibility examples? → [Design Patterns - Extensibility](patterns-and-design.md#extensibility-points)

## Key Architecture Decisions

1. **Onion Architecture** — 5 concentric layers with inward dependency flow
2. **CQRS Pattern** — Separate read and write models via MediatR
3. **Global Query Filters** — Automatic multi-tenant data isolation at EF Core level
4. **Repository Pattern** — Generic repository with specifications
5. **Specification Pattern** — Reusable, testable query specifications
6. **Pipeline Behaviors** — Cross-cutting concerns via MediatR
7. **Response Envelope** — Unified response format for all APIs
8. **Soft Deletes** — Logical deletion via IsDeleted flag

See [Architecture Overview](overview.md) for core concepts and [Design Patterns & Implementation](patterns-and-design.md) for detailed patterns.

---

**Reference:** See also [Project Structure](project-structure.md) for folder organization and [Development Guidelines](../development/) for development workflows.
