# Execution Workflow — Build Doctor (BE + WPF)

## Prerequisites

Skill này chạy **SAU KHI** generate code thành công (entity mới, UseCase mới, feature wiring `/ct-be-to-desktop`, redesign form lớn) — trước khi báo "xong" cho user.

## Stage 0: Determine Project Scope

Đọc `PROJECT` từ input (`be` | `wpf` | `both`, default `both`). Xác định root:

| PROJECT | Root | Build command |
|---|---|---|
| `be` | `/Users/hai.phan/Desktop/haiphan/be-window-lamour` | `dotnet build` |
| `wpf` | `/Users/hai.phan/Desktop/haiphan/desktop-lamour` | `dotnet build src/DesktopLamour/DesktopLamour.csproj -p:EnableWindowsTargeting=true` (build trên Mac — chỉ là compile-check proxy, không chạy được app; app thật chạy trên UTM Windows VM) |

Nếu vừa sửa cả BE lẫn WPF trong cùng 1 task (ví dụ: thêm field Product cần cả 2 phía) → luôn dùng `PROJECT: both`.

## Stage 1: Build & Collect Errors

```bash
# BE
cd /Users/hai.phan/Desktop/haiphan/be-window-lamour
dotnet build 2>&1 | tee /tmp/be_build.log

# WPF (chỉ build project chính, KHÔNG build cả solution —
# tests/DesktopLamour.Tests thường fail restore xunit độc lập với lỗi code chính,
# đừng nhầm đó là lỗi cần fix)
cd /Users/hai.phan/Desktop/haiphan/desktop-lamour
dotnet build src/DesktopLamour/DesktopLamour.csproj -p:EnableWindowsTargeting=true 2>&1 | tee /tmp/wpf_build.log
```

Parse output, trích mỗi lỗi thành:
```yaml
file: <path>
line: <int>
column: <int>
code: <CSxxxx>
message: <string>
project: be | wpf
```

Dòng lỗi dạng: `<file>(<line>,<col>): error <CODE>: <message> [<csproj>]`

## Stage 2: Classify Errors

Match mỗi lỗi với pattern trong `ERROR_PATTERNS.md` theo `code` (CS-number) trước, fallback theo message text nếu code không đủ phân biệt (vd `CS0111` dùng chung cho nhiều tình huống).

| Code | Category | Auto-fixable |
|---|---|---|
| `CS0118` | `namespace_type_collision` | ✅ Yes |
| `CS0101` / `CS0111` | `duplicate_definition` | ⚠️ Depends (đọc kỹ trước — có thể là định nghĩa đã tồn tại, đừng tự tạo bản trùng) |
| `CS0104` | `ambiguous_reference` | ✅ Yes |
| `CS0535` | `missing_interface_member` | ✅ Yes |
| `CS7036` / `CS1729` / `CS9035` | `missing_required_argument` | ✅ Yes |
| `CS0029` / `CS1503` | `type_mismatch` | ✅ Yes |
| `CS0246` / `CS0103` | `missing_using_or_typo` | ⚠️ Depends (missing using vs typo — xem Stage 3.1) |
| `CS1061` | `missing_member` | ⚠️ Depends |
| Runtime `InvalidOperationException: Unable to resolve service for type` | `di_resolution_failure` | ✅ Yes |
| Npgsql `23505 duplicate key` khi `dotnet ef database update` | `ef_migration_seed_collision` | ✅ Yes |
| `XamlParseException` / `Cannot find resource named` | `xaml_resource_not_found` | ✅ Yes |

## Stage 3: Auto-Fix by Category

### 3.1. Namespace/Type Collision (CS0118)

**Triệu chứng thật đã gặp:** tạo feature `Features.Warehouses` (plural) mới, file trong đó dùng bare `Warehouse` — compiler resolve nhầm thành namespace lồng nhau `Features.Warehouse` (singular, feature cũ) vì namespace-lookup ưu tiên declaration trong enclosing scope hơn `using` directive.

