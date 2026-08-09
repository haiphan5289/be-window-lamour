# Examples — Build Doctor (BE + WPF)

Toàn bộ ví dụ dưới đây lấy từ incident **thật** đã xảy ra trong `be-window-lamour`/`desktop-lamour` (không phải case giả định) — khi build 2 danh mục cài đặt mới (Đơn vị tính, Tài khoản kế toán, Kho) và redesign popup "Sửa Vật tư, hàng hoá, dịch vụ".

---

## Example 1: Namespace Collision khi tạo feature `Warehouses` mới (BE)

### Scenario

Tạo feature `Lamour.Application.Features.Warehouses` (plural, CRUD danh mục Kho) trong khi đã có sẵn `Lamour.Application.Features.Warehouse` (singular, feature Tổng hợp tồn kho cũ).

### Input

```yaml
MODE: auto
PROJECT: be
```

### Execution Log

```
🔍 Stage 1: dotnet build...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 8 errors:
  ✗ Repositories/IWarehouseRepository.cs(7,22): CS0118 'Warehouse' is a namespace but is used like a type
  ✗ Repositories/IWarehouseRepository.cs(8,10): CS0118 (same)
  ✗ Repositories/IWarehouseRepository.cs(10,30): CS0118 (same)
  ✗ Repositories/IWarehouseRepository.cs(10,10): CS0118 (same)
  ✗ Repositories/IWarehouseRepository.cs(11,33): CS0118 (same)
  ✗ Repositories/IWarehouseRepository.cs(11,10): CS0118 (same)
  ✗ Repositories/IWarehouseRepository.cs(12,22): CS0118 (same)
  ✗ UseCases/GetWarehousesUseCase.cs(26,51): CS0118 (same)

🔧 Stage 2: Classifying...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  • namespace_type_collision: 8 errors, root cause: bare 'Warehouse'
    resolves to nested namespace 'Lamour.Application.Features.Warehouse'
    thay vì entity 'Lamour.Domain.Entities.Warehouse'

🛠️  Stage 3: Applying fix (type alias — Strategy 1A, vì chỉ 3 file
    Application-layer bị ảnh hưởng, chưa lan ra Infrastructure/Api)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✅ IWarehouseRepository.cs → using WarehouseEntity = Lamour.Domain.Entities.Warehouse;
     thay 7 chỗ bare 'Warehouse' → 'WarehouseEntity'
  ✅ GetWarehousesUseCase.cs → cùng alias, thay 1 chỗ
  ✅ CreateWarehouseUseCase.cs → cùng alias, thay 1 chỗ ('new Warehouse {...}')

✓ Stage 4: Re-building...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Errors: 0

✅ SUCCESS: 8 errors fixed in 1 iteration
```

### Output Report

```yaml
status: success
project: be
initial_error_count: 8
final_error_count: 0
iterations_used: 1
fixed_errors:
  - category: namespace_type_collision
    count: 8
    files:
      - Repositories/IWarehouseRepository.cs
      - UseCases/GetWarehousesUseCase.cs
      - UseCases/CreateWarehouseUseCase.cs
remaining_errors: []
```

---

## Example 2: Duplicate Definition — Configuration đã tồn tại ở file khác tên (BE)

### Scenario

Sau khi fix Example 1, build lại → gặp lỗi khác: tạo mới `Configurations/WarehouseConfiguration.cs`, nhưng `WarehouseConfiguration` **đã tồn tại từ trước**, nằm gộp chung trong `WarehouseReceiptConfiguration.cs` (kèm seed 1 row `KHO01`/"Kho chính" viết tay).

### Execution Log

