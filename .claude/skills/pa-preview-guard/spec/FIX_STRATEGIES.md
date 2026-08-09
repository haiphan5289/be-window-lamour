# Fix Strategies — Build Doctor (BE + WPF)

Chi tiết chiến lược auto-fix cho mỗi category, viết cho C#/.NET (BE ASP.NET Core + WPF MVVM), không phải Swift.

---

## Strategy 1: Namespace/Type Collision & Ambiguous Reference (CS0118 / CS0104)

### Input

```yaml
category: namespace_type_collision
file: Features/Warehouses/Repositories/IWarehouseRepository.cs
name: Warehouse
colliding_namespace: Lamour.Application.Features.Warehouse   # namespace lồng đã tồn tại
```

### Execution Steps

```
1. Xác nhận nguồn collision:
   grep -rn "^namespace" --include=*.cs | grep -i "\.Warehouse\b"
   → tìm mọi namespace kết thúc đúng bằng tên đang bị nhầm

2. Quyết định hướng fix — 2 lựa chọn, ưu tiên theo tình huống:

   (A) Type alias tại chỗ (nhanh, ít file, dùng khi type đó CHỈ bị
       reference ở vài file):
       using WarehouseEntity = Lamour.Domain.Entities.Warehouse;
       → thay mọi "Warehouse" bare trong file đó thành "WarehouseEntity"
       Đã dùng thật cho: IWarehouseRepository.cs, GetWarehousesUseCase.cs,
       CreateWarehouseUseCase.cs (BE) — chỉ 3 file bị ảnh hưởng trực tiếp.

   (B) Đổi tên type ở feature MỚI (triệt để hơn, dùng khi type đó sẽ
       được dùng lặp lại xuyên nhiều layer — Model/DTO/Repository/UseCase/
       ViewModel/View — vì alias local phải lặp lại ở MỌI file, dễ quên):
       Đã dùng thật cho: WPF model Warehouse → WarehouseSetting
       (kèm IWarehouseRepository → IWarehouseSettingRepository,
       WarehouseService → WarehouseSettingService, ...) vì namespace
       Warehouse (singular, feature cũ) ĐÃ có sẵn IWarehouseRepository/
       WarehouseRepository/IWarehouseService/WarehouseService — alias
       không đủ, phải đổi tên type để tránh CS0104 ambiguous ở
       HomeServiceCollectionExtensions.cs (file using cả 2 namespace).

3. Nếu chọn (B): đổi tên nhất quán ở TẤT CẢ file trong feature mới —
   Domain Model, DTOs (không bắt buộc, DTO field JSON không cần đổi),
   Repository interface+impl, UseCase interface+impl x4, ViewModel x2,
   View x2, DI registration, Navigation route nếu có. Dùng grep để
   không bỏ sót:
   grep -rln "\bWarehouse\b" Features/Warehouses/

4. Rebuild, verify hết CS0118/CS0104
```

### Quyết định nhanh: (A) hay (B)?

| Điều kiện | Chọn |
|---|---|
| Type chỉ bị reference ở ≤ 3 file, không lan sang layer khác | (A) alias |
| Type sẽ là Model/Entity chính của 1 feature mới, dùng ở Repository+UseCase+ViewModel+View | (B) đổi tên |
| Namespace cũ đã có SẴN type CÙNG TÊN với chức năng tương tự (không chỉ namespace lồng) | (B) đổi tên — alias không giải quyết được ambiguous reference |

---

## Strategy 2: Type Mismatch & Conversion (CS0029 / CS1503)

### Input

```yaml
category: type_mismatch
actual_type: string
expected_type: int?
context: FK mới thêm song song field string cũ
```

### Conversion Table (C#)

| From | To | Code |
|---|---|---|
| `string` | `int` | `int.Parse(value)` hoặc `int.TryParse(value, out var n) ? n : 0` |
| `string` | `int?` | `int.TryParse(value, out var n) ? n : (int?)null` |
| `string` | `decimal` | `decimal.TryParse(value, out var d) ? d : 0m` |
| `int` | `string` | `value.ToString()` |
| Model object | `int` (FK Id) | `model.Id` — KHÔNG gán cả object vào field int |
| `enum` | `string` | `value.ToString()` |
| `string` | `enum` | `Enum.TryParse<TEnum>(value, out var result) ? result : default` |
| `T` | `T?` | Gán trực tiếp, tự box được |
| `T?` | `T` | `value ?? default` hoặc `value!` nếu chắc chắn not-null |

