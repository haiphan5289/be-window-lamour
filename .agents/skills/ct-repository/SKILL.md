---
name: ct-repository
description: Generate a basic BE Repository (interface + EF Core implementation) following Clean Architecture. Repositories abstract EF Core access, returning DTOs. Use when adding a new data access layer for an existing entity.
argument-hint: "repositoryName:[Name] feature:[Feature]"
---

# BE Repository Generator

> **Anti-Hallucination:** Verify the Entity name, AppDbContext DbSet name, and DTO types before generating.

Generates `I[Name]Repository` interface + `[Name]Repository` EF Core implementation.

---

## Output Files

| File | Layer |
|---|---|
| `Lamour.Infrastructure/Repositories/[Name]Repository.cs` | Infrastructure |

---

## Template

```csharp
// Lamour.Infrastructure/Repositories/[Name]Repository.cs
using Lamour.Application.Features.[Feature].Dtos;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public interface I[Name]Repository
{
    Task<IEnumerable<[Name]ResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<[Name]ResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<[Name]ResponseDto> CreateAsync([Name] entity, CancellationToken ct = default);
    Task<[Name]ResponseDto> UpdateAsync(int id, Update[Name]RequestDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
}

public sealed class [Name]Repository : I[Name]Repository
{
    private readonly AppDbContext _db;

    public [Name]Repository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<[Name]ResponseDto>> GetAllAsync(CancellationToken ct = default)
        => await _db.[Name]s
            .AsNoTracking()
            .Select(x => Map(x))
            .ToListAsync(ct);

    public async Task<[Name]ResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.[Name]s
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<[Name]ResponseDto> CreateAsync([Name] entity, CancellationToken ct = default)
    {
        _db.[Name]s.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<[Name]ResponseDto> UpdateAsync(
        int id, Update[Name]RequestDto dto, CancellationToken ct = default)
    {
        var entity = await _db.[Name]s.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof([Name]), id);

        entity.Name = dto.Name;
        // Map other updated fields here

        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
        => await _db.[Name]s.Where(x => x.Id == id).ExecuteDeleteAsync(ct);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.[Name]s.AnyAsync(x => x.Id == id, ct);

    public async Task<bool> CodeExistsAsync(
        string code, int? excludeId = null, CancellationToken ct = default)
        => await _db.[Name]s.AnyAsync(
            x => x.Code.ToLower() == code.ToLower()
                 && (excludeId == null || x.Id != excludeId), ct);

    private static [Name]ResponseDto Map([Name] x) => new()
    {
        Id = x.Id,
        Name = x.Name
        // Map all DTO fields here
    };
}
```

---

## EF Core Best Practices Applied

| Rule | Applied Where |
|---|---|
| `AsNoTracking()` | All read queries (GetAll, GetById) |
| `ExecuteDeleteAsync()` | Delete — avoids loading entity just to delete |
| `ExecuteUpdateAsync()` | Bulk updates — avoids loading entity just to update scalars |
| Map to DTO at boundary | `Map()` private method — entities never leave the repository |
| `NotFoundException` on missing | `GetById`, `Update` when entity not found |
| Snake_case JSON | In DTO files with `[JsonPropertyName]` |

---

## Registration

```csharp
// In [Feature]ServiceCollectionExtensions.cs
services.AddScoped<I[Name]Repository, [Name]Repository>();
```
