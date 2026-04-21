---
name: ct-handle-usecase
description: Add a UseCase execution to an existing Controller following Clean Architecture. Generates the controller action method, injects the UseCase via constructor, and registers it in DI. Use when wiring an already-created UseCase into its Controller.
argument-hint: "useCaseName:[Name] controllerName:[Name] httpMethod:[GET|POST|PUT|DELETE] route:[/path]"
---

# BE Handle UseCase — Wire UseCase into Controller

> **Anti-Hallucination:** Read the existing Controller constructor and DI extension before adding injection.

Wires an **existing UseCase** into an existing Controller action.

---

## Steps

### Step 1 — Inject UseCase into Controller Constructor

```csharp
// Before — existing constructor
public [Feature]Controller(IGetAll[Feature]UseCase getAll, ICreate[Feature]UseCase create)
{
    _getAll = getAll;
    _create = create;
}

// After — add new UseCase
private readonly I[NewUseCase]UseCase _[newUseCase];

public [Feature]Controller(
    IGetAll[Feature]UseCase getAll,
    ICreate[Feature]UseCase create,
    I[NewUseCase]UseCase [newUseCase])       // ← added
{
    _getAll = getAll;
    _create = create;
    _[newUseCase] = [newUseCase];            // ← added
}
```

### Step 2 — Add Controller Action

```csharp
// GET list
[HttpGet]
public async Task<IActionResult> GetAll(CancellationToken ct)
    => Ok(await _[useCase].ExecuteAsync(ct));

// GET by id
[HttpGet("{id:int}")]
public async Task<IActionResult> GetById(int id, CancellationToken ct)
{
    var result = await _[useCase].ExecuteAsync(id, ct);
    return result is null ? NotFound() : Ok(result);
}

// POST create
[HttpPost]
public async Task<IActionResult> Create([FromBody] Create[Feature]RequestDto dto, CancellationToken ct)
{
    var result = await _[useCase].ExecuteAsync(dto, ct);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}

// PUT update
[HttpPut("{id:int}")]
public async Task<IActionResult> Update(
    int id, [FromBody] Update[Feature]RequestDto dto, CancellationToken ct)
    => Ok(await _[useCase].ExecuteAsync(id, dto, ct));

// DELETE
[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id, CancellationToken ct)
{
    await _[useCase].ExecuteAsync(id, ct);
    return NoContent();
}

// Custom action (e.g., confirm, duplicate, cancel)
[HttpPost("{id:int}/[action-name]")]
public async Task<IActionResult> [ActionName](int id, CancellationToken ct)
    => Ok(await _[useCase].ExecuteAsync(id, ct));
```

### Step 3 — Register UseCase in DI Extension

```csharp
// In [Feature]ServiceCollectionExtensions.cs
// Add to existing Add[Feature] method:
services.AddScoped<I[NewUseCase]UseCase, [NewUseCase]UseCase>();
```

---

## Rules

- UseCase interfaces are injected via constructor, not `[FromServices]` (except one-off ad-hoc usages)
- Controller action only validates HTTP concerns (route params, body presence) — no business logic
- All actions accept `CancellationToken ct` as last parameter
- Return types: `Ok(result)` / `CreatedAtAction(...)` / `NoContent()` / `NotFound()`
- Domain/business exceptions are caught by `GlobalExceptionHandler` — no try/catch in Controller
