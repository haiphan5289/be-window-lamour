---
name: ct-be-to-desktop
description: Full end-to-end workflow for a new BE API feature — from clarifying questions to implementation and WPF desktop wiring. Automatically asks requirements first (flipped interaction), then scaffolds BE (entity → EF → repository → usecases → controller → migration) and wires the WPF desktop client (replace mock → typed HttpClient → UTM sync). Single command to go from idea to working API.
model: sonnet
effort: high
---

# BE → Desktop: Full Pipeline (Ask → Build → Wire)

> **Anti-Hallucination:** Verify every class, interface, route, and EF entity against the codebase before generating code. See [ct-anti-hallucination](../ct-anti-hallucination/SKILL.md).

> One skill, three phases: **Ask** requirements → **Build** BE API → **Wire** WPF client.

## Input Format

```
FEATURE_REQUEST: [Feature description]
CONTEXT: [Why this feature is needed]
PRIORITY: High / Medium / Low
```

## Priority Behavior

| Priority | Questions | Scope |
|---|---|---|
| **High** | 1–2 (route + return shape only) | Fastest path, reuse existing patterns |
| **Medium** | 3–4 (business rules + edge cases) | Full validation, new UseCase |
| **Low** | 4–5 (all edge cases + tests + DI) | Optimal architecture, full checklist |

---

## Phase 1 — ASK (Flipped Interaction)

**CRITICAL: Ask questions FIRST. Do NOT write any code until all requirements are confirmed.**

### Step 1 — Read WPF client first

Before asking anything, check the WPF client at `/Users/hai.phan/Desktop/haiphan/desktop-lamour`:
- Find existing mock `Service` at `Features/[Module]/Data/Services/[Name]Service.cs`
- Find `IService` interface to know what methods are expected
- Find `Domain/Models/[Name].cs` to derive DTO shape
- Find `ServiceCollectionExtensions` to see current DI registration

This eliminates most questions about DTO shape and method signatures.

### Step 2 — Ask clarifying questions (one at a time)

**Q1 — Scope** (always ask):
> The WPF client's `I[Name]Service` defines X operations: [list them].
> Should I implement **all endpoints** now, or a subset?

**Q2 — Auth**:
> Should all endpoints require Bearer JWT `[Authorize]`, or are some public?
> (Note: if WPF auth flow not yet wired, we remove `[Authorize]` temporarily)

**Q3 — Business rules** (Medium/Low only):
> Any uniqueness constraints (e.g. `code` case-insensitive)?
> Which fields are required vs optional?

**Q4 — GET list filtering** (Low only):
> Should `GET /api/v1/[name]s` support query filters, or return all records always?

**Q5 — Tests** (Low only):
> Do you need xUnit + Moq unit tests for UseCases?

### Step 3 — Confirm understanding

Before writing code, present a full summary table:

```
Confirmed scope:
- GET /api/v1/[name]s       → 200 list, no filter
- POST /api/v1/[name]s      → 201, validates X
- PUT /api/v1/[name]s/{id}  → 200, 404 / 400
- DELETE /api/v1/[name]s/{id} → 204, 404
- POST /api/v1/[name]s/{id}/duplicate → 201, 404 / 400

Auth: [Authorize] on all / removed temporarily
Business rules: [list]
WPF: replace mock SupplierService → typed HttpClient

Does this match? Shall I start?
```

---

## Phase 2 — BUILD (BE Clean Architecture)

Only start after user confirms requirements.

### Layer order (always follow this sequence)

**1. Domain Entity** — `src/Lamour.Domain/Entities/[Name].cs`
```csharp
namespace Lamour.Domain.Entities;
public class [Name]
{
    public int    Id   { get; set; }
    public string Code { get; set; } = string.Empty;
    // ... fields matching WPF Domain Model
}
```

**2. EF Configuration** — `src/Lamour.Infrastructure/Persistence/Configurations/[Name]Configuration.cs`
```csharp
public class [Name]Configuration : IEntityTypeConfiguration<[Name]>
{
    public void Configure(EntityTypeBuilder<[Name]> builder)
    {
        builder.ToTable("[name]s");           // snake_case table
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique(); // if unique constraint needed
    }
}
```

**3. Update AppDbContext** — add `public DbSet<[Name]> [Name]s => Set<[Name]>();`

**4. Repository Interface** — `src/Lamour.Application/Features/[Name]/Repositories/I[Name]Repository.cs`
- Methods: `GetAllAsync`, `GetByIdAsync`, `CodeExistsAsync` (if unique), `AddAsync`, `UpdateAsync`, `DeleteAsync`
- All accept `CancellationToken ct = default`

