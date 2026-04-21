---
name: ct-bugfix-skill
description: Debug and fix C#/.NET ASP.NET Core bugs in BE Window Lamour with precision. Use when encountering 500 errors, EF Core query failures, DI resolution errors, async deadlocks, JWT auth issues, or unexpected business rule violations. Identifies root causes by verifying Clean Architecture data flow, checking EF Core context lifetime, validating async patterns, and confirming DI registration.
model: sonnet
effort: high
---

# .NET ASP.NET Core Bug Fix Skill

> **Anti-Hallucination:** Verify every class name, interface, method, and file path against the codebase before suggesting a fix.

## When to Use This Skill

- 500 Internal Server Error / unhandled exception
- EF Core query fails or returns wrong data
- DI resolution exception (`InvalidOperationException: Unable to resolve service`)
- Async deadlock or `.Result`/`.Wait()` usage
- JWT authentication failing (401/403)
- Business rule not enforced (stock going negative, confirmed invoice being edited)
- Migration fails or DB schema mismatch

## Core Debugging Workflow

### Step 1: Limit Scope (Read 3–4 Files Max)

Only read files directly related to the failing path. Never explore broadly.

**Good scope:**
- Controller that returns the error
- UseCase that processes the request
- Repository that queries the DB

**Avoid:**
- Reading entire project
- Exploring unrelated modules

### Step 2: Identify Root Cause

State the root cause clearly. Ask:

**For 500 errors:**
- Is there an unhandled exception? Check GlobalExceptionHandler.
- Is a `null` reference being dereferenced? Check for nullable annotations.
- Is the DB returning no rows when rows are expected? Check EF query conditions.

**For DI errors:**
- Is the service registered? Check `[Feature]ServiceCollectionExtensions.cs`.
- Is the lifetime correct? (`Scoped` services cannot be injected into `Singleton`.)
- Is the interface correctly mapped to the implementation?

**For EF Core bugs:**
- Is `AsNoTracking()` missing on read queries causing unexpected tracking?
- Is `SaveChangesAsync()` called after mutations?
- Is the `DbContext` lifetime correct? (`Scoped` per request — never `Singleton`.)
- Is there a missing `Include()` for navigations accessed after the query?
- Are migrations applied? (`dotnet ef database update`)

**For async bugs:**
- Is `.Result` or `.Wait()` used anywhere? → Replace with `await`.
- Is `CancellationToken` propagated through all layers?
- Are multiple `await` calls on the same DbContext (not thread-safe) happening concurrently?

**For JWT/auth bugs:**
- Is the endpoint decorated with `[Authorize]`?
- Is the token being sent as `Authorization: Bearer {token}`?
- Is the JWT secret/issuer/audience configured correctly in `appsettings.json`?
- Has the token expired?

**For business rule violations:**
- Is the stock guard implemented in the UseCase, not the Controller?
- Is invoice immutability checked before any mutation?

### Step 3: Apply Minimal Fix

Only fix the root cause. Do not refactor surrounding code.

**Good fixes:**
- Register missing service in DI extension
- Add missing `await` / remove `.Result`
- Add `Include()` for missing navigation property
- Add null check with `?? throw new NotFoundException(...)`
- Add stock validation in UseCase before confirming export

**Avoid:**
- Rewriting the entire UseCase or Controller
- Restructuring project layout
- "Cleanup" of unrelated code

### Step 4: Verify the Full Path

Trace the fix end-to-end:

```
HTTP Request
  → Controller (input validation → dispatch)
  → UseCase (business rules)
  → Repository (EF Core query)
  → DB
  ← Repository (maps Entity → DTO)
  ← UseCase (returns DTO)
  ← Controller (200 OK with DTO)
```

Each arrow: is data flowing? Are nulls handled? Are errors propagated correctly?

### Step 5: Verify the Fix

- Run `dotnet build` — no warnings/errors
- Run relevant xUnit tests: `dotnet test`
- Test the HTTP endpoint with correct request body

## Common Patterns & Solutions

### Pattern 1: DI Resolution Failure

