---
name: pa-preview-guard
description: Tự động phát hiện, sửa, và ngăn chặn lỗi build (C#/.NET) phát sinh sau khi generate/scaffold code cho BE (be-window-lamour, ASP.NET Core + EF Core) và WPF (desktop-lamour, MVVM + XAML)
model: sonnet
effort: high
---

# Build Doctor — BE (Lamour API) + WPF (DesktopLamour)

**Post-generation auto-fixer** — Tự động phát hiện và sửa lỗi build sau khi generate code (thêm field, entity mới, UseCase mới, wire feature mới xuyên 2 project). Không cần fix từng case thủ công nữa.

Áp dụng cho 2 project trong hệ sinh thái Lamour:

| Project | Path | Stack |
|---|---|---|
| **BE** | `be-window-lamour` | .NET 10, ASP.NET Core Web API, EF Core + PostgreSQL, Clean Architecture (Domain/Application/Infrastructure/Api) |
| **WPF** | `desktop-lamour` | .NET (net8.0-windows), WPF, MVVM (CommunityToolkit.Mvvm), XAML |

## Vấn đề giải quyết

Sau khi generate code (thêm entity/field mới, scaffold UseCase, wire feature mới theo `/ct-be-to-desktop`, redesign popup lớn kiểu "Sửa Vật tư hàng hoá, dịch vụ"), thường gặp lỗi build lặp lại — phần lớn đã từng xảy ra thật trong repo này:

- ❌ Namespace/type trùng tên khi 2 feature cùng dùng 1 danh từ nghiệp vụ (ví dụ: feature `Warehouse` cũ và `Warehouses` mới) → `CS0118`
- ❌ Định nghĩa trùng lặp khi 1 Configuration/class đã tồn tại ở file khác không ngờ tới (ví dụ `WarehouseConfiguration` nằm lẫn trong `WarehouseReceiptConfiguration.cs`) → `CS0101`/`CS0111`
- ❌ Thiếu implement interface member khi thêm field mới vào model implement `ISearchableItem` → `CS0535`
- ❌ Ambiguous reference khi 1 file `using` cả 2 namespace có type trùng tên (`IWarehouseRepository` ở 2 feature khác nhau) → `CS0104`
- ❌ Thiếu argument bắt buộc sau khi đổi record sang `required` property hoặc thêm field mới vào constructor/DTO → `CS7036`/`CS1729`
- ❌ Type mismatch sau khi đổi kiểu field (`string` → `int?` FK, positional record → init-property record) → `CS0029`/`CS1503`
- ❌ Thiếu `using`/assembly reference khi cross-feature wiring (WPF ViewModel mới inject UseCase từ feature khác) → `CS0246`/`CS0103`
- ❌ DI resolve fail lúc runtime vì quên đăng ký UseCase/Repository/Service mới trong `Program.cs` (BE) hoặc `HomeServiceCollectionExtensions.cs` (WPF)
- ❌ EF Core migration seed data (`HasData`) đụng Id đã tồn tại sẵn trong DB (data cũ chèn tay, không qua migration) → Postgres `23505 duplicate key`
- ❌ XAML `StaticResource`/converter chưa đăng ký → `XamlParseException: Cannot find resource named 'X'`

## Giải pháp

Skill này chạy **sau mỗi lần generate/scaffold code** ở BE và/hoặc WPF để:
1. **Build** — chạy `dotnet build` (BE) và/hoặc `dotnet build -p:EnableWindowsTargeting=true` (WPF, build trên Mac làm proxy compile-check) để lấy lỗi thật
2. **Classify** — match lỗi với pattern trong `ERROR_PATTERNS.md` (namespace collision, duplicate definition, missing interface member, missing argument, type mismatch, missing using, DI resolution, EF seed collision, XAML resource)
3. **Auto-fix** — áp dụng chiến lược tương ứng trong `FIX_STRATEGIES.md`
4. **Re-build** — lặp lại cho đến khi build sạch hoặc hết `MAX_ITERATIONS`
5. **Report** — files đã sửa, lỗi còn lại cần tay

## Files

| File | Purpose |
|---|---|
| [spec/PROMPT.md](spec/PROMPT.md) | Step-by-step execution workflow (build → classify → fix → rebuild) |
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Input parameters (MODE, PROJECT, ERROR_TYPES, MAX_ITERATIONS) |
| [spec/ERROR_PATTERNS.md](spec/ERROR_PATTERNS.md) | Error classification (CS-code patterns, EF/DI/XAML-specific) |
| [spec/FIX_STRATEGIES.md](spec/FIX_STRATEGIES.md) | Per-error-type auto-fix logic |
| [spec/EXAMPLES.md](spec/EXAMPLES.md) | Worked examples — lấy từ incident thật trong repo này |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | Safety rules (Clean Architecture, migration đã apply, git safety) |

## Quick Start

```
# Mode 1: Auto-scan và fix tất cả lỗi, cả 2 project
MODE: auto
PROJECT: both

# Mode 2: Chỉ build/fix 1 project
MODE: auto
PROJECT: be        # hoặc: wpf

# Mode 3: Fix loại lỗi cụ thể
MODE: targeted
ERROR_TYPES: namespace_type_collision,missing_interface_member

# Mode 4: Dry-run (chỉ báo cáo, không sửa)
MODE: dry-run
```

## Output

```
✅ Build Doctor Report
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Project: BE + WPF
📊 Errors scanned: 9
🔧 Errors fixed: 8
❌ Errors remaining: 1 (manual intervention needed)

Fixed breakdown:
  ✅ Namespace/type collision (CS0118): 3 files (BE)
  ✅ Duplicate definition (CS0101): 1 file (BE)
  ✅ Missing interface member (CS0535): 1 file (WPF)
  ✅ Missing using (CS0246): 2 files (WPF)
  ✅ XAML resource not found: 1 file (WPF)

Manual fixes needed:
  ❌ ProductConfiguration.cs:45 — cần quyết định OnDelete behavior cho FK mới (Restrict vs SetNull), không tự suy ra được
```
