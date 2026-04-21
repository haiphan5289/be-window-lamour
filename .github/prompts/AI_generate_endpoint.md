# [AI] Auto-generate an API Endpoint through all layers

**Owned by:** Hai Phan
**Date:** 2026-04-21

---

## 1. Aim

### Objective
Generate a new REST API endpoint across all 4 Clean Architecture layers:
**Domain** → **Infrastructure** → **Application** → **Presentation**.

### Layers Covered
```
┌─────────────────────┐
│ Lamour.Api          │ ← 4. Controller + Route
├─────────────────────┤
│ Lamour.Application  │ ← 3. IUseCase + UseCase + DTOs
├─────────────────────┤
│ Lamour.Infrastructure│ ← 2. IRepository + Repository (EF Core)
├─────────────────────┤
│ Lamour.Domain       │ ← 1. Entity + Exceptions (if new)
└─────────────────────┘
```

---

## 2. Inputs Required

Before generating, confirm:
- **Feature name**: e.g., `Employee`
- **HTTP method + route**: e.g., `GET /api/v1/employees`
- **Input DTO fields**: e.g., `name: string, phone: string, role: EmployeeRole`
- **Output DTO fields**: e.g., `id: int, name: string, role: string`
- **Business rules**: e.g., code must be unique, stock guard, invoice immutability

---

## 3. Step-by-Step Generation Guide

### Step 1: Domain Entity (if new)

**File:** `src/Lamour.Domain/Entities/[Name].cs`

```csharp
namespace Lamour.Domain.Entities;

public sealed class [Name]
{
    public int Id { get; set; }
    public string [Field] { get; set; } = "";
    // Add all fields
}
```

### Step 2: EF Core Configuration

**File:** `src/Lamour.Infrastructure/Persistence/Configurations/[Name]Configuration.cs`

```csharp
using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class [Name]Configuration : IEntityTypeConfiguration<[Name]>
{
    public void Configure(EntityTypeBuilder<[Name]> builder)
    {
        builder.ToTable("[table_name]");    // snake_case plural
        builder.HasKey(x => x.Id);
        builder.Property(x => x.[Field]).IsRequired().HasMaxLength(100);
    }
}
```

Add `DbSet<[Name]> [Name]s { get; set; }` to `AppDbContext`.

### Step 3: Repository

**File:** `src/Lamour.Infrastructure/Repositories/[Name]Repository.cs`

```csharp
namespace Lamour.Infrastructure.Repositories;

public interface I[Name]Repository
{
    Task<IEnumerable<[Name]ResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<[Name]ResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<[Name]ResponseDto> CreateAsync([Name] entity, CancellationToken ct = default);
    Task<[Name]ResponseDto> UpdateAsync([Name] entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}

public sealed class [Name]Repository : I[Name]Repository
{
    private readonly AppDbContext _db;

    public [Name]Repository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<[Name]ResponseDto>> GetAllAsync(CancellationToken ct = default)
        => await _db.[Name]s
            .AsNoTracking()
            .Select(x => MapToDto(x))
            .ToListAsync(ct);

    public async Task<[Name]ResponseDto> CreateAsync([Name] entity, CancellationToken ct = default)
    {
        _db.[Name]s.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
        => await _db.[Name]s.Where(x => x.Id == id).ExecuteDeleteAsync(ct);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.[Name]s.AnyAsync(x => x.Id == id, ct);

    private static [Name]ResponseDto MapToDto([Name] x) => new()
    {
        Id = x.Id,
        // Map all fields
    };
}
```

### Step 4: UseCase + DTOs

**File:** `src/Lamour.Application/Features/[Name]/Dtos/[Name]ResponseDto.cs`

```csharp
using System.Text.Json.Serialization;

namespace Lamour.Application.Features.[Name].Dtos;

public class [Name]ResponseDto
{
    [JsonPropertyName("id")]     public int Id { get; set; }
    [JsonPropertyName("name")]   public string Name { get; set; } = "";
    // All fields snake_case
}

public class Create[Name]RequestDto
{
    [JsonPropertyName("name")]   public string Name { get; set; } = "";
    // Input fields
}
```