**Symptom:** `InvalidOperationException: Unable to resolve service for type 'IXxxRepository'`

**Check:**
```csharp
// Good — registered in DI extension
services.AddScoped<IXxxRepository, XxxRepository>();

// Bad — missing registration
// → DI container cannot inject IXxxRepository
```

**Fix:** Add `services.AddScoped<IXxxRepository, XxxRepository>()` in the feature's `ServiceCollectionExtensions` and call it from `Program.cs`.

---

### Pattern 2: Async Deadlock

**Symptom:** Request hangs indefinitely, no response

**Check:**
```csharp
// Bad — blocks thread, causes deadlock in ASP.NET Core context
var result = _repository.GetAllAsync().Result;
_repository.DeleteAsync(id).Wait();

// Good
var result = await _repository.GetAllAsync(ct);
await _repository.DeleteAsync(id, ct);
```

**Fix:** Replace all `.Result` and `.Wait()` with `await`. Add `CancellationToken ct = default` to method signatures.

---

### Pattern 3: EF Core — Lazy Load After Context Disposal

**Symptom:** `InvalidOperationException: Cannot access a disposed context`

**Check:**
```csharp
// Bad — context disposed before navigation accessed
var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id, ct);
// context disposed here ↑
var lines = invoice.Lines; // ← fails, navigation not loaded

// Good — eager load with Include
var invoice = await _db.Invoices
    .Include(i => i.Lines)
    .FirstOrDefaultAsync(i => i.Id == id, ct);
```

**Fix:** Add `.Include()` for all navigation properties accessed after the query.

---

### Pattern 4: Returning Entity Instead of DTO

**Symptom:** Response contains EF Core navigation cycles, serialization error, or exposes internal fields

**Check:**
```csharp
// Bad — exposes entity directly
public async Task<Supplier> GetByIdAsync(int id, CancellationToken ct)
    => await _db.Suppliers.FindAsync(id, ct);

// Good — maps to DTO
public async Task<SupplierResponseDto> GetByIdAsync(int id, CancellationToken ct)
{
    var s = await _db.Suppliers.FindAsync(id, ct)
        ?? throw new NotFoundException(nameof(Supplier), id);
    return new SupplierResponseDto { Id = s.Id, Code = s.Code, /* ... */ };
}
```

**Fix:** Always map entities to DTOs at the repository boundary. Never return EF entities from UseCases or Controllers.

---

### Pattern 5: Stock Going Negative

**Symptom:** Export invoice confirms but product stock becomes negative

**Check:**
```csharp
// Bad — no stock validation
invoice.Status = InvoiceStatus.Confirmed;
product.StockQuantity -= line.Quantity;

// Good — validate first
if (product.StockQuantity < line.Quantity)
    throw new InsufficientStockException(product.Name, product.StockQuantity, line.Quantity);

product.StockQuantity -= line.Quantity;
invoice.Status = InvoiceStatus.Confirmed;
```

**Fix:** Add stock guard in `ConfirmExportInvoiceUseCase` before any stock mutation.

---

### Pattern 6: Mutating a Confirmed Invoice

**Symptom:** Confirmed invoice data changes after confirmation

**Check:**
```csharp
// Bad — no status check before mutation
invoice.Lines.Add(newLine);
await _db.SaveChangesAsync(ct);

// Good — immutability enforced
if (invoice.Status != InvoiceStatus.Draft)
    throw new DomainException("Only draft invoices can be modified.");
invoice.Lines.Add(newLine);
```

**Fix:** Add status guard at the start of every mutation UseCase.

---

## Debugging Checklist

Before marking a fix complete:

- [ ] Read only 3–4 relevant files
- [ ] Root cause stated clearly (one sentence)
- [ ] Fix is minimal (only root cause addressed)
- [ ] Full request path traced
- [ ] `dotnet build` passes
- [ ] Relevant tests pass
- [ ] No `.Result` or `.Wait()` introduced
- [ ] Stock guard present for export invoice confirm
- [ ] Immutability guard present for confirmed invoice mutations
- [ ] DTOs returned (not entities)
- [ ] `CancellationToken` propagated
