# Error Patterns & Classification — C#/.NET (BE + WPF)

## Pattern Matching Rules

`dotnet build` in ra lỗi theo format MSBuild:
```
<file>(<line>,<col>): error <CSxxxx>: <message> [<csproj>]
```

Ưu tiên match theo `CSxxxx` code — ổn định hơn parse message text (message có thể đổi theo ngôn ngữ/SDK version).

---

## 1. Namespace/Type Collision — `CS0118`

### Pattern

```
<file>(<line>,<col>): error CS0118: '<name>' is a namespace but is used like a type
```

### Ví dụ thật (đã xảy ra trong repo này)

```
Repositories/IWarehouseRepository.cs(7,22): error CS0118: 'Warehouse' is a namespace but is used like a type
```

**Nguyên nhân:** feature mới `Lamour.Application.Features.Warehouses` (plural) khai báo type `Warehouse` (bare, via `using Lamour.Domain.Entities;`), nhưng enclosing scope `Lamour.Application.Features` đã có sẵn nested namespace `Warehouse` (singular — feature cũ chứa `GetInventorySummaryUseCase`). C# namespace lookup ưu tiên declaration trong enclosing namespace hơn `using` directive → resolve nhầm.

### Extracted Info

```yaml
category: namespace_type_collision
file: Repositories/IWarehouseRepository.cs
line: 7
name: Warehouse
auto_fixable: true
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 1

---

## 2. Duplicate Definition — `CS0101` / `CS0111`

### Pattern

```
<file>(<line>,<col>): error CS0101: The namespace '<ns>' already contains a definition for '<type>'
<file>(<line>,<col>): error CS0111: Type '<type>' already defines a member called '<member>' with the same parameter types
```

### Ví dụ thật

```
Configurations/WarehouseConfiguration.cs(7,14): error CS0101: The namespace 'Lamour.Infrastructure.Persistence.Configurations' already contains a definition for 'WarehouseConfiguration'
```

**Nguyên nhân:** tạo file `WarehouseConfiguration.cs` mới, nhưng class cùng tên đã tồn tại — nằm **trong file tên khác** (`WarehouseReceiptConfiguration.cs`, gộp 3 configuration class 1 file). Dễ bị miss nếu chỉ tìm file theo tên khớp.

### Extracted Info

```yaml
category: duplicate_definition
file: Configurations/WarehouseConfiguration.cs
existing_definition_file: Configurations/WarehouseReceiptConfiguration.cs   # phải grep mới tìm ra, không suy từ tên file lỗi
auto_fixable: depends   # PHẢI đọc cả 2 định nghĩa trước khi quyết định xoá cái nào
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 2 — **không tự xoá nếu chưa đọc cả 2 bản**

---

## 3. Ambiguous Reference — `CS0104`

### Pattern

```
<file>(<line>,<col>): error CS0104: '<name>' is an ambiguous reference between '<A>' and '<B>'
```

### Ví dụ thật (suýt xảy ra, phát hiện trước khi build)

Nếu 1 file DI registration (`HomeServiceCollectionExtensions.cs`) `using` cả `Features.Warehouse.Data.Repositories` (có `IWarehouseRepository` cho phiếu nhập kho) và `Features.Warehouses.Data.Repositories` (định nghĩa `IWarehouseRepository` trùng tên cho danh mục Kho) cùng lúc → ambiguous khi dùng bare `IWarehouseRepository`.

### Extracted Info

```yaml
category: ambiguous_reference
file: HomeServiceCollectionExtensions.cs
name: IWarehouseRepository
candidate_a: DesktopLamour.Features.HomePage.Warehouse.Data.Repositories.IWarehouseRepository
candidate_b: DesktopLamour.Features.HomePage.Warehouses.Data.Repositories.IWarehouseRepository
auto_fixable: true
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 1 (cùng gốc nguyên nhân với CS0118 — đổi tên type ở 1 trong 2 feature, ưu tiên đổi feature MỚI)

---

## 4. Missing Interface Member — `CS0535`

### Pattern

```
<file>(<line>,<col>): error CS0535: '<type>' does not implement interface member '<interface>.<member>'
```

### Ví dụ thật

```
Domain/Models/AccountSetting.cs(5,31): error CS0535: 'AccountSetting' does not implement interface member 'ISearchableItem.Name'
```

**Nguyên nhân:** `ISearchableItem` (WPF, dùng cho mọi dropdown picker — `AppSearchableComboBox`) yêu cầu `Id`, `Code`, `Name`, `DisplayText`. Model mới chỉ định nghĩa `Code`/`DisplayText`, quên `Name`.

### Extracted Info

```yaml
category: missing_interface_member
file: Domain/Models/AccountSetting.cs
type: AccountSetting
interface: ISearchableItem
member: Name
auto_fixable: true
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 3

