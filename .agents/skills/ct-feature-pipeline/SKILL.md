---
name: ct-feature-pipeline
description: End-to-end BE feature pipeline — single input (feature description + business rules) auto-runs 3 phases in sequence for ASP.NET Core: domain modeling → layer scaffolding → wiring and validation. Use when implementing a complete new business feature from scratch.
---

# BE Feature Pipeline — End-to-End

> Single input → 3 automated phases → complete working feature.

---

## Input Format

```
FEATURE: [Feature name, e.g. "Employees"]
DOMAIN: [Business description]
OPERATIONS: [e.g. CRUD + assign-role + deactivate]
BUSINESS_RULES: [e.g. Admin only, unique phone, role-based access]
ENDPOINT_PREFIX: [e.g. /api/v1/employees]
```

---

## Phase 1 — Domain Modeling

**Goal:** Define the domain entity, enums, and exceptions before any infrastructure.

### 1a. Identify Entity Fields
Analyze the business description and extract:
- Required fields (non-nullable)
- Optional fields (nullable)
- Enum fields (fixed value sets)
- Computed fields (calculated, not stored)

### 1b. Define Business Rules
Document all rules the UseCase must enforce:
- Uniqueness constraints (unique code/phone)
- Status guards (only Draft can be confirmed)
- Stock guards (quantity must not exceed available stock)
- Role checks (only Admin can perform this action)
- Immutability (confirmed records cannot be edited)

### 1c. Output — Domain Files

```
Lamour.Domain/
├── Entities/[Name].cs
└── Exceptions/[Name]Exception.cs  (if new exception type needed)
```

---

## Phase 2 — Layer Scaffolding

**Goal:** Generate all files using `ct-module` skill.

Invoke `ct-module` with:
```
moduleName: [Name]
operations: [from input]
```

Files generated:
```
Lamour.Infrastructure/
├── Persistence/Configurations/[Name]Configuration.cs
└── Repositories/[Name]Repository.cs

Lamour.Application/
└── Features/[Name]/
    ├── Dtos/[Name]Dtos.cs
    └── UseCases/
        ├── GetAll[Name]UseCase.cs
        ├── Create[Name]UseCase.cs
        ├── Update[Name]UseCase.cs
        └── Delete[Name]UseCase.cs

Lamour.Api/
├── Controllers/[Name]Controller.cs
└── [Name]ServiceCollectionExtensions.cs
```

For custom operations (beyond CRUD), invoke `ct-generate-usecase` for each.

---

## Phase 3 — Wiring, Validation & Tests

**Goal:** Connect all layers and verify correctness.

### 3a. AppDbContext Wiring
```csharp
// Add to AppDbContext.cs
public DbSet<[Name]> [Name]s { get; set; }
```

```csharp
// Add to OnModelCreating
modelBuilder.ApplyConfiguration(new [Name]Configuration());
```

### 3b. DI Wiring
```csharp
// Add to Program.cs
builder.Services.Add[Name]();
```

### 3c. EF Core Migration
```bash
dotnet ef migrations add Add[Name] \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api

dotnet ef database update \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

### 3d. Build Verification
```bash
dotnet build --no-restore /warnaserror
```

### 3e. Unit Tests (invoke `ct-unittest`)
Generate tests for:
- Happy path (valid input → expected DTO returned)
- Business rule violations (each rule → specific exception)
- Not found (non-existent ID → NotFoundException)

---

## Full Pipeline Checklist

**Phase 1 — Domain**
- [ ] Entity created with all fields
- [ ] Enum types defined (if any)
- [ ] Business rules documented

**Phase 2 — Scaffold**
- [ ] EF Configuration created
- [ ] Repository (interface + impl)
- [ ] DTOs (Request + Response)
- [ ] UseCases for all operations
- [ ] Controller with all actions
- [ ] DI extension method

**Phase 3 — Wire + Validate**
- [ ] `DbSet<T>` added to AppDbContext
- [ ] EF config applied in `OnModelCreating`
- [ ] DI extension called in `Program.cs`
- [ ] Migration created and applied
- [ ] `dotnet build` passes
- [ ] Unit tests written and passing
- [ ] Business rules tested (not just happy path)

---

## Business Domain Cheat Sheet

| Feature | Key Rules |
|---|---|
| Suppliers | Unique code (case-insensitive), IsStopTracking disables from orders |
| Employees | Unique phone, role-based access (Admin/Cashier/Warehouse) |
| Products | Unique SKU, StockQuantity never negative, low-stock alert |
| ImportInvoices | Draft→Confirmed (stock +), immutable after confirm, NK-YYYYMMDD-NNN numbering |
| ExportInvoices | Stock guard before confirm, VAT 10%, discount, XK-YYYYMMDD-NNN numbering |