**5. Repository Implementation** — `src/Lamour.Infrastructure/Repositories/[Name]Repository.cs`
- `AsNoTracking()` on ALL reads
- `SaveChangesAsync(ct)` after every write

**6. DTOs** — `src/Lamour.Application/Features/[Name]/Dtos/`
- All fields: `[JsonPropertyName("snake_case")]`
- `[Name]ResponseDto`, `Create[Name]RequestDto`, `Update[Name]RequestDto`

**7. UseCases** — `src/Lamour.Application/Features/[Name]/UseCases/` (one interface + impl per operation)
- Inject `I[Name]Repository` + `ILogger<T>` via constructor
- `DomainException` → 400, `NotFoundException` → 404
- Map entity → DTO inside UseCase — never return EF entities

**8. Controller** — `src/Lamour.Api/Controllers/[Name]sController.cs`
- `[ApiController]`, `[Route("api/v1/[name]s")]`, `[Authorize]`
- No try/catch — `GlobalExceptionHandler` handles everything
- Returns: `Ok()`, `CreatedAtAction()`, `NoContent()`

**9. DI in Program.cs**
```csharp
builder.Services.AddScoped<I[Name]Repository, [Name]Repository>();
builder.Services.AddScoped<IGet[Name]sUseCase, Get[Name]sUseCase>();
// ... one line per UseCase
```

**10. EF Migration**
```bash
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef migrations add [FeatureName]Create \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
dotnet ef database update \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

---

## Phase 3 — WIRE (WPF Desktop Integration)

### Replace mock Service with typed HttpClient

`Features/[Module]/Data/Services/[Name]Service.cs`:
```csharp
public sealed class [Name]Service : I[Name]Service
{
    private readonly HttpClient            _httpClient;
    private readonly IAuthTokenStorage     _tokenStorage;
    private readonly ILogger<[Name]Service> _logger;

    public [Name]Service(HttpClient httpClient, IAuthTokenStorage tokenStorage, ILogger<[Name]Service> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null ? new AuthenticationHeaderValue("Bearer", token) : null;
    }

    public async Task<IEnumerable<[Name]ResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        SetBearerToken();
        var response = await _httpClient.GetAsync("/api/v1/[name]s", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<[Name]ResponseDto>>(ct)
            ?? Enumerable.Empty<[Name]ResponseDto>();
    }
    // ... other methods follow same pattern
}
```

### Update DI Registration

`Features/[Module]/[Module]ServiceCollectionExtensions.cs`:
```csharp
// REMOVE:
services.AddSingleton<I[Name]Service, [Name]Service>();

