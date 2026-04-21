---
name: ct-scaffold
description: Generate individual BE ASP.NET Core files following Clean Architecture. Creates single file types for an existing feature — Controller, UseCase, Repository, Entity, DTO, EF Configuration, or DI extension. Use when adding one file to an existing module, not a full module from scratch.
argument-hint: "fileName:[Name] fileType:[Controller|UseCase|Repository|Entity|Dto|EfConfig|DiExtension] feature:[FeatureName]"
---

# BE Scaffold Skill — Single File Generator

> **Anti-Hallucination:** Verify all namespaces and existing interfaces before generating.

Generate a **single file** for an existing BE feature following Clean Architecture.

---

## Supported File Types

| `fileType` | Output | Layer |
|---|---|---|
| `Entity` | Domain entity class | `Lamour.Domain/Entities/` |
| `EfConfig` | IEntityTypeConfiguration<T> | `Lamour.Infrastructure/Persistence/Configurations/` |
| `Repository` | Interface + implementation | `Lamour.Infrastructure/Repositories/` |
| `UseCase` | Interface + implementation | `Lamour.Application/Features/[Feature]/UseCases/` |
| `Dto` | Request + Response DTOs | `Lamour.Application/Features/[Feature]/Dtos/` |
| `Controller` | ApiController | `Lamour.Api/Controllers/` |
| `DiExtension` | ServiceCollectionExtensions | `Lamour.Api/` |

---

## Templates

### Entity
```csharp
// Lamour.Domain/Entities/[Name].cs
namespace Lamour.Domain.Entities;

public sealed class [Name]
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    // Add domain fields here
}
```

### EfConfig
```csharp
// Lamour.Infrastructure/Persistence/Configurations/[Name]Configuration.cs
using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public sealed class [Name]Configuration : IEntityTypeConfiguration<[Name]>
{
    public void Configure(EntityTypeBuilder<[Name]> builder)
    {
        builder.ToTable("[table_name]");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
    }
}
```

### Repository
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
    Task<[Name]ResponseDto> UpdateAsync([Name] entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}

public sealed class [Name]Repository : I[Name]Repository
{
    private readonly AppDbContext _db;

    public [Name]Repository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<[Name]ResponseDto>> GetAllAsync(CancellationToken ct = default)
        => await _db.[Name]s.AsNoTracking().Select(x => Map(x)).ToListAsync(ct);

    public async Task<[Name]ResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.[Name]s.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<[Name]ResponseDto> CreateAsync([Name] entity, CancellationToken ct = default)
    {
        _db.[Name]s.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<[Name]ResponseDto> UpdateAsync([Name] entity, CancellationToken ct = default)
    {
        _db.[Name]s.Update(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
        => await _db.[Name]s.Where(x => x.Id == id).ExecuteDeleteAsync(ct);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _db.[Name]s.AnyAsync(x => x.Id == id, ct);

    private static [Name]ResponseDto Map([Name] x) => new()
    {
        Id = x.Id,
        Name = x.Name
    };
}
```

### UseCase
```csharp
// Lamour.Application/Features/[Feature]/UseCases/[Operation][Name]UseCase.cs
using Lamour.Application.Features.[Feature].Dtos;
using Lamour.Infrastructure.Repositories;

namespace Lamour.Application.Features.[Feature].UseCases;

public interface I[Operation][Name]UseCase
{
    Task<[Name]ResponseDto> ExecuteAsync([Operation][Name]RequestDto dto, CancellationToken ct = default);
}

public sealed class [Operation][Name]UseCase : I[Operation][Name]UseCase
{
    private readonly I[Name]Repository _repository;

    public [Operation][Name]UseCase(I[Name]Repository repository) => _repository = repository;

    public async Task<[Name]ResponseDto> ExecuteAsync(
        [Operation][Name]RequestDto dto, CancellationToken ct = default)
    {
        // TODO: Add business rule validation
        var entity = new Domain.Entities.[Name] { Name = dto.Name };
        return await _repository.CreateAsync(entity, ct);
    }
}
```

### Dto
```csharp
// Lamour.Application/Features/[Feature]/Dtos/[Name]Dtos.cs
using System.Text.Json.Serialization;

namespace Lamour.Application.Features.[Feature].Dtos;

public class [Name]ResponseDto
{
    [JsonPropertyName("id")]   public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public class Create[Name]RequestDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public class Update[Name]RequestDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}
```

### Controller
```csharp
// Lamour.Api/Controllers/[Name]Controller.cs
using Lamour.Application.Features.[Feature].Dtos;
using Lamour.Application.Features.[Feature].UseCases;
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
    private readonly IUpdate[Name]UseCase _update;
    private readonly IDelete[Name]UseCase _delete;

    public [Name]Controller(
        IGetAll[Name]UseCase getAll,
        ICreate[Name]UseCase create,
        IUpdate[Name]UseCase update,
        IDelete[Name]UseCase delete)
    {
        _getAll = getAll; _create = create; _update = update; _delete = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _getAll.ExecuteAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _getAll.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create[Name]RequestDto dto, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Update[Name]RequestDto dto, CancellationToken ct)
        => Ok(await _update.ExecuteAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _delete.ExecuteAsync(id, ct);
        return NoContent();
    }
}
```

### DiExtension
```csharp
// Lamour.Api/[Name]ServiceCollectionExtensions.cs
using Lamour.Application.Features.[Feature].UseCases;
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
        services.AddScoped<IUpdate[Name]UseCase, Update[Name]UseCase>();
        services.AddScoped<IDelete[Name]UseCase, Delete[Name]UseCase>();
        return services;
    }
}
```