**Chiến lược:**
```
1. Đọc full message: "'X' is a namespace but is used like a type"
2. grep toàn repo tìm namespace nào đang dùng đúng tên X:
   grep -rn "^namespace.*\.X\b" --include=*.cs
3. Nếu conflict là do 1 feature namespace CON trỏ đúng tên loại đang cần —
   KHÔNG đổi tên namespace cũ. Chọn 1 trong 2:
   a) Đổi tên type/class ở feature MỚI sang tên không đụng (khuyến nghị nếu
      X còn được dùng làm tên biến/property ở nhiều chỗ trong feature mới —
      xem incident thật: model WPF đổi Warehouse → WarehouseSetting)
   b) Dùng type alias tại đúng file bị lỗi:
      using XEntity = Full.Namespace.To.X;
      rồi thay mọi tham chiếu bare "X" trong file đó bằng "XEntity"
4. Nếu chọn (a) — đổi tên xuyên suốt cả layer (Model/DTO/Repository/UseCase/
   ViewModel/View) để nhất quán, không chỉ đổi 1 chỗ
5. Rebuild, verify hết CS0118
```

### 3.2. Duplicate Definition (CS0101 / CS0111)

**Triệu chứng thật đã gặp:** tạo `WarehouseConfiguration.cs` mới cho entity `Warehouse` — nhưng class `WarehouseConfiguration` đã tồn tại sẵn, nằm **trong file khác tên** (`WarehouseReceiptConfiguration.cs`, gộp 3 class configuration cùng file).

**Chiến lược — KHÔNG tự động xoá, luôn đọc trước:**
```
1. grep toàn repo tìm định nghĩa khác của type trùng tên:
   grep -rln "class X\b\|interface X\b" --include=*.cs
2. Đọc file tìm được — xác nhận đây đúng là bản trùng (không phải type
   khác cùng tên ở namespace khác — nếu khác namespace thì đây không phải
   lỗi CS0101 thật, xem lại message)
3. Nếu đúng là trùng thật (cùng namespace):
   - Nếu bản cũ đã đầy đủ và đúng cho nhu cầu hiện tại → xoá file/class mới
     tạo, dùng lại bản cũ
   - Nếu bản cũ thiếu (ví dụ chỉ có Configure() cơ bản, thiếu seed data mới
     cần) → EDIT bản cũ để bổ sung, không tạo bản 2
4. Rebuild, verify hết CS0101/CS0111
```

### 3.3. Missing Interface Member (CS0535)

**Triệu chứng thật đã gặp:** tạo model mới implement `ISearchableItem` (interface có `Id`, `Code`, `Name`, `DisplayText`) nhưng chỉ định nghĩa `Code`/`DisplayText`, quên `Name`.

**Chiến lược:**
```
1. Đọc interface definition đầy đủ (thường ở Shared/Controls/ISearchableItem.cs
   phía WPF) → list toàn bộ member bắt buộc
2. Đối chiếu class đang lỗi → tìm member còn thiếu
3. Thêm property/method thiếu:
   - Nếu có field tương đương ngữ nghĩa đã tồn tại (vd AccountSetting.Description
     có thể đóng vai trò Name) → expression-bodied property trả field đó:
     public string Name => Description;
   - Nếu không có field tương đương → thêm field mới hoặc trả string.Empty
     tuỳ ngữ nghĩa, KHÔNG bịa giá trị sai lệch business logic
4. Rebuild, verify hết CS0535
```

### 3.4. Missing Required Argument (CS7036 / CS1729 / CS9035)

**Triệu chứng thật đã gặp:** thêm field mới vào `Product` entity → `CreateProductRequestDto`/`UpdateProductRequestDto` thêm property → tất cả call site cũ (UseCase, ViewModel) thiếu argument.