### Pattern hay gặp trong repo này: nâng cấp free-text field lên FK

Khi 1 field string tự do (vd `Product.Unit`, `Product.Category` cũ) được nâng lên thành FK có bảng master-data riêng (`ProductUnit`, `Category`):

```csharp
// KHÔNG xoá field string cũ nếu nơi khác đang đọc trực tiếp (check trước
// bằng grep):
grep -rln "\.Unit\b" --include=*.cs | grep -v ProductUnit

// Thêm FK mới song song, đồng bộ 1 chiều lúc save (UseCase):
Unit = productUnit?.Name ?? request.Unit,   // ĐVT chính nếu chọn thì override,
                                              // không thì giữ giá trị cũ
ProductUnitId = request.ProductUnitId,
```

Đây là chiến lược đã áp dụng thật cho `Product.Unit` (giữ) + `Product.ProductUnitId` (thêm mới) — tránh phải sửa toàn bộ `Sales`/`SalesReturn`/`WarehouseReceipt` đang đọc `product.Unit` string trực tiếp.

---

## Strategy 3: Missing Interface Member (CS0535)

### Input

```yaml
category: missing_interface_member
type: AccountSetting
interface: ISearchableItem
member: Name
```

### `ISearchableItem` full contract (WPF)

```csharp
// Shared/Controls/ISearchableItem.cs
public interface ISearchableItem
{
    int     Id          { get; }
    string  Code        { get; }
    string  Name        { get; }
    string  DisplayText { get; }
    string? Phone => null;                    // default, không bắt buộc override
    string  DropdownText => ...;               // default, không bắt buộc override
}
```

### Execution Steps

```
1. Đọc ISearchableItem.cs → list member KHÔNG có default implementation
   (Id, Code, Name, DisplayText — 4 member bắt buộc)
2. Đọc class đang lỗi → đối chiếu member nào chưa có
3. Với member thiếu, tìm field ngữ nghĩa gần nhất đã có trong class:
   - AccountSetting có Code + Description, không có "Name" riêng
     → Name nên trả field đóng vai trò tên hiển thị:
       public string Name => Description;
4. Nếu KHÔNG có field tương đương nào hợp lý — hỏi user field nào nên
   đóng vai trò đó, đừng tự bịa (vd trả string.Empty sẽ làm dropdown
   hiển thị rỗng, che mất lỗi thật)
```

### Ví dụ thật đã fix

```csharp
// Trước (lỗi CS0535)
public class AccountSetting : ISearchableItem
{
    public int    Id          { get; set; }
    public string Code        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DisplayText => $"{Code} — {Description}";
}

// Sau
public class AccountSetting : ISearchableItem
{
    public int    Id          { get; set; }
    public string Code        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Name        => Description;   // ← thêm, thoả ISearchableItem
    public string DisplayText => $"{Code} — {Description}";
}
```

---

## Strategy 4: Missing Required Argument (CS7036 / CS1729 / CS9035)

### Input

```yaml
category: missing_required_argument
type: CreateProductInput
missing_member: Code
call_site: ProductFormViewModel.cs SaveAsync()
```

### Execution Steps

```
1. Đọc definition record/class đang lỗi → list toàn bộ member 'required'
   hoặc positional-constructor param
2. Đọc call site → so khớp member nào đã set, member nào thiếu
3. Với record dùng object-initializer (`new X { A = ..., B = ... }`):
   thêm dòng thiếu, set giá trị đúng từ context (biến ViewModel tương ứng,
   KHÔNG bịa giá trị)
4. Nếu constructor positional bị thêm quá nhiều param theo thời gian
   (repo này: CreateProductInput đi từ 13 → nếu thêm > ~15 param nữa sẽ
   rất khó đọc/dễ nhầm thứ tự) — ĐỀ XUẤT cho user chuyển sang init-property
   record:

   // Trước — positional, dễ nhầm thứ tự khi > 8 param
   public record CreateProductInput(string Code, string Name, int CategoryId, ...);

   // Sau — mỗi field explicit tên, thêm field mới không ảnh hưởng call site cũ
   // (miễn field mới có default hoặc optional)
   public sealed record CreateProductInput
   {
       public required string Code { get; init; }
       public required string Name { get; init; }
       ...
       public string? NewOptionalField { get; init; }   // field mới, optional
   }

   Refactor này ĐÃ làm thật cho CreateProductInput/UpdateProductInput khi
   field tăng từ 13 → 33. English rule of thumb: > 8 constructor param
   positional → cân nhắc init-property. Nhưng đây là refactor có blast
   radius (đổi mọi call site) — LUÔN hỏi user trước khi tự quyết đổi kiểu
   record, đừng tự làm ngầm trong lúc "chỉ fix lỗi build".
```

