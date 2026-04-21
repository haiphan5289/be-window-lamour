---
name: ct-ai-document
description: Generate structured feature documentation for BE ASP.NET Core features — API contract, business rules, data model, layer responsibilities, and test strategy. Use when documenting a completed or in-progress backend feature for the team or WPF client developers.
---

# BE Feature Documentation Generator

> Generates structured `.md` documentation for a BE feature covering API contract, domain model, business rules, and test strategy.

---

## Input Format

```
FEATURE: [Feature name, e.g. "ExportInvoices"]
STATUS: [Draft | In Progress | Complete]
SOURCE: [file paths, git diff, or feature description]
```

---

## Output Structure

```markdown
# [Feature Name] — BE API Documentation

**Status:** [Draft | In Progress | Complete]
**Module:** `Lamour.Application/Features/[Feature]`
**Last Updated:** [date]

---

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/v1/[feature]` | Bearer | List all |
| POST | `/api/v1/[feature]` | Bearer | Create |
| PUT | `/api/v1/[feature]/{id}` | Bearer | Update |
| DELETE | `/api/v1/[feature]/{id}` | Bearer | Delete |
| POST | `/api/v1/[feature]/{id}/confirm` | Bearer | Confirm |

---

## Data Model

### [Feature] Entity (`Lamour.Domain/Entities/`)
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| Id | int | No | Primary key |
| Code | string | No | Unique identifier |
| Name | string | No | Display name |
| Status | InvoiceStatus | No | Draft/Confirmed/Cancelled |

### Request DTOs
| DTO | Fields |
|-----|--------|
| `Create[Feature]RequestDto` | code, name, ... |
| `Update[Feature]RequestDto` | code, name, ... |

### Response DTO
| Field | JSON Key | Type |
|-------|----------|------|
| Id | `id` | int |
| Code | `code` | string |
| Name | `name` | string |

---

## Business Rules

| Rule | Where Enforced | Exception Thrown |
|------|---------------|-----------------|
| Code must be unique | `Create[Feature]UseCase` | `DomainException` |
| Only Draft can be confirmed | `ConfirmUseCase` | `DomainException` |
| Stock guard | `ConfirmExportUseCase` | `InsufficientStockException` |

---

## Layer Responsibilities

| Layer | File | Responsibility |
|-------|------|---------------|
| Controller | `[Feature]Controller.cs` | HTTP dispatch, [Authorize] |
| UseCase | `Create[Feature]UseCase.cs` | Business validation, orchestration |
| Repository | `[Feature]Repository.cs` | EF Core queries, DTO mapping |
| Entity | `[Feature].cs` | Domain data, no logic |

---

## Test Coverage

| Test | Type | File |
|------|------|------|
| Valid create returns DTO | Unit | `Create[Feature]UseCaseTests.cs` |
| Duplicate code throws | Unit | `Create[Feature]UseCaseTests.cs` |
| Not found throws 404 | Unit | `Delete[Feature]UseCaseTests.cs` |

---

## Integration Notes

**WPF Client:** Calls this endpoint via `[Feature]Service.cs` in `desktop-lamour`.
Expected JSON format: snake_case (`is_stop_tracking`, not `IsStopTracking`).

**Dependencies:**
- Requires `AppDbContext.DbSet<[Feature]>`
- Requires EF migration `Add[Feature]`
```

---

## Inline Code Documentation Convention

For complex business logic in UseCases:

```csharp
// Enforce: stock must not go negative before export invoice is confirmed.
// Each product line is validated independently; first violation throws immediately.
foreach (var line in invoice.Lines)
{
    if (product.StockQuantity < line.Quantity)
        throw new InsufficientStockException(product.Name, product.StockQuantity, line.Quantity);
}
```
