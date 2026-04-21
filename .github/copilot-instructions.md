# BE Window Lamour — AI Agent Guide

Backend REST API for the **Lamour** cosmetics business management system.
Client: WPF desktop app (`desktop-lamour`). Stack: .NET 8, ASP.NET Core Web API.

## Commands

### Build & Run

```bash
dotnet build                                          # Build all projects
dotnet run --project src/Lamour.Api                  # Start API (http://localhost:5000)
dotnet watch --project src/Lamour.Api                # Hot reload dev server
```

### Testing

```bash
dotnet test                                           # Run all tests
dotnet test --filter "Category=Unit"                  # Unit tests only
dotnet test --filter "Category=Integration"           # Integration tests only
```

### Database Migrations (EF Core)

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api

dotnet ef database update \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

### Code Quality

```bash
dotnet format                                         # Auto-format code
dotnet build /warnaserror                             # Treat warnings as errors
```

## Architecture Summary

**Clean Architecture** — 4-layer strict separation:

```
Lamour.Api           (Controllers, Middleware)
     ↕ interfaces
Lamour.Application   (UseCases, DTOs)
     ↕ interfaces
Lamour.Domain        (Entities, Enums, Exceptions — zero deps)
     ↕ interfaces
Lamour.Infrastructure (Repositories, EF Core, AppDbContext)
```

**Data flow per request:**
```
HTTP Request → Controller → UseCase → IRepository → EF Core → PostgreSQL
HTTP Response ← Controller ← UseCase ← IRepository ← Mapped DTO
```

## Core Rules

1. **No `.Result` or `.Wait()`** — always `await`
2. **`CancellationToken ct = default`** on all async public methods
3. **Constructor injection only** — no service locator, no `new XxxService()`
4. **DTOs, never entities** — map at repository boundary, use `[JsonPropertyName("snake_case")]`
5. **`AsNoTracking()`** on all read-only EF Core queries
6. **Business logic in UseCase** — Controllers only validate input and dispatch
7. **Stock guard before export confirm** — throw `InsufficientStockException` if stock < quantity
8. **Invoice immutability** — check `Status == Draft` before any mutation; confirmed = cancel only

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

## API Endpoints Contract

All routes prefixed `/api/v1/`. Authenticated routes require `Authorization: Bearer {token}`.

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/auth/check-phone` | None | Verify phone exists |
| POST | `/auth/register` | None | Register + return JWT |
| POST | `/auth/login` | None | Login + return JWT |
| GET | `/suppliers` | Bearer | List all suppliers |
| POST | `/suppliers` | Bearer | Create supplier |
| PUT | `/suppliers/{id}` | Bearer | Update supplier |
| DELETE | `/suppliers/{id}` | Bearer | Delete supplier |
| POST | `/suppliers/{id}/duplicate` | Bearer | Clone supplier |

## Business Domains

- **Authentication** — phone-based sign up/login, JWT tokens
- **Employees** — staff profiles, roles (Admin / Cashier / Warehouse)
- **Inventory** — cosmetics products, stock levels, low-stock alerts
- **ImportInvoices** — purchase from suppliers → increases stock (NK-YYYYMMDD-NNN)
- **ExportInvoices** — sales to customers → decreases stock, VAT 10% (XK-YYYYMMDD-NNN)
- **Suppliers** — CRUD + duplicate; code is unique case-insensitive

## Adding a New Feature

1. Add entity to `Lamour.Domain/Entities/`
2. Add EF configuration to `Lamour.Infrastructure/Persistence/Configurations/`
3. Add `DbSet<T>` to `AppDbContext`
4. Add `IRepository` + `Repository` to `Lamour.Infrastructure/Repositories/`
5. Add `IUseCase` + `UseCase` + DTOs to `Lamour.Application/Features/[Feature]/`
6. Add `Controller` to `Lamour.Api/Controllers/`
7. Add `[Feature]ServiceCollectionExtensions.cs`, wire in `Program.cs`
8. Run `dotnet ef migrations add Add[Feature]`
9. Add xUnit unit tests + integration tests

## IMPORTANT NOTES

- Never use `print()` — use `ILogger<T>` injected via constructor
- Never hardcode connection strings — use `appsettings.json` / environment variables
- All JSON responses use snake_case — WPF client expects `is_stop_tracking`, not `IsStopTracking`
- Current date/time must use `DateTime.UtcNow` — store UTC in DB, display local time in client
- Invoice numbers auto-generated server-side: `NK-{yyyyMMdd}-{seq:D3}` / `XK-{yyyyMMdd}-{seq:D3}`