// ADD:
services.AddHttpClient<I[Name]Service, [Name]Service>(client =>
{
    client.BaseAddress = new Uri("http://192.168.64.1:5282");
    client.Timeout     = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

### Auth during development

If WPF login not yet wired → temporarily comment out `[Authorize]` on controller:
```csharp
// TODO: restore [Authorize] once WPF auth flow is wired
```

---

## Environment Quick Reference

| Setting | Value |
|---|---|
| .NET version | net10.0 (machine has no .NET 8) |
| PostgreSQL username | `hai.phan` (not `postgres`) |
| Connection string | `Host=localhost;Database=lamour_db;Username=hai.phan;Password=` |
| BE listen URL | `http://0.0.0.0:5282` |
| MacBook IP (from UTM) | `192.168.64.1` |
| dotnet-ef PATH fix | `export PATH="$PATH:$HOME/.dotnet/tools"` |
| UTM sync command | `.\sync.ps1` on UTM Terminal 2 |

## Common Errors

| Error | Fix |
|---|---|
| `role "postgres" does not exist` | Use `Username=hai.phan;Password=` in connection string |
| `dotnet-ef not found` | `export PATH="$PATH:$HOME/.dotnet/tools"` |
| `You must install .NET 8.0` | Change all `.csproj` to `<TargetFramework>net10.0</TargetFramework>` |
| `401 Unauthorized` | Remove `[Authorize]` temporarily until WPF auth is wired |
| `NavigationRoutes.XxxList not found` | Add flat constants to `NavigationRoutes.cs`, run `.\sync.ps1` |
| `NU1605 package downgrade` | Bump `Microsoft.Extensions.Logging.Abstractions` to match Http package version |

---

## Example Interaction

### Sample Input

```
/ct-be-to-desktop
FEATURE_REQUEST: create a new api Suppliers
CONTEXT: desktop-lamour cần hiển thị và quản lý danh sách nhà cung cấp
PRIORITY: Low
```

---

### Phase 1 — AI reads WPF client first (before asking anything)

> Checking `/Users/hai.phan/Desktop/haiphan/desktop-lamour/src/DesktopLamour/Features/HomePage/Suppliers/`...
>
> Found:
> - `ISupplierService` → 5 methods: `GetAllAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `DuplicateAsync`
> - `SupplierResponseDto` → fields: `id`, `code`, `name`, `address`, `group`, `tax_code`, `phone`, `is_stop_tracking`
> - Current `SupplierService.cs` is a pure in-memory mock

---

**Question 1 — Scope:**

> `ISupplierService` có 5 operations: GetAll, Create, Update, Delete, Duplicate.
> Implement **tất cả 5 endpoints** ngay bây giờ, hay chỉ GET list trước?

**User:** all 5

---

**Question 2 — Auth:**

> Tất cả endpoints có cần Bearer JWT `[Authorize]` không?
> (Lưu ý: nếu WPF auth chưa wired, mình sẽ tạm bỏ `[Authorize]`)

**User:** Option A — all require auth, nhưng WPF chưa có token handling

---

**Question 3 — Business rules:**

> - `code` có cần unique case-insensitive không? (400 nếu conflict)
> - `code` và `name` có phải required không?

**User:** có, code unique, code + name required. GET trả hết không filter

---

### Confirmed Understanding (AI presents before coding)

> Mình sẽ implement:
>
> | Method | Route | Response | Errors |
> |---|---|---|---|
> | GET | `/api/v1/suppliers` | 200 full list | — |
> | POST | `/api/v1/suppliers` | 201 created | 400 code conflict / required |
> | PUT | `/api/v1/suppliers/{id}` | 200 updated | 404 / 400 |
> | DELETE | `/api/v1/suppliers/{id}` | 204 | 404 |
> | POST | `/api/v1/suppliers/{id}/duplicate` | 201 copy | 404 / 400 |
>
> Auth: tạm bỏ `[Authorize]` (TODO khi WPF auth xong)
> WPF: thay mock `SupplierService` → typed `HttpClient` + `IAuthTokenStorage`
> Base URL: `http://192.168.64.1:5282`
>
> Bắt đầu implement?

**User:** yes

---

### Phase 2 — AI builds BE (all layers)

> Creating:
> - `Lamour.Domain/Entities/Supplier.cs`
> - `Lamour.Infrastructure/Persistence/Configurations/SupplierConfiguration.cs`
> - `Lamour.Application/Features/Suppliers/Repositories/ISupplierRepository.cs`
> - `Lamour.Infrastructure/Repositories/SupplierRepository.cs`
> - `Lamour.Application/Features/Suppliers/Dtos/` (3 DTOs)
> - `Lamour.Application/Features/Suppliers/UseCases/` (5 pairs)
> - `Lamour.Api/Controllers/SuppliersController.cs`
> - DI in `Program.cs`
> - EF migration `InitialCreate`
> - `dotnet build` → 0 errors ✓
> - `dotnet ef database update` → bảng `suppliers` created ✓

---

### Phase 3 — AI wires WPF client

> Replacing `SupplierService.cs` (in-memory mock → typed HttpClient)
> Updating `HomeServiceCollectionExtensions.cs` (`AddSingleton` → `AddHttpClient`)
> Base URL set to `http://192.168.64.1:5282`
>
> **Next steps:**
> 1. `dotnet run --project src/Lamour.Api` trên MacBook
> 2. `.\sync.ps1` trên UTM Terminal 2
> 3. WPF app tự reload → load data từ BE API ✓

---

## Final Checklist

- [ ] Read WPF mock service → derive DTO shape + method signatures
- [ ] Ask scope, auth, business rules (per Priority level)
- [ ] Confirm understanding before writing code
- [ ] Domain entity → EF config → AppDbContext → Repository → DTOs → UseCases → Controller → DI
- [ ] `dotnet build` — 0 errors
- [ ] EF migration + database update
- [ ] Replace WPF mock service → typed HttpClient + IAuthTokenStorage
- [ ] `AddSingleton` → `AddHttpClient` with `http://192.168.64.1:5282`
- [ ] `dotnet run --project src/Lamour.Api` on MacBook
- [ ] `.\sync.ps1` on UTM Terminal 2
- [ ] WPF loads real data from BE ✓
