# BE Window Lamour — Claude Code Guide

> Backend REST API for the **Lamour** cosmetics business management system.
> Client: WPF desktop app (`desktop-lamour`). Stack: .NET 8, ASP.NET Core Web API, EF Core + PostgreSQL.

## Agent Routing

When a task matches a domain below, **spawn the appropriate agent** via the Agent tool before responding directly.

| Task type | Agent to invoke | Trigger keywords |
|---|---|---|
| Implement feature, fix bug, scaffold layers, wire UseCase | `lamour-be-expert` | implement, add feature, usecase, repository, controller, bug, crash, error, exception |
| Business rules, domain models, invoice logic, stock, VAT | `lamour-domain-expert` | business rule, domain, inventory, invoice, stock, employee, role, supplier, VAT, validate |
| Module navigation, architecture, file structure, DI context | `lamour-module-context-expert` | module, architecture, structure, folder, which layer, navigate code |

## Project Stack

- **Platform**: .NET 8, ASP.NET Core Web API
- **ORM**: EF Core 8 + PostgreSQL (Npgsql)
- **Auth**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **DI**: `Microsoft.Extensions.DependencyInjection` — constructor injection only
- **Tests**: xUnit + Moq

## Architecture (4 layers, strictly separated)

```
Lamour.Api           (Controllers, Middleware)
     ↕ interfaces
Lamour.Application   (UseCases, DTOs)
     ↕ interfaces
Lamour.Domain        (Entities, Enums, Exceptions — zero deps)
     ↕ interfaces
Lamour.Infrastructure (Repositories, EF Core, AppDbContext)
```

## Module Structure

```
src/
├── Lamour.Api/
│   ├── Controllers/[Feature]Controller.cs
│   ├── Middleware/GlobalExceptionHandler.cs
│   └── Program.cs
├── Lamour.Application/
│   └── Features/[Feature]/
│       ├── UseCases/I[Name]UseCase.cs + [Name]UseCase.cs
│       └── Dtos/[Name]RequestDto.cs + [Name]ResponseDto.cs
├── Lamour.Domain/
│   ├── Entities/[Name].cs
│   └── Exceptions/DomainException.cs
├── Lamour.Infrastructure/
│   ├── Persistence/AppDbContext.cs
│   ├── Persistence/Configurations/[Name]Configuration.cs
│   └── Repositories/[Name]Repository.cs
└── Lamour.Contracts/           # Shared DTOs (referenced by WPF client)
```

## Business Domains

- **Authentication** — phone-based sign up/login, JWT tokens
- **Employees** — staff profiles, roles (Admin / Cashier / Warehouse)
- **Inventory** — cosmetics products, stock levels, low-stock alerts
- **ImportInvoices** — purchase from suppliers → increases stock (NK-YYYYMMDD-NNN)
- **ExportInvoices** — sales to customers → decreases stock, VAT 10% (XK-YYYYMMDD-NNN)
- **Suppliers** — CRUD + duplicate; code is unique case-insensitive

## Mandatory Rules

1. All async public methods accept `CancellationToken ct = default`
2. Never `.Result` or `.Wait()` — always `await`
3. Constructor injection only — never `new XxxService()` or service locator
4. DTOs only cross layer boundaries — never return EF entities from API
5. `AsNoTracking()` on all read-only EF Core queries
6. All JSON fields use `[JsonPropertyName("snake_case")]` — WPF client expects snake_case
7. Confirmed invoices are immutable — only cancellation allowed
8. Stock never goes negative — validate before confirming export invoice
9. Use `ILogger<T>` — never `Console.WriteLine` or `Debug.Print`
10. Store `DateTime.UtcNow` — convert to local time in WPF client