---

## Strategy 5: Missing Using / Typo (CS0246 / CS0103)

### Input

```yaml
category: missing_using_or_typo
file: ProductFormViewModel.cs
symbol: IGetWarehouseSettingsUseCase
```

### Execution Steps

```
1. grep tìm định nghĩa thật của symbol:
   grep -rn "interface IGetWarehouseSettingsUseCase\|class IGetWarehouseSettingsUseCase" --include=*.cs

2. Nếu tìm thấy đúng 1 nơi → lấy namespace khai báo, thêm using:
   using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

3. Nếu tìm thấy 0 kết quả:
   → symbol chưa từng được tạo. KHÔNG cố "fix" bằng cách thêm using tới
     namespace gần giống — quay lại kiểm tra bước generate code trước đó,
     có thể quên tạo file interface/class.

4. Nếu tìm thấy > 1 kết quả (trùng tên ở 2 feature khác nhau — chính là
   dấu hiệu Strategy 1 namespace collision, không phải thiếu using đơn
   thuần) → chuyển sang xử lý theo Strategy 1.

5. Nếu là typo (Levenshtein ≤ 2 với 1 symbol có thật, không tìm thấy
   symbol y hệt) → sửa tên gọi thay vì thêm using. Chỉ auto-fix nếu
   similarity ≥ 80%.
```

### Import order convention (C#)

Không có convention cứng như Swift (Foundation → Combine → SwiftUI). Repo này thường theo thứ tự: BCL (`System.*`) → third-party (`Microsoft.*`, `CommunityToolkit.*`) → project namespace, nhưng **không** bắt buộc — chỉ cần build được, đừng tốn công sắp xếp lại import order khi mục tiêu là fix lỗi build.

---

## Strategy 6: DI Resolution Failure (Runtime)

### Input

```yaml
category: di_resolution_failure
interface: IGetWarehouseSettingsUseCase
project: wpf
```

### Execution Steps

```
1. Xác định file đăng ký DI đúng theo project:
   - BE: src/Lamour.Api/Program.cs
   - WPF (feature UseCase/Service/Repository/ViewModel/Window):
     Features/HomePage/HomeServiceCollectionExtensions.cs
   - WPF (cache store dùng cho realtime sync):
     Features/Realtime/RealtimeServiceCollectionExtensions.cs +
     PostLoginSyncService.cs (nhớ cả 2 — cache store phải xuất hiện ở cả
     RealtimeSyncService constructor VÀ PostLoginSyncService warmup list)

2. grep xem đã đăng ký chưa:
   grep -n "IGetWarehouseSettingsUseCase" HomeServiceCollectionExtensions.cs

3. Nếu chưa có, thêm đúng pattern của UseCase cùng feature đã có
   (copy 1 dòng AddTransient/AddScoped cùng feature, đổi tên type):

   BE pattern:
     builder.Services.AddScoped<IXxxUseCase, XxxUseCase>();

   WPF pattern (ViewModel/UseCase/Repository — Transient;
   CacheStore — Singleton; Service — AddHttpClient):
     services.AddTransient<IXxxUseCase, XxxUseCase>();
     services.AddSingleton<IXxxCacheStore, XxxCacheStore>();
     services.AddHttpClient<IXxxService, XxxService>(client => { ... });

4. Đây là lỗi RUNTIME, không lộ qua `dotnet build`. Sau khi build xanh,
   luôn tự grep-check: với MỌI interface mới xuất hiện trong constructor
   của 1 UseCase/ViewModel/Controller mới, xác nhận có đúng 1 dòng đăng ký
   DI tương ứng — coi đây là bước bắt buộc riêng, không gộp vào "build
   thành công là xong".
```

---

## Strategy 7: EF Migration Seed Collision

### Input

```yaml
category: ef_migration_seed_collision
table: warehouses
colliding_id: 3
```

### Execution Steps

