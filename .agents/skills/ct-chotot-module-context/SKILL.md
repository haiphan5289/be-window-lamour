---
name: ct-chotot-module-context
description: Quick reference for BE Window Lamour module architecture, Clean Architecture layer patterns, DI setup, and module-specific conventions. Use when navigating the codebase, understanding which layer a file belongs to, or setting up a new feature module.
---

# BE Module Context — Quick Reference

> Navigation guide for the BE Window Lamour ASP.NET Core codebase.

---

## Project Map

```
be-window-lamour/
├── src/
│   ├── Lamour.Api/                    ← HTTP layer (Controllers, Middleware, Program.cs)
│   ├── Lamour.Application/            ← Business orchestration (UseCases + DTOs)
│   ├── Lamour.Domain/                 ← Pure domain (Entities, Enums, Exceptions)
│   ├── Lamour.Infrastructure/         ← Data access (Repositories, EF Core, External services)
│   └── Lamour.Contracts/              ← Shared DTOs (referenced by WPF client)
└── tests/
    ├── Lamour.Application.Tests/      ← xUnit unit tests
    └── Lamour.Api.IntegrationTests/   ← HTTP integration tests
```

---

## Layer Decision Guide

| Question | Answer |
|---|---|
| "Where does a new entity go?" | `Lamour.Domain/Entities/` |
| "Where does a new DB table go?" | `Lamour.Infrastructure/Persistence/Configurations/` + `AppDbContext` |
| "Where does a new SQL query go?" | `Lamour.Infrastructure/Repositories/` |
| "Where does a business rule go?" | `Lamour.Application/Features/[Feature]/UseCases/` |
| "Where does a new HTTP endpoint go?" | `Lamour.Api/Controllers/` |
| "Where does the API response shape go?" | `Lamour.Application/Features/[Feature]/Dtos/` |
| "Where does JWT auth go?" | `Lamour.Api/Program.cs` (middleware) |
| "Where does DI registration go?" | `Lamour.Api/[Feature]ServiceCollectionExtensions.cs` |

---

## Feature Module Pattern

Each business feature follows this exact layout:

```
src/Lamour.Application/Features/[Feature]/
├── Dtos/
│   └── [Feature]Dtos.cs              ← All DTOs for this feature in one file
└── UseCases/
    ├── GetAll[Feature]UseCase.cs
    ├── GetById[Feature]UseCase.cs
    ├── Create[Feature]UseCase.cs
    ├── Update[Feature]UseCase.cs
    ├── Delete[Feature]UseCase.cs
    └── [Custom][Feature]UseCase.cs   ← e.g. ConfirmExportInvoiceUseCase

src/Lamour.Infrastructure/Repositories/
└── [Feature]Repository.cs            ← Interface + implementation in same file

src/Lamour.Api/Controllers/
└── [Feature]Controller.cs

src/Lamour.Api/
└── [Feature]ServiceCollectionExtensions.cs
```

---

## Current Modules

| Module | Status | Key Business Rule |
|---|---|---|
| Auth | ✅ | Phone-based JWT auth |
| Suppliers | ✅ (mock) | Unique code, duplicate action |
| Employees | ⏳ Planned | Role-based access |
| Inventory (Products) | ⏳ Planned | Stock never negative |
| ImportInvoices | ⏳ Planned | Stock +, NK-YYYYMMDD-NNN |
| ExportInvoices | ⏳ Planned | Stock guard, VAT 10%, XK-YYYYMMDD-NNN |

---

## DI Lifetime Rules

| Service Type | Lifetime | Why |
|---|---|---|
| `AppDbContext` | `Scoped` | One per HTTP request |
| `IXxxRepository` | `Scoped` | Uses DbContext |
| `IXxxUseCase` | `Scoped` | Uses Repository |
| `IHttpClientFactory` | `Singleton` | Managed by framework |
| Typed HttpClient services | `Transient` | HttpMessageHandler pooled by factory |

**Never** register `AppDbContext` as `Singleton` — causes threading issues.

---

## Key Interfaces

| Interface | Implementation | Layer |
|---|---|---|
| `ISupplierRepository` | `SupplierRepository` | Infrastructure |
| `IGetAllSuppliersUseCase` | `GetAllSuppliersUseCase` | Application |
| `IAuthenticationService` (ext) | `AuthenticationService` | Infrastructure (HttpClient) |

---

## Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Entity | `PascalCase` singular | `ExportInvoice` |
| Table | `snake_case` plural | `export_invoices` |
| Repository | `[Name]Repository` | `SupplierRepository` |
| UseCase | `[Verb][Name]UseCase` | `CreateSupplierUseCase` |
| Controller | `[Name]Controller` | `SuppliersController` |
| DTO | `[Operation][Name][Request|Response]Dto` | `CreateSupplierRequestDto` |
| JSON field | `snake_case` | `is_stop_tracking` |
| Route | `kebab-case` plural | `/api/v1/export-invoices` |