```
🔍 Stage 1: dotnet build...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 2 errors:
  ✗ Configurations/WarehouseConfiguration.cs(7,14): CS0101 The namespace
    'Lamour.Infrastructure.Persistence.Configurations' already contains
    a definition for 'WarehouseConfiguration'
  ✗ Configurations/WarehouseConfiguration.cs(9,17): CS0111 Type
    'WarehouseConfiguration' already defines a member called 'Configure'

🔧 Stage 2: Classifying...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  • duplicate_definition: 2 errors — KHÔNG tự xoá ngay, grep tìm bản còn lại trước

🛠️  Stage 3: Investigate trước khi fix
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  grep -rln "class WarehouseConfiguration" --include=*.cs
  → tìm thấy: Configurations/WarehouseReceiptConfiguration.cs (định nghĩa
    CÙNG class, đã có Configure() cơ bản + 1 row seed KHO01)

  Đọc cả 2 bản → bản cũ THIẾU 2 row seed mới cần (HH/TB) → quyết định:
  GIỮ bản cũ, EDIT thêm 2 dòng HasData, XOÁ file mới tạo trùng.

  ✅ rm Configurations/WarehouseConfiguration.cs (file vừa tạo, trùng)
  ✅ WarehouseReceiptConfiguration.cs → HasData thêm 2 row:
     new Warehouse { Id = 2, Code = "HH", Name = "Hàng hoá", IsActive = true },
     new Warehouse { Id = 3, Code = "TB", Name = "Trưng bày", IsActive = true }

✓ Stage 4: Re-building...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Errors: 0

✅ SUCCESS: 2 errors fixed in 1 iteration (sau investigate thủ công)
```

> **Lưu ý:** bước investigate (grep + đọc cả 2 bản) là **bắt buộc**, không được tự động xoá file mới hay file cũ mà không đọc nội dung — có thể bản MỚI mới là bản đúng/đầy đủ hơn.

---

## Example 3: Missing Interface Member khi build WPF (`ISearchableItem`)

### Scenario

Tạo domain model `AccountSetting` (WPF) implement `ISearchableItem` để dùng cho `AppSearchableComboBox`, nhưng chỉ định nghĩa `Id`/`Code`/`DisplayText`, quên `Name`.

### Input

```yaml
MODE: auto
PROJECT: wpf
```

### Execution Log

```
🔍 Stage 1: dotnet build src/DesktopLamour/DesktopLamour.csproj -p:EnableWindowsTargeting=true...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 1 error:
  ✗ Domain/Models/AccountSetting.cs(5,31): CS0535 'AccountSetting' does
    not implement interface member 'ISearchableItem.Name'

🔧 Stage 2: Classifying...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  • missing_interface_member: 1 error (confidence: 90%)

🛠️  Stage 3: Applying fix...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Đọc AccountSetting.cs → có Code + Description, không có field "Name" riêng.
  Description đóng vai trò gần nhất với "tên hiển thị" → dùng làm Name.

  ✅ AccountSetting.cs → thêm:
     public string Name => Description;

✓ Stage 4: Re-building...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Errors: 0

✅ SUCCESS: 1 error fixed in 1 iteration
```

---

## Example 4: EF Migration Seed Id Collision với data đã chèn tay

### Scenario

Migration `ExtendProductForVTHHForm` seed 2 warehouse mới (`HasData Id=2, Id=3`) — nhưng chạy `dotnet ef database update` fail vì DB local đã có sẵn `Id=3` (`KHO02`, chèn tay từ trước, ngoài migration history).

### Input

```yaml
MODE: auto
PROJECT: be
```

### Execution Log

