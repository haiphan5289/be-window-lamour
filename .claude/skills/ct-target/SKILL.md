---
name: ct-target
description: Generate a BE Controller action (endpoint target) — the HTTP entry point for a specific operation. Adapts to standard CRUD actions or custom business actions (confirm, duplicate, cancel). Use when adding a single new endpoint to an existing Controller.
argument-hint: "actionName:[Name] httpMethod:[GET|POST|PUT|DELETE|PATCH] route:[/path] inputType:[Dto|int|void] outputType:[Dto|void]"
---

# BE Endpoint Target Generator — Controller Action

> Maps the iOS concept of "API Target" (Requestable) to the BE equivalent: a **Controller action method** that defines the HTTP contract.

Generates a single **Controller action** for the BE Window Lamour API.

---

## Action Templates by HTTP Method

### GET — List

```csharp
/// <summary>Returns all [feature] items.</summary>
[HttpGet]
[ProducesResponseType(typeof(IEnumerable<[Name]ResponseDto>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetAll(CancellationToken ct)
    => Ok(await _getAll.ExecuteAsync(ct));
```

### GET — Single by ID

```csharp
/// <summary>Returns a single [feature] by id.</summary>
[HttpGet("{id:int}")]
[ProducesResponseType(typeof([Name]ResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetById(int id, CancellationToken ct)
{
    var result = await _getById.ExecuteAsync(id, ct);
    return result is null ? NotFound() : Ok(result);
}
```

### POST — Create

```csharp
/// <summary>Creates a new [feature].</summary>
[HttpPost]
[ProducesResponseType(typeof([Name]ResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Create([FromBody] Create[Name]RequestDto dto, CancellationToken ct)
{
    var result = await _create.ExecuteAsync(dto, ct);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

### PUT — Update

```csharp
/// <summary>Updates an existing [feature].</summary>
[HttpPut("{id:int}")]
[ProducesResponseType(typeof([Name]ResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Update(
    int id, [FromBody] Update[Name]RequestDto dto, CancellationToken ct)
    => Ok(await _update.ExecuteAsync(id, dto, ct));
```

### DELETE

```csharp
/// <summary>Deletes a [feature].</summary>
[HttpDelete("{id:int}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Delete(int id, CancellationToken ct)
{
    await _delete.ExecuteAsync(id, ct);
    return NoContent();
}
```

### POST — Custom Business Action (confirm, cancel, duplicate)

```csharp
/// <summary>[Action description] — e.g., Confirms an invoice.</summary>
[HttpPost("{id:int}/[action-slug]")]
[ProducesResponseType(typeof([Name]ResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> [ActionName](int id, CancellationToken ct)
    => Ok(await _[actionUseCase].ExecuteAsync(id, ct));
```

---

## Route Naming Conventions

| Action | Route pattern |
|---|---|
| List | `GET /api/v1/[feature]` |
| Single | `GET /api/v1/[feature]/{id}` |
| Create | `POST /api/v1/[feature]` |
| Update | `PUT /api/v1/[feature]/{id}` |
| Delete | `DELETE /api/v1/[feature]/{id}` |
| Confirm | `POST /api/v1/[feature]/{id}/confirm` |
| Cancel | `POST /api/v1/[feature]/{id}/cancel` |
| Duplicate | `POST /api/v1/[feature]/{id}/duplicate` |

---

## Controller-Level Rules

1. No business logic — only dispatch to UseCase
2. No try/catch — `GlobalExceptionHandler` handles domain exceptions
3. All actions accept `CancellationToken ct` last
4. Use `[ProducesResponseType]` for Swagger accuracy
5. `[Authorize]` on the class (not per-method) unless one action is public
6. Input validation only at HTTP boundary: null check, `[Required]` attributes on DTO