**Chiến lược:**
```
1. Extract: type đang thiếu argument, tên parameter/property
2. Đọc definition (class/record) → lấy type + có nullable/default không
3. Nếu record dùng positional constructor và bị thêm quá nhiều field
   (>6-8 param) → cân nhắc đề xuất chuyển sang init-property record
   (xem incident thật: CreateProductInput/UpdateProductInput đổi sang
   required + init property khi field tăng từ 13 → 33) — báo cho user
   trước khi refactor lớn kiểu này, đừng tự quyết
4. Nếu chỉ thiếu 1-2 field: set giá trị mặc định an toàn tại call site
   - Required non-nullable value type → 0 / false / string.Empty
   - Required nullable → null (nếu hợp lệ) hoặc giá trị nghiệp vụ đã biết
   - KHÔNG bịa giá trị nghiệp vụ (vd không tự đoán "amount = 500000") —
     nếu không rõ default hợp lý, để trống/null và flag cho user xác nhận
5. Rebuild, verify
```

### 3.5. Type Mismatch (CS0029 / CS1503)

```
1. Extract: actual type, expected type
2. C# conversion table (xem FIX_STRATEGIES.md § Strategy 2)
3. Với FK đổi từ string → int?/int (case rất phổ biến trong repo này khi
   1 field free-text được nâng cấp thành master-data FK — vd Category,
   ProductUnit): KHÔNG xoá field string cũ nếu còn nơi khác đọc trực tiếp
   (vd Product.Unit vẫn giữ nguyên string cho Sales/SalesReturn/WarehouseReceipt,
   chỉ thêm ProductUnitId song song rồi đồng bộ 1 chiều lúc save)
4. Rebuild, verify
```

### 3.6. Missing Using / Typo (CS0246 / CS0103)

```
1. Tìm type/symbol bị báo thiếu
2. grep toàn repo (cả BE và WPF nếu đang cross-feature):
   grep -rn "^namespace .*\.SymbolParentFolder" --include=*.cs
   hoặc: grep -rn "class Symbol\b\|record Symbol\b\|enum Symbol\b"
3. Nếu tìm thấy đúng 1 chỗ định nghĩa → thêm `using <namespace>;` vào đầu file
4. Nếu tên gần giống (Levenshtein ≤ 2) với 1 symbol tồn tại khác → có thể là
   typo, đề xuất sửa tên (không tự sửa nếu < 80% chắc chắn)
5. Nếu KHÔNG tìm thấy ở đâu cả → symbol thật sự chưa được tạo, đây không
   phải lỗi "thiếu using" mà là code sinh thiếu — quay lại bước generate,
   không cố "fix" bằng cách thêm using vô nghĩa
```

### 3.7. DI Resolution Failure (runtime)

**Triệu chứng thật đã gặp:** thêm `IGetWarehouseSettingsUseCase` mới, build C# pass (interface tồn tại, constructor hợp lệ) nhưng chạy app crash `Unable to resolve service for type 'IGetWarehouseSettingsUseCase'` — vì quên đăng ký DI.

**Chiến lược:**
```
1. Xác định project: BE → Program.cs; WPF → HomeServiceCollectionExtensions.cs
   (hoặc RealtimeServiceCollectionExtensions.cs nếu liên quan cache/realtime)
2. grep xem interface đã có dòng AddScoped/AddTransient/AddSingleton chưa:
   grep -n "IGetWarehouseSettingsUseCase" Program.cs
3. Nếu chưa có → thêm đúng theo pattern của các UseCase cùng feature
   (BE dùng AddScoped, WPF dùng AddTransient cho ViewModel/UseCase,
   AddSingleton cho CacheStore)
4. Không thể phát hiện lỗi này qua `dotnet build` — chỉ lộ ra khi RUN app.
   Nếu không chạy được app để test (không có Windows/UTM sẵn), chủ động
   grep-check DI registration cho MỌI interface mới thêm vào constructor
   của UseCase/ViewModel/Controller, coi đây là 1 bước bắt buộc sau generate,
   không chỉ dựa vào build xanh.
```

### 3.8. EF Migration Seed Collision (Npgsql 23505)

**Triệu chứng thật đã gặp:** `HasData` seed `Warehouse { Id = 2 }`/`{ Id = 3 }` nhưng DB local đã có sẵn row `Id = 3` (chèn tay từ trước, không qua migration) → `dotnet ef database update` fail giữa migration (transaction rollback, an toàn nhưng cần fix).