```
🔍 Stage 1: dotnet ef database update...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Failed executing DbCommand:
  INSERT INTO warehouses (id, code, is_active, name) VALUES (2, 'HH', ...), (3, 'TB', ...);
  → Npgsql.PostgresException 23505: duplicate key value violates unique
    constraint "PK_warehouses"

🔧 Stage 2: Classifying...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  • ef_migration_seed_collision: 1 error
  Transaction đã tự rollback — DB an toàn, chưa có gì ghi thành công.

🛠️  Stage 3: Applying fix...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  psql -c "SELECT id, code FROM warehouses ORDER BY id;"
  → id=1 (KHO01), id=3 (KHO02) đã tồn tại. id=2 trống nhưng id=3 đụng.

  Sửa WarehouseReceiptConfiguration.cs: đổi Id seed 2/3 → 4/5

  ✅ dotnet ef migrations remove   (an toàn — chưa apply thành công lần nào)
  ✅ dotnet ef migrations add ExtendProductForVTHHForm   (tạo lại, Snapshot tự đồng bộ)
  ✅ dotnet ef database update

✓ Stage 4: Verify...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  psql -c "SELECT id, code FROM warehouses ORDER BY id;"
  → 1 KHO01, 3 KHO02, 4 HH, 5 TB  ✅

✅ SUCCESS: migration applied clean
```

---

## Example 5: XAML Resource Not Found (không lộ qua `dotnet build`)

### Scenario

Thêm nút "💾 Cất & Thêm" dùng `Style="{StaticResource AppButton.Secondary.Large}"` — build C# pass bình thường (XAML resource resolve ở runtime), nhưng khi mở `ProductFormWindow` sẽ crash `XamlParseException`.

### Input

```yaml
MODE: dry-run
PROJECT: wpf
```

### Execution Log

```
🔍 Stage 1: dotnet build → 0 lỗi C# (KHÔNG đủ để kết luận XAML sạch)

🔍 Stage 1b: grep chéo StaticResource keys dùng trong file .xaml vừa sửa
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  grep -o "StaticResource [A-Za-z.]*" ProductFormWindow.xaml | sort -u
  → ... AppButton.Secondary.Large ...

  grep "x:Key=\"AppButton" Shared/Styles/AppButtonStyles.xaml
  → AppButton.Primary.Small, .Primary.Medium, .Primary.Large,
    AppButton.Secondary.Medium, AppButton.Tertiary.Medium,
    AppButton.Destructive.Medium
  → 'AppButton.Secondary.Large' KHÔNG tồn tại

📋 Dry-Run Report
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[xaml_resource_not_found] ProductFormWindow.xaml
  Key dùng: AppButton.Secondary.Large (không tồn tại)
  Gần nhất có sẵn: AppButton.Secondary.Medium
  Fix đề xuất: đổi Style về AppButton.Secondary.Medium
  Confidence: 90%

Run với MODE=auto để áp dụng.
```

---

## Example 6: Combined BE + WPF sau `/ct-be-to-desktop` (dạng thường gặp nhất)

### Scenario

Thêm field mới trên `Product` cần cả BE (entity/DTO/UseCase/migration) và WPF (model/DTO/ViewModel/XAML) đồng bộ.

### Input

```yaml
MODE: auto
PROJECT: both
```

### Execution Log

```
🔍 Stage 1: BE — dotnet build
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Errors: 0 (BE luôn build trước vì WPF phụ thuộc field/JSON shape từ đây)

🔍 Stage 1: WPF — dotnet build src/DesktopLamour/DesktopLamour.csproj -p:EnableWindowsTargeting=true
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 3 errors:
  ✗ ProductFormViewModel.cs(29,53): CS0246 'IGetWarehouseSettingsUseCase'
    could not be found
  ✗ ProductFormViewModel.cs(151,45): CS9035 Required member
    'CreateProductInput.Code' must be set

🔧 Stage 2-3: Fix
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✅ Thêm using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
  ✅ Thêm Code = Code.Trim() vào object initializer CreateProductInput

✓ Stage 4: Re-build WPF → 0 errors

✅ SUCCESS: BE 0 lỗi, WPF 2 lỗi fixed trong 1 iteration
```

### Output Report

```yaml
status: success
project: both
initial_error_count: 2
final_error_count: 0
iterations_used: 1
fixed_errors:
  - category: missing_using_or_typo
    count: 1
    files: [ProductFormViewModel.cs]
  - category: missing_required_argument
    count: 1
    files: [ProductFormViewModel.cs]
remaining_errors: []
```
