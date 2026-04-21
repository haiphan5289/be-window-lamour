---
name: ct-module
description: Generate a complete BE ASP.NET Core feature module across all 4 Clean Architecture layers in one shot. Creates Entity + EF Configuration + Repository + UseCases (CRUD) + DTOs + Controller + DI extension + migration command. Use when adding an entirely new business feature.
argument-hint: "moduleName:[Name] operations:[get,create,update,delete,duplicate]"
---

# BE Module Scaffold — Complete Feature Generator

> **Anti-Hallucination:** Verify project namespaces and AppDbContext location before generating.

Generates a **complete feature module** across all layers for the BE Window Lamour project.

---

## Files Generated

```
Lamour.Domain/
└── Entities/[Name].cs

Lamour.Infrastructure/
├── Persistence/Configurations/[Name]Configuration.cs
└── Repositories/[Name]Repository.cs

Lamour.Application/
└── Features/[Name]/
    ├── Dtos/[Name]Dtos.cs
    └── UseCases/
        ├── GetAll[Name]UseCase.cs
        ├── GetById[Name]UseCase.cs
        ├── Create[Name]UseCase.cs
        ├── Update[Name]UseCase.cs
        └── Delete[Name]UseCase.cs

Lamour.Api/
├── Controllers/[Name]Controller.cs
└── [Name]ServiceCollectionExtensions.cs
```

Plus: `dotnet ef migrations add Add[Name]` command.

---

## Generation Steps

### Step 1 — Domain Entity

```csharp
// Lamour.Domain/Entities/[Name].cs
namespace Lamour.Domain.Entities;

public sealed class [Name]
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Step 2 — EF Configuration

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
        builder.ToTable("[table_name_plural_snake_case]");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
    }
}
```

Add to `AppDbContext`:
```csharp
public DbSet<[Name]> [Name]s { get; set; }
```

### Step 3 — Repository

```csharp
// Lamour.Infrastructure/Repositories/[Name]Repository.cs
using Lamour.Application.Features.[Name].Dtos;
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
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
}

public sealed class [Name]Repository : I[Name]Repository
{
    private readonly AppDbContext _db;
    public [Name]Repository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<[Name]ResponseDto>> GetAllAsync(CancellationToken ct = default)
        => await _db.[Name]s.AsNoTracking().Select(x => Map(x)).ToListAsync(ct);

    public async Task<[Name]ResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var x = await _db.[Name]s.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        return x is null ? null : Map(x);
    }

    public async Task<[Name]ResponseDto> CreateAsync([Name] entity, CancellationToken ct = default)
    {
        _db.[Name]s.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<[Name]ResponseDto> UpdateAsync(int id, Update[Name]RequestDto dto, CancellationToken ct = default)
    {
        await _db.[Name]s.Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Code, dto.Code)
                .SetProperty(x => x.Name, dto.Name), ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
        => await _db.[Name]s.Where(x => x.Id == id).ExecuteDeleteAsync(ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _db.[Name]s.AnyAsync(x => x.Code.ToLower() == code.ToLower()
            && (excludeId == null || x.Id != excludeId), ct);

    private static [Name]ResponseDto Map([Name] x) => new() { Id = x.Id, Code = x.Code, Name = x.Name };
}
```

### Step 4 — DTOs

```csharp
// Lamour.Application/Features/[Name]/Dtos/[Name]Dtos.cs
using System.Text.Json.Serialization;

namespace Lamour.Application.Features.[Name].Dtos;

public class [Name]ResponseDto
{
    [JsonPropertyName("id")]   public int Id { get; set; }
    [JsonPropertyName("code")] public string Code { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public class Create[Name]RequestDto
{
    [JsonPropertyName("code")] public string Code { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public class Update[Name]RequestDto
{
    [JsonPropertyName("code")] public string Code { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}
```

### Step 5 — UseCases

```csharp
// Lamour.Application/Features/[Name]/UseCases/GetAll[Name]UseCase.cs
public interface IGetAll[Name]UseCase
{
    Task<IEnumerable<[Name]ResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
public sealed class GetAll[Name]UseCase(I[Name]Repository repo) : IGetAll[Name]UseCase
{
    public Task<IEnumerable<[Name]ResponseDto>> ExecuteAsync(CancellationToken ct = default)
        => repo.GetAllAsync(ct);
}

// Create[Name]UseCase.cs
public interface ICreate[Name]UseCase
{
    Task<[Name]ResponseDto> ExecuteAsync(Create[Name]RequestDto dto, CancellationToken ct = default);
}
public sealed class Create[Name]UseCase(I[Name]Repository repo) : ICreate[Name]UseCase
{
    public async Task<[Name]ResponseDto> ExecuteAsync(Create[Name]RequestDto dto, CancellationToken ct = default)
    {
        if (await repo.CodeExistsAsync(dto.Code, ct: ct))
            throw new DomainException($"Code '{dto.Code}' already exists.");
        var entity = new Domain.Entities.[Name] { Code = dto.Code, Name = dto.Name };
        return await repo.CreateAsync(entity, ct);
    }
}

// Delete[Name]UseCase.cs
public interface IDelete[Name]UseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
public sealed class Delete[Name]UseCase(I[Name]Repository repo) : IDelete[Name]UseCase
{
    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        if (!await repo.ExistsAsync(id, ct))
            throw new NotFoundException(nameof([Name]), id);
        await repo.DeleteAsync(id, ct);
    }
}
```

### Step 6 — Controller

```csharp
// Lamour.Api/Controllers/[Name]Controller.cs
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class [Name]Controller : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] IGetAll[Name]UseCase uc, CancellationToken ct)
        => Ok(await uc.ExecuteAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create[Name]RequestDto dto,
        [FromServices] ICreate[Name]UseCase uc, CancellationToken ct)
    {
        var result = await uc.ExecuteAsync(dto, ct);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Update[Name]RequestDto dto,
        [FromServices] IUpdate[Name]UseCase uc, CancellationToken ct)
        => Ok(await uc.ExecuteAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id,
        [FromServices] IDelete[Name]UseCase uc, CancellationToken ct)
    {
        await uc.ExecuteAsync(id, ct);
        return NoContent();
    }
}
```

### Step 7 — DI Registration

```csharp
// Lamour.Api/[Name]ServiceCollectionExtensions.cs
public static IServiceCollection Add[Name](this IServiceCollection services)
{
    services.AddScoped<I[Name]Repository, [Name]Repository>();
    services.AddScoped<IGetAll[Name]UseCase, GetAll[Name]UseCase>();
    services.AddScoped<ICreate[Name]UseCase, Create[Name]UseCase>();
    services.AddScoped<IUpdate[Name]UseCase, Update[Name]UseCase>();
    services.AddScoped<IDelete[Name]UseCase, Delete[Name]UseCase>();
    return services;
}
```

Call from `Program.cs`: `builder.Services.Add[Name]();`

### Step 8 — Migration

```bash
dotnet ef migrations add Add[Name] \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api

dotnet ef database update \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

---

## Post-Generation Checklist

- [ ] Entity added to `AppDbContext.cs` as `DbSet<[Name]>`
- [ ] `[Name]Configuration` applied in `AppDbContext.OnModelCreating`
- [ ] DI extension called in `Program.cs`
- [ ] Migration created and applied
- [ ] All DTOs use `[JsonPropertyName("snake_case")]`
- [ ] Business rules added to UseCases
- [ ] Unit tests written
