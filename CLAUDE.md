# BE Window Lamour — Claude Code Guide

> Backend REST API for the **Lamour** cosmetics business management system.
> Client: WPF desktop app (`desktop-lamour`). Stack: .NET 10, ASP.NET Core Web API, EF Core + PostgreSQL.

## Environment Notes

- **Runtime**: .NET 10 (machine has no .NET 8 — all `.csproj` use `net10.0`)
- **PostgreSQL**: installed via Homebrew — username is `hai.phan`, not `postgres`
- **Connection string**: `Host=localhost;Database=lamour_db;Username=hai.phan;Password=`
- **API listen URL**: `http://0.0.0.0:5282` (bind all interfaces so UTM can reach it)
- **dotnet-ef PATH**: run `export PATH="$PATH:$HOME/.dotnet/tools"` before `dotnet ef` commands
- **WPF client runs on**: UTM (Windows VM), IP `192.168.64.2` — MacBook IP from UTM is `192.168.64.1`
- **UTM sync**: on UTM Terminal 2, run `.\sync.ps1` to robocopy files from `Z:\` → `C:\projects\desktop-lamour\`

## Local DB Scripts

Run from project root `/Users/hai.phan/Desktop/haiphan/be-window-lamour`:

```bash
# 1. Apply all pending migrations to local DB
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef database update --project src/Lamour.Infrastructure --startup-project src/Lamour.Api

# 2. Create a new migration (replace <Name> with migration name, e.g. AddCashTransactions)
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef migrations add <Name> --project src/Lamour.Infrastructure --startup-project src/Lamour.Api

# 3. Run the API locally
dotnet run --project src/Lamour.Api

# 4. Reset DB (drop + recreate + seed)
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef database drop --project src/Lamour.Infrastructure --startup-project src/Lamour.Api --force
dotnet ef database update --project src/Lamour.Infrastructure --startup-project src/Lamour.Api
```

## UTM Workflow (Run on Windows VM)

Step-by-step to run the full stack locally with the WPF client on UTM.

### Step 1 — Start BE API on Mac

```bash
cd /Users/hai.phan/Desktop/haiphan/be-window-lamour
dotnet run --project src/Lamour.Api
```

API will listen on `http://0.0.0.0:5282` — accessible from UTM at `http://192.168.64.1:5282`.

### Step 2 — Sync WPF files to UTM

On **UTM Terminal 2** (PowerShell inside Windows VM), run:

```powershell
cd C:\projects\desktop-lamour
.\sync.ps1
```

Syncs only `src\` (skips `obj\`, `bin\`, `.git\`) + root-level files. Shows progress: `Copied: N  Skipped: N  Synced in Xs!`

### Step 3 — Run WPF on UTM

On **UTM Terminal 1** (PowerShell **Run as Administrator**, inside Windows VM):

> **Prerequisite (one-time):** Enable Windows long path support, otherwise NuGet restore fails with `NETSDK1064`:
> ```powershell
> Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem' -Name 'LongPathsEnabled' -Value 1
> # Then open a NEW terminal for the change to take effect
> ```

```powershell
cd C:\projects\desktop-lamour
dotnet watch run --project src\DesktopLamour
```

> `dotnet watch run` builds and auto-restarts whenever files change after `.\sync.ps1`.

### Network Reference

| Machine | IP |
|---|---|
| MacBook (BE API host) | `192.168.64.1` |
| UTM Windows VM (WPF client) | `192.168.64.2` |

### Quick Checklist

- [ ] BE API running on Mac (`dotnet run --project src/Lamour.Api`)
- [ ] `sync.ps1` executed on UTM Terminal 2
- [ ] WPF app launched on UTM Terminal 1
- [ ] WPF connects to `http://192.168.64.1:5282`

---

## Deployment (Windows Server)

### Khi deploy lên máy chủ/máy client mới — chỗ cần sửa

**1. WPF client — đổi BE URL (10 chỗ)**

File: `/Users/hai.phan/Desktop/haiphan/desktop-lamour/src/DesktopLamour/Features/HomePage/HomeServiceCollectionExtensions.cs`

Tìm tất cả: `http://192.168.64.1:5282` → thay bằng IP server mới (ví dụ: `http://192.168.1.50:5282`)

Có đúng **10 dòng** cần sửa (ProductService, SupplierService, CustomerService, EmployeeService, CashLedgerService, ReceiptService, PaymentService, WarehouseService, WarehouseReceiptService, SalesOrderService).

**2. BE config — đổi PostgreSQL credentials**

File: `src/Lamour.Api/appsettings.Production.json` (tạo mới nếu chưa có)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=lamour_db;Username=lamour;Password=<mật_khẩu>"
  }
}
```

**3. Publish commands**

```bat
REM BE — chạy trên Windows Server
publish-be.bat