```
1. Query DB thật để biết Id nào đang trống:
   psql -h localhost -U hai.phan -d lamour_db \
     -c "SELECT id, code, name FROM warehouses ORDER BY id;"

2. Sửa lại HasData trong Configuration.cs, chọn Id không đụng:
   new Warehouse { Id = 4, Code = "HH", ... },
   new Warehouse { Id = 5, Code = "TB", ... }

3. Migration BỊ LỖI GIỮA CHỪNG thì transaction đã tự rollback — KHÔNG có
   gì apply thành công, an toàn để xoá migration file vừa tạo và tạo lại
   (không cần sửa tay Snapshot/Designer, dễ lệch):

   export PATH="$PATH:$HOME/.dotnet/tools"
   dotnet ef migrations remove --project src/Lamour.Infrastructure --startup-project src/Lamour.Api
   dotnet ef migrations add <TênMigrationCũ> --project src/Lamour.Infrastructure --startup-project src/Lamour.Api
   dotnet ef database update --project src/Lamour.Infrastructure --startup-project src/Lamour.Api

4. Verify:
   psql -h localhost -U hai.phan -d lamour_db \
     -c "SELECT id, code FROM warehouses WHERE code IN ('HH','TB');"
```

### Quan trọng — phân biệt với migration ĐÃ apply thành công

Nếu migration đã `dotnet ef database update` thành công (không lỗi) rồi mới phát hiện vấn đề khác (không phải seed collision) — **KHÔNG** `migrations remove` nữa, vì đã có dòng trong `__EFMigrationsHistory` và schema thật đã đổi. Thay vào đó tạo **migration mới** để sửa tiếp. `migrations remove` chỉ an toàn khi migration đó CHƯA từng apply thành công lần nào.

---

## Strategy 8: XAML Resource Not Found

### Input

```yaml
category: xaml_resource_not_found
file: ProductFormWindow.xaml
key: AppButton.Secondary.Large
```

### Execution Steps

```
1. grep toàn bộ key thật có trong file style/converter tương ứng:
   grep -n "x:Key=\"AppButton" Shared/Styles/AppButtonStyles.xaml
   grep -n "x:Key=" Shared/AppConverters.xaml

2. So khớp key đang dùng trong XAML lỗi với list key thật:
   - Nếu có key gần giống (chỉ khác size/variant, vd .Large vs .Medium)
     → đổi XAML về key đúng đang tồn tại
   - Nếu không có key nào gần giống và đây là 1 CONVERTER mới tự viết
     (.cs file mới) → kiểm tra đã đăng ký trong AppConverters.xaml chưa,
     thêm dòng đăng ký nếu thiếu:
     <converters:XxxDisplayConverter x:Key="XxxDisplayConverter"/>

3. Nếu user THỰC SỰ cần 1 style/variant mới chưa tồn tại (vd cần
   AppButton.Secondary.Large thật) — đây không phải lỗi cần "fix" mà là
   thiếu 1 style cần thêm mới vào AppButtonStyles.xaml. Hỏi user trước
   khi tự thêm style mới vào design system chung (ảnh hưởng toàn app).

4. Vì lỗi này KHÔNG lộ qua `dotnet build`, sau khi build xanh vẫn phải
   tự grep chéo mọi StaticResource key mới dùng trong .xaml vừa sửa/tạo.
```

---

## Fix Priority & Dependencies

```
1. Duplicate definition (CS0101/CS0111)      — xác nhận bản giữ trước khi đụng gì khác
   ↓
2. Namespace/type collision + ambiguous ref  — fix cùng lúc, cùng gốc
   (CS0118 + CS0104)
   ↓
3. Missing using / typo (CS0246/CS0103)      — enables symbol resolution, có thể tự
                                                 giải quyết CS0103 dây chuyền
   ↓
4. Missing interface member (CS0535)
   ↓
5. Missing required argument                  — thường xuất hiện SAU khi model/DTO
   (CS7036/CS1729/CS9035)                       đổi field — fix ở call site, không
                                                 đổi lại model
   ↓
6. Type mismatch (CS0029/CS1503)
   ↓
7. Missing member (CS1061)                    — đọc kỹ, có thể là code sinh thiếu
   ↓
[song song, không lệ thuộc build]
8. DI resolution failure                      — check riêng bằng grep, không chờ build
9. EF migration seed collision                — check riêng khi chạy `dotnet ef database update`
10. XAML resource not found                   — check riêng bằng grep chéo StaticResource

Sau mỗi batch fix, rebuild để xem cascade errors đã hết chưa trước khi
qua batch tiếp theo.
```