**File:** `src/Lamour.Application/Features/[Name]/UseCases/[Name]UseCase.cs`

```csharp
namespace Lamour.Application.Features.[Name].UseCases;

public interface ICreate[Name]UseCase
{
    Task<[Name]ResponseDto> ExecuteAsync(Create[Name]RequestDto dto, CancellationToken ct = default);
}

public sealed class Create[Name]UseCase : ICreate[Name]UseCase
{
    private readonly I[Name]Repository _repository;

    public Create[Name]UseCase(I[Name]Repository repository) => _repository = repository;

    public async Task<[Name]ResponseDto> ExecuteAsync(
        Create[Name]RequestDto dto, CancellationToken ct = default)
    {
        // Add business rule validation here
        var entity = new [Name] { Name = dto.Name, /* map all fields */ };
        return await _repository.CreateAsync(entity, ct);
    }
}
```

### Step 5: Controller

**File:** `src/Lamour.Api/Controllers/[Name]Controller.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class [Name]Controller : ControllerBase
{
    private readonly IGetAll[Name]UseCase _getAll;
    private readonly ICreate[Name]UseCase _create;
    private readonly IDelete[Name]UseCase _delete;

    public [Name]Controller(
        IGetAll[Name]UseCase getAll,
        ICreate[Name]UseCase create,
        IDelete[Name]UseCase delete)
    {
        _getAll = getAll;
        _create = create;
        _delete = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _getAll.ExecuteAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] Create[Name]RequestDto dto, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(dto, ct);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _delete.ExecuteAsync(id, ct);
        return NoContent();
    }
}
```

### Step 6: DI Registration

**File:** `src/Lamour.Api/[Name]ServiceCollectionExtensions.cs`

```csharp
using Lamour.Application.Features.[Name].UseCases;
using Lamour.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Lamour.Api;

public static class [Name]ServiceCollectionExtensions
{
    public static IServiceCollection Add[Name](this IServiceCollection services)
    {
        services.AddScoped<I[Name]Repository, [Name]Repository>();
        services.AddScoped<IGetAll[Name]UseCase, GetAll[Name]UseCase>();
        services.AddScoped<ICreate[Name]UseCase, Create[Name]UseCase>();
        services.AddScoped<IDelete[Name]UseCase, Delete[Name]UseCase>();
        return services;
    }
}
```

Add `builder.Services.Add[Name]();` in `Program.cs`.

### Step 7: Migration

```bash
dotnet ef migrations add Add[Name] \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api

dotnet ef database update \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

---

## 4. GitHub Copilot Prompt

```
Generate a new API endpoint for [FeatureName] following Clean Architecture across 4 layers:
1. Domain entity: [describe fields]
2. EF Core configuration (table: [table_name])
3. Repository with CRUD methods + AsNoTracking on reads
4. UseCase with business rules: [describe rules]
5. Controller at route: [HTTP_METHOD] /api/v1/[route]
6. DI registration in [Feature]ServiceCollectionExtensions
7. EF Core migration command

All DTOs must use [JsonPropertyName("snake_case")].
All async methods must accept CancellationToken ct = default.
Never use .Result or .Wait().
```

---

## 5. Checklist

- [ ] Entity in `Lamour.Domain/Entities/`
- [ ] EF configuration in `Lamour.Infrastructure/Persistence/Configurations/`
- [ ] `DbSet<T>` added to `AppDbContext`
- [ ] Repository interface + implementation
- [ ] UseCase interface + implementation + DTOs
- [ ] Controller with `[Authorize]`
- [ ] DI extension called from `Program.cs`
- [ ] Migration created and applied
- [ ] Unit tests for UseCase
- [ ] All DTOs use `[JsonPropertyName("snake_case")]`
- [ ] All async methods have `CancellationToken`
- [ ] No `.Result` / `.Wait()`