**Chiến lược:**
```
1. TRƯỚC KHI viết HasData với Id cố định cho bảng đã tồn tại data — luôn
   query DB thật trước:
   psql -h localhost -U hai.phan -d lamour_db -c "SELECT id, code FROM <table> ORDER BY id;"
2. Chọn Id trống, không đoán
3. Nếu đã lỡ tạo migration với Id đụng và fail:
   - Sửa lại HasData trong Configuration.cs với Id đúng
   - dotnet ef migrations remove (an toàn vì transaction đã rollback,
     CHƯA có gì apply thành công)
   - dotnet ef migrations add <TênCũ> (tạo lại — Snapshot/Designer file sẽ
     tự đồng bộ đúng, tránh sửa tay Snapshot/Designer vì dễ lệch)
   - dotnet ef database update
4. Verify bằng psql SELECT lại
```

### 3.9. XAML Resource Not Found

**Triệu chứng thật đã gặp:** dùng `Style="{StaticResource AppButton.Secondary.Large}"` nhưng style đó không tồn tại (chỉ có `AppButton.Secondary.Medium`) → `XamlParseException` lúc load Window, KHÔNG lộ ra ở `dotnet build` (build C# vẫn pass, XAML resource lookup là runtime).

**Chiến lược:**
```
1. grep định nghĩa style/converter thật có trong Shared/Styles/*.xaml,
   Shared/AppConverters.xaml:
   grep -n "x:Key=\"AppButton" Shared/Styles/AppButtonStyles.xaml
2. Nếu key dùng trong XAML không khớp key nào tồn tại → sửa về key đúng
   gần nhất (không tự tạo style mới trừ khi user yêu cầu thêm style mới)
3. Converter mới tạo (.cs) PHẢI đăng ký trong AppConverters.xaml mới dùng
   được StaticResource — kiểm tra luôn cả 2 khi thêm converter
4. Vì lỗi này không lộ qua `dotnet build`, sau khi build xanh vẫn nên
   grep chéo mọi StaticResource key dùng trong file .xaml mới sửa/tạo
   đối chiếu với file định nghĩa style/converter tương ứng
```

## Stage 4: Re-build & Iterate

```bash
# Sau mỗi batch fix, build lại đúng project vừa sửa
dotnet build ...

if errors == 0:
    → SUCCESS
elif errors < previous_count:
    → REPEAT Stage 2-3 (còn cascade, tiếp tục fix)
else:
    → STOP, report lỗi còn lại cần tay (không lặp vô hạn)
```

Nếu sửa cả BE và WPF trong cùng task — build và fix **BE trước** (Domain/Application layer là nguồn của DTO/contract), sau đó build WPF (thường phụ thuộc field/shape từ BE qua DTO đồng bộ tay).

## Stage 5: Report Results

```
✅ Build Doctor Report
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Initial errors: 9 (BE: 3, WPF: 6)
🔧 Auto-fixed: 8
❌ Remaining: 1

Fixed breakdown:
  ✅ Namespace/type collision (3 files, BE)
  ✅ Missing interface member (1 file, WPF)
  ✅ Missing using (2 files, WPF)
  ✅ XAML resource not found (1 file, WPF)
  ✅ EF seed collision (1 migration, BE)

Manual fixes needed:
  ❌ ProductConfiguration.cs:45 — OnDelete behavior cho FK mới cần user quyết định
```

## Safety Guardrails

Xem đầy đủ tại `GUARDRAILS.md`. Tóm tắt:

1. **Never delete existing correct definitions** — CS0101 fix phải đọc kỹ, không xoá nhầm bản đang dùng
2. **Backup before fix** — lưu nội dung gốc trước khi sửa
3. **Max 3 iterations** — tránh lặp vô hạn
4. **Preserve Clean Architecture boundaries** (BE) / MVVM boundaries (WPF) — không fix bằng cách nhét logic sai layer
5. **EF migration đã `dotnet ef database update` thành công rồi thì KHÔNG xoá/sửa lại** — chỉ sửa migration còn "sạch" (chưa apply hoặc apply rồi rollback do lỗi)
6. **Flag uncertain fixes** — confidence < 80% thì báo, không tự áp dụng
