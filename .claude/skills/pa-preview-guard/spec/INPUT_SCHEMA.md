# Input Schema — Build Doctor (BE + WPF)

## Input Block

```yaml
MODE: auto | targeted | dry-run
PROJECT: be | wpf | both       # default: both
ERROR_TYPES: [optional] comma-separated list when MODE=targeted
MAX_ITERATIONS: [optional] default=3
```

## Field Definitions

| Field | Type | Required | Description |
|---|---|---|---|
| `MODE` | `Enum` | Yes | Execution mode: `auto`, `targeted`, `dry-run` |
| `PROJECT` | `Enum` | No | Project scope: `be` (be-window-lamour), `wpf` (desktop-lamour), `both`. Default `both` |
| `ERROR_TYPES` | `String` | No | Filter specific error types (only when MODE=targeted) |
| `MAX_ITERATIONS` | `Int` | No | Max fix-validate cycles (default: 3) |

## PROJECT Options

### `be`

Chỉ build/fix `be-window-lamour`:
```bash
cd /Users/hai.phan/Desktop/haiphan/be-window-lamour && dotnet build
```

**Use when:** vừa sửa BE-only (entity, UseCase, migration, controller) chưa đụng WPF.

### `wpf`

Chỉ build/fix `desktop-lamour`:
```bash
cd /Users/hai.phan/Desktop/haiphan/desktop-lamour && dotnet build src/DesktopLamour/DesktopLamour.csproj -p:EnableWindowsTargeting=true
```

**Use when:** vừa sửa WPF-only (ViewModel, XAML, DI registration) không đổi contract BE.

### `both` (Default)

Build/fix cả 2, **BE trước WPF** (WPF thường phụ thuộc field/JSON shape đồng bộ tay từ BE DTO).

**Use when:** sau `/ct-be-to-desktop`, thêm field mới trên entity cần cả 2 phía cập nhật (case phổ biến nhất trong repo này).

---

## MODE Options

### `auto` (Default)

Fix all detectable errors automatically.

```yaml
MODE: auto
PROJECT: both
```

**Behavior:**
1. Build project(s) theo `PROJECT`
2. Classify theo `ERROR_PATTERNS.md`
3. Auto-fix errors có confidence ≥ 70% (namespace collision, duplicate definition đã xác nhận, missing interface member, missing using, type mismatch cơ học, XAML resource key sai, EF seed Id collision)
4. Re-build sau mỗi batch, lặp tới `MAX_ITERATIONS`
5. Báo cáo lỗi còn lại (thường là quyết định nghiệp vụ — vd OnDelete behavior, giá trị mặc định field mới — không tự đoán)

**Use when:** Sau khi generate/scaffold code xong, trước khi báo "hoàn thành" cho user.

---

### `targeted`

Fix only specific error types.

```yaml
MODE: targeted
PROJECT: be
ERROR_TYPES: namespace_type_collision,duplicate_definition
```

**Available ERROR_TYPES:**
- `namespace_type_collision` (CS0118)
- `duplicate_definition` (CS0101/CS0111)
- `ambiguous_reference` (CS0104)
- `missing_interface_member` (CS0535)
- `missing_required_argument` (CS7036/CS1729/CS9035)
- `type_mismatch` (CS0029/CS1503)
- `missing_using_or_typo` (CS0246/CS0103)
- `missing_member` (CS1061)
- `di_resolution_failure` (runtime)
- `ef_migration_seed_collision` (Npgsql 23505)
- `xaml_resource_not_found` (XamlParseException)

**Use when:** Biết chính xác loại lỗi đang gặp (vd vừa đổi tên 1 feature namespace, chỉ cần rà `namespace_type_collision`/`ambiguous_reference`).

---

### `dry-run`

Scan and report only — no actual fixes applied.

```yaml
MODE: dry-run
PROJECT: both
```

**Behavior:**
1. Build project(s)
2. Classify errors
3. Show what WOULD be fixed + confidence
4. Không sửa file nào

**Use when:** Audit trước 1 refactor lớn (vd redesign form nhiều field như popup "Sửa Vật tư hàng hoá") để ước lượng blast radius trước khi commit vào việc fix.

---

## MAX_ITERATIONS

```yaml
MODE: auto
MAX_ITERATIONS: 5
```

**Default:** 3

**Iteration flow:**
```
Iteration 1: dotnet build → 9 lỗi → fix namespace collision + duplicate definition → 4 lỗi còn lại
Iteration 2: dotnet build → 4 lỗi → fix missing interface member + missing using → 1 lỗi còn lại
Iteration 3: dotnet build → 1 lỗi (OnDelete behavior — cần quyết định nghiệp vụ) → dừng, báo cáo
```

Nếu sau `MAX_ITERATIONS` vẫn còn lỗi → dừng và báo cáo cho user, không lặp vô hạn.

---

## Output Schema

```yaml
status: success | partial | failed
project: be | wpf | both
initial_error_count: int
final_error_count: int
iterations_used: int
fixed_errors:
  - category: string
    count: int
    files: [string]
remaining_errors:
  - file: string
    line: int
    code: string        # CS-number nếu có
    message: string
    category: string
    auto_fixable: bool
    suggestion: string
```

### Example Output

```yaml
status: success
project: both
initial_error_count: 9
final_error_count: 0
iterations_used: 2
fixed_errors:
  - category: namespace_type_collision
    count: 3
    files:
      - Features/Warehouses/Repositories/IWarehouseRepository.cs
      - Features/Warehouses/UseCases/GetWarehousesUseCase.cs
      - Features/Warehouses/UseCases/CreateWarehouseUseCase.cs
  - category: duplicate_definition
    count: 1
    files:
      - Configurations/WarehouseConfiguration.cs   # xoá, dùng lại bản trong WarehouseReceiptConfiguration.cs
  - category: missing_interface_member
    count: 1
    files:
      - Domain/Models/AccountSetting.cs
remaining_errors: []
```

---

## Error Handling

### Build output không parse được

```yaml
status: failed
error: "Không parse được output của `dotnet build`. Kiểm tra format MSBuild có đổi không."
```

### MAX_ITERATIONS exceeded

```yaml
status: partial
message: "Max iterations (3) reached. 1 lỗi còn lại chưa fix."
remaining_errors: [...]
```

### Không có lỗi nào

```yaml
status: success
message: "Build sạch. Không có lỗi cần fix."
initial_error_count: 0
final_error_count: 0
```
