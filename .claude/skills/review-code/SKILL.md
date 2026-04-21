---
name: review-code
description: "C#/.NET ASP.NET Core code review for BE Window Lamour — Clean Architecture compliance, async patterns, EF Core usage, DTO discipline, business rule enforcement, security, and DI correctness. Use when asked to review any C# file."
argument-hint: "[file path or code to review] [focus area: Architecture | Async | EF Core | DTOs | Business Rules | Security | DI | Full Review]"
---

# ASP.NET Core Code Review Skill

> **Anti-Hallucination:** Verify every class name, interface, and path against the codebase before suggesting changes.

Full code review for C# files in **BE Window Lamour** — ASP.NET Core Web API.

---

## When to Use

Invoke when asked to:
- Review a Controller, UseCase, Repository, or Entity file
- Check Clean Architecture compliance
- Audit async patterns
- Verify EF Core usage
- Confirm business rule enforcement

---

## Focus Areas

| Area | What Is Checked |
|------|----------------|
| `Architecture` | Layer separation, no cross-layer leakage, interface usage |
| `Async` | `await` everywhere, no `.Result`/`.Wait()`, `CancellationToken` passed |
| `EF Core` | `AsNoTracking()` on reads, `Include()` for navigations, no N+1, migrations |
| `DTOs` | Entities never returned from API, snake_case `[JsonPropertyName]` |
| `Business Rules` | Stock guard, invoice immutability, role checks |
| `Security` | `[Authorize]` on protected routes, no sensitive data in responses |
| `DI` | Services registered with correct lifetime, constructor injection only |
| `Full Review` | All of the above combined |

---

## Key Rules (ALWAYS APPLY)

| ❌ Forbidden | ✅ Required |
|-------------|-------------|
| `.Result` / `.Wait()` | `await` all async calls |
| Missing `CancellationToken` | `CancellationToken ct = default` on all async methods |
| Returning EF entity from controller | Return DTO mapped from entity |
| Missing `AsNoTracking()` on reads | `.AsNoTracking()` on all read queries |
| `new XxxService()` / `new XxxRepository()` | Constructor injection via DI |
| Business logic in Controller | Business logic in UseCase only |
| Inline `Foreground` / DB config in code | `appsettings.json` / environment variables |
| Missing `[Authorize]` on protected routes | `[Authorize]` on all non-auth controllers |
| Stock mutation without guard | Validate `StockQuantity >= line.Quantity` first |
| Mutating confirmed invoice | Check `Status == Draft` before any mutation |

---

## Review Template

For each file reviewed, output:

```
## Review: [FileName.cs]

### Layer Compliance
- ✅/❌ [observation]

### Async Patterns
- ✅/❌ [observation]

### EF Core Usage
- ✅/❌ [observation]

### DTO Discipline
- ✅/❌ [observation]

### Business Rules
- ✅/❌ [observation]

### Security
- ✅/❌ [observation]

### DI
- ✅/❌ [observation]

### Summary
[2-3 sentence summary of biggest issues and recommended fixes]
```

---

## Common Violations

### 1. Controller Doing Business Logic

```csharp
// ❌ Bad — stock check in controller
[HttpPost("{id}/confirm")]
public async Task<IActionResult> Confirm(int id)
{
    var invoice = await _db.ExportInvoices.FindAsync(id);
    if (invoice.Lines.Sum(l => l.Quantity) > product.StockQuantity)
        return BadRequest("Insufficient stock");
    // ...
}

// ✅ Good — delegate to UseCase
[HttpPost("{id}/confirm")]
public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    => Ok(await _confirmUseCase.ExecuteAsync(id, ct));
```

### 2. Entity Returned from API

```csharp
// ❌ Bad — returns EF entity (circular refs, exposes internals)
public async Task<Supplier> GetByIdAsync(int id) =>
    await _db.Suppliers.FindAsync(id);

// ✅ Good — maps to DTO
public async Task<SupplierResponseDto> GetByIdAsync(int id, CancellationToken ct)
{
    var s = await _db.Suppliers.FindAsync(id, ct)
        ?? throw new NotFoundException(nameof(Supplier), id);
    return new SupplierResponseDto { Id = s.Id, Code = s.Code, Name = s.Name };
}
```

### 3. Missing AsNoTracking on Read

```csharp
// ❌ Bad — EF tracks all entities (wastes memory on read-only queries)
var suppliers = await _db.Suppliers.ToListAsync(ct);

// ✅ Good
var suppliers = await _db.Suppliers.AsNoTracking().ToListAsync(ct);
```

### 4. Missing CancellationToken

```csharp
// ❌ Bad
public async Task<IEnumerable<SupplierResponseDto>> GetAllAsync()

// ✅ Good
public async Task<IEnumerable<SupplierResponseDto>> GetAllAsync(CancellationToken ct = default)
```

### 5. Blocking Call

```csharp
// ❌ Bad — deadlocks under ASP.NET Core sync context
var result = _useCase.GetAllAsync().Result;

// ✅ Good
var result = await _useCase.GetAllAsync(ct);
```

### 6. Missing [Authorize]

```csharp
// ❌ Bad — endpoint is public
[ApiController]
[Route("api/v1/suppliers")]
public class SuppliersController : ControllerBase { }

// ✅ Good
[ApiController]
[Route("api/v1/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase { }
```

### 7. Snake_case DTO Missing JsonPropertyName

```csharp
// ❌ Bad — WPF client expects snake_case but gets PascalCase
public class SupplierResponseDto
{
    public string TaxCode { get; set; }
    public bool IsStopTracking { get; set; }
}

// ✅ Good
public class SupplierResponseDto
{
    [JsonPropertyName("tax_code")]        public string TaxCode { get; set; } = "";
    [JsonPropertyName("is_stop_tracking")] public bool IsStopTracking { get; set; }
}
```