---

## 5. Missing Required Argument — `CS7036` / `CS1729` / `CS9035`

### Pattern

```
<file>(<line>,<col>): error CS7036: There is no argument given that corresponds to the required formal parameter '<param>' of '<method>'
<file>(<line>,<col>): error CS1729: '<type>' does not contain a constructor that takes <N> arguments
<file>(<line>,<col>): error CS9035: Required member '<type>.<member>' must be set in the object initializer
```

### Ví dụ thật (dạng sẽ gặp)

Sau khi thêm ~20 field mới vào `CreateProductInput`/`UpdateProductInput` (đổi từ positional record sang record với `required` + `init` property), mọi call site cũ dùng object-initializer thiếu field bắt buộc sẽ báo `CS9035` cho mỗi property `required` chưa set.

### Extracted Info

```yaml
category: missing_required_argument
file: ProductFormViewModel.cs
param: Code
type: CreateProductInput
auto_fixable: true
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 4

---

## 6. Type Mismatch — `CS0029` / `CS1503`

### Pattern

```
<file>(<line>,<col>): error CS0029: Cannot implicitly convert type '<actual>' to '<expected>'
<file>(<line>,<col>): error CS1503: Argument <N>: cannot convert from '<actual>' to '<expected>'
```

### Ví dụ thật (dạng sẽ gặp)

Nâng cấp `Product.Unit` (string free-text) thêm `ProductUnitId` (int? FK) song song — nếu lỡ gán `ProductUnitId = someProductUnit` (object) thay vì `someProductUnit.Id` (int) → CS1503.

### Extracted Info

```yaml
category: type_mismatch
actual_type: ProductUnit
expected_type: int?
auto_fixable: true
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 2

---

## 7. Missing Using / Typo — `CS0246` / `CS0103`

### Pattern

```
<file>(<line>,<col>): error CS0246: The type or namespace name '<symbol>' could not be found (are you missing a using directive or an assembly reference?)
<file>(<line>,<col>): error CS0103: The name '<symbol>' does not exist in the current context
```

### Ví dụ thật (dạng sẽ gặp)

`ProductFormViewModel.cs` inject `IGetWarehouseSettingsUseCase` (feature `Warehouses` mới) — nếu quên `using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;` → CS0246.

### Symbol → Namespace Mapping (rút ra từ 2 repo này)

| Symbol pattern | Namespace cần `using` |
|---|---|
| `IGet*UseCase`, `ICreate*UseCase`, ... trong feature X | `[Lamour.Application \| DesktopLamour.Features.HomePage].<X>.[Domain\|Application].UseCases` |
| `ISearchableItem` | `DesktopLamour.Shared.Controls` |
| `ValidationException`, `DomainException`, `NotFoundException` | WPF: `DesktopLamour.Core.Exceptions`; BE: `Lamour.Domain.Exceptions` |
| `INotificationBroadcaster` | `Lamour.Application.Abstractions` (BE only — WPF không có khái niệm này, nhận qua SignalR client trong `Features/Realtime/`) |
| `[ObservableProperty]`, `[RelayCommand]` | `CommunityToolkit.Mvvm.ComponentModel` / `CommunityToolkit.Mvvm.Input` |
| `MessageBox`, `Window` | `System.Windows` |

### Extracted Info

```yaml
category: missing_using_or_typo
file: ProductFormViewModel.cs
symbol: IGetWarehouseSettingsUseCase
required_using: DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases
auto_fixable: true
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 5

---

## 8. Missing Member — `CS1061`

### Pattern

```
<file>(<line>,<col>): error CS1061: '<type>' does not contain a definition for '<member>' and no accessible extension method '<member>' accepting a first argument of type '<type>' could be found
```

### Ví dụ (dạng sẽ gặp)

Đổi `Product.Unit` từ property độc lập thành derive từ `ProductUnit.Name` ở đâu đó rồi quên là `Unit` vẫn tồn tại — hoặc gọi `SelectedProductUnit.Code` khi `ISearchableItem` không có field đó active cho model đó.

### Fix Strategy
→ Đọc lại definition thật của type, xác nhận member đã đổi tên/xoá hay chưa từng tồn tại. Nếu tìm thấy member tên gần giống → sửa tên gọi. Nếu member thực sự chưa có → đây là thiếu code sinh (quay lại bước generate), không phải thiếu using.

---

## 9. DI Resolution Failure (Runtime)

### Pattern

```
System.InvalidOperationException: Unable to resolve service for type '<Interface>' while attempting to activate '<Consumer>'.
```

**Không xuất hiện trong `dotnet build`** — chỉ lộ khi chạy app (`dotnet run` BE, hoặc app WPF chạy trên UTM). Vì môi trường dev thường không chạy được WPF trực tiếp trên Mac, coi đây là checklist bắt buộc song song với build, không chỉ dựa build xanh.

### Extracted Info

```yaml
category: di_resolution_failure
interface: IGetWarehouseSettingsUseCase
consumer: ProductFormViewModel
project: wpf
registration_file: HomeServiceCollectionExtensions.cs   # hoặc Program.cs nếu BE
auto_fixable: true
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 6