REM WPF — chạy trên máy developer, copy output sang client
publish-wpf.bat
```

Scripts nằm ở: `deploy/publish-be.bat`, `deploy/publish-wpf.bat`, `deploy/installer.iss`, `deploy/postgresql-setup.sql`

---

## Feature Docs (REQUIRED before implement or debug)

Each feature has a dedicated doc. **Always read the feature doc before implementing or debugging.**

| Feature | Doc path |
|---------|----------|
| Sales / Chứng từ bán hàng | `src/Lamour.Application/Features/Sales/docs/sales.md` |
| SalesReturn / Chứng từ hàng bán bị trả lại | `src/Lamour.Application/Features/SalesReturn/docs/sales-return.md` |
| Products / Sản phẩm | `src/Lamour.Application/Features/Products/docs/products.md` |
| Categories / Danh mục | `src/Lamour.Application/Features/Categories/docs/categories.md` |

> For any other feature, check `src/Lamour.Application/Features/[Feature]/docs/` first.

---

## Agent Routing

When a task matches a domain below, **spawn the appropriate agent** via the Agent tool before responding directly.

| Task type | Agent to invoke | Trigger keywords |
|---|---|---|
| Implement feature, fix bug, scaffold layers, wire UseCase | `lamour-be-expert` | implement, add feature, usecase, repository, controller, bug, crash, error, exception |
| Business rules, domain models, invoice logic, stock, VAT | `lamour-domain-expert` | business rule, domain, inventory, invoice, stock, employee, role, supplier, VAT, validate |
| Module navigation, architecture, file structure, DI context | `lamour-module-context-expert` | module, architecture, structure, folder, which layer, navigate code |

## Skill Routing

| Task type | Skill | When to invoke |
|---|---|---|
| New API feature end-to-end | `/ct-be-to-desktop` | Creating a new module that desktop-lamour WPF must consume |
| Ask requirements first | `/ct-flipped-interaction` | Vague or underspecified feature request |
| Full feature pipeline | `/ct-feature-pipeline` | Complete scaffold across all 4 layers |

## Hook Skill Auto-Invoke (REQUIRED)

When `<system-reminder>` contains a `Skills auto-triggered` block from the hook, you **MUST** invoke every listed skill via the `Skill` tool **before** writing any response or code. This is a hard requirement — do not skip, summarize, or assume the skill content.

```
🔧 Skills auto-triggered:
   ▶ /skill-name   ← invoke this via Skill tool immediately
```

- Invoke skills **in the order listed**
- If multiple skills are listed, invoke all of them sequentially
- Only after all skills are loaded may you begin your response

# BE Desktop Lamour Project - 

> **Desktop Lamour Project:** This BE project will serve for desktop-lamour.  Verify every class, interface, route, and EF entity against the codebase before generating code. /Users/hai.phan/Desktop/haiphan/desktop-lamour.

## Project Stack

- **Platform**: .NET 8, ASP.NET Core Web API
- **ORM**: EF Core 8 + PostgreSQL (Npgsql)
- **Auth**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **DI**: `Microsoft.Extensions.DependencyInjection` — constructor injection only
- **Tests**: xUnit + Moq

## Architecture (4 layers, strictly separated)

```
Lamour.Api           (Controllers, Middleware)
     ↕ interfaces
Lamour.Application   (UseCases, DTOs)
     ↕ interfaces
Lamour.Domain        (Entities, Enums, Exceptions — zero deps)
     ↕ interfaces
Lamour.Infrastructure (Repositories, EF Core, AppDbContext)
```

## Module Structure

```
src/
├── Lamour.Api/
│   ├── Controllers/[Feature]Controller.cs
│   ├── Middleware/GlobalExceptionHandler.cs
│   └── Program.cs
├── Lamour.Application/
│   └── Features/[Feature]/
│       ├── UseCases/I[Name]UseCase.cs + [Name]UseCase.cs
│       └── Dtos/[Name]RequestDto.cs + [Name]ResponseDto.cs
├── Lamour.Domain/
│   ├── Entities/[Name].cs
│   └── Exceptions/DomainException.cs
├── Lamour.Infrastructure/
│   ├── Persistence/AppDbContext.cs
│   ├── Persistence/Configurations/[Name]Configuration.cs
│   └── Repositories/[Name]Repository.cs
└── Lamour.Contracts/           # Shared DTOs (referenced by WPF client)
```

## Business Domains

- **Authentication** — phone-based sign up/login, JWT tokens
- **Employees** — staff profiles, roles (Admin / Cashier / Warehouse)
- **Inventory** — cosmetics products, stock levels, low-stock alerts
- **ImportInvoices** — purchase from suppliers → increases stock (NK-YYYYMMDD-NNN)
- **ExportInvoices** — sales to customers → decreases stock, VAT 10% (XK-YYYYMMDD-NNN)
- **Suppliers** — CRUD + duplicate; code is unique case-insensitive

## Mandatory Rules

1. All async public methods accept `CancellationToken ct = default`
2. Never `.Result` or `.Wait()` — always `await`
3. Constructor injection only — never `new XxxService()` or service locator
4. DTOs only cross layer boundaries — never return EF entities from API
5. `AsNoTracking()` on all read-only EF Core queries
6. All JSON fields use `[JsonPropertyName("snake_case")]` — WPF client expects snake_case
7. Confirmed invoices are immutable — only cancellation allowed
8. Stock never goes negative — validate before confirming export invoice
9. Use `ILogger<T>` — never `Console.WriteLine` or `Debug.Print`
10. Store `DateTime.UtcNow` — convert to local time in WPF client