---

## 10. EF Migration Seed Collision

### Pattern

```
Npgsql.PostgresException (0x80004005): 23505: duplicate key value violates unique constraint "PK_<table>"
```

Xảy ra khi `dotnet ef database update` chạy `HasData` insert.

### Ví dụ thật

```
INSERT INTO warehouses (id, code, is_active, name) VALUES (2, 'HH', ...), (3, 'TB', ...);
→ 23505: duplicate key value violates unique constraint "PK_warehouses"
```

**Nguyên nhân:** DB local đã có row `Id=3` (`KHO02`, chèn tay từ trước, không qua `HasData`/migration nào) — `HasData` mới chọn Id=2/3 mà không check trước.

### Extracted Info

```yaml
category: ef_migration_seed_collision
table: warehouses
colliding_id: 3
auto_fixable: true
note: "Transaction tự rollback khi lỗi giữa migration — DB KHÔNG bị hỏng dở, an toàn để sửa lại và chạy lại"
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 7

---

## 11. XAML Resource Not Found (Runtime)

### Pattern

```
System.Windows.Markup.XamlParseException: 'Cannot find resource named '<Key>'. Resource names are case sensitive.'
```

**Không xuất hiện trong `dotnet build`** — XAML `StaticResource` resolve lúc runtime (khi Window/UserControl load).

### Ví dụ thật (dạng sẽ gặp)

```xml
<controls:AppButton Style="{StaticResource AppButton.Secondary.Large}" .../>
```
nhưng `AppButtonStyles.xaml` chỉ định nghĩa `AppButton.Secondary.Medium` — không có bản `.Large`.

### Extracted Info

```yaml
category: xaml_resource_not_found
file: ProductFormWindow.xaml
key: AppButton.Secondary.Large
available_keys: [AppButton.Primary.Small, AppButton.Primary.Medium, AppButton.Primary.Large, AppButton.Secondary.Medium, AppButton.Tertiary.Medium, AppButton.Destructive.Medium]
auto_fixable: true
```

### Fix Strategy
→ `FIX_STRATEGIES.md` § Strategy 8

---

## Error Priority for Fixing

Fix theo thứ tự này để tránh cascade errors và tránh sửa nhầm khi lỗi gốc chưa rõ:

1. **Duplicate definition** (CS0101/CS0111) — phải xác nhận bản nào giữ trước khi làm gì khác, vì các lỗi sau có thể chỉ là hệ quả
2. **Namespace/type collision** (CS0118) + **Ambiguous reference** (CS0104) — cùng gốc, fix cùng lúc
3. **Missing using / typo** (CS0246/CS0103) — thường gây ra thêm CS0103 dây chuyền
4. **Missing interface member** (CS0535)
5. **Missing required argument** (CS7036/CS1729/CS9035)
6. **Type mismatch** (CS0029/CS1503)
7. **Missing member** (CS1061) — thường là dấu hiệu code sinh thiếu, ưu tiên thấp vì cần đọc kỹ hơn là auto-fix máy móc
8. **DI resolution failure** — check song song với build, không phụ thuộc thứ tự trên
9. **EF seed collision** / **XAML resource not found** — check riêng vì không lộ qua `dotnet build`

---

## Confidence Scoring

| Confidence | Action |
|---|---|
| 90-100% | Auto-fix immediately (CS0118 rõ nguồn, CS0246 tìm thấy đúng 1 namespace khớp, XAML key gần đúng duy nhất) |
| 70-89% | Auto-fix + log warning (CS0535 dùng field tương đương làm default, CS7036 set default an toàn) |
| 50-69% | Suggest fix, require confirmation (CS0101 chưa rõ bản nào nên giữ, quyết định OnDelete FK) |
| < 50% | Skip, report for manual intervention (giá trị nghiệp vụ mặc định không rõ, refactor kiến trúc lớn) |
