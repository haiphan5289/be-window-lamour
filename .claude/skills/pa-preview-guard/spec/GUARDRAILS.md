# Guardrails — Build Doctor (BE + WPF)

Safety rules để tránh làm hỏng code hoặc DB khi auto-fix trong `be-window-lamour`/`desktop-lamour`.

---

## Rule 1: Never Delete Without Reading Both Sides First

```
❌ PROHIBITED
Xoá file/class ngay khi thấy CS0101 (duplicate definition) mà chưa đọc
nội dung CẢ HAI định nghĩa.

✅ ALLOWED
grep tìm bản còn lại → đọc cả 2 → xác nhận bản nào đầy đủ/đúng hơn →
giữ 1, xoá/gộp bản kia.
```

**Ví dụ thật:** `WarehouseConfiguration` mới tạo bị trùng bản đã có sẵn trong `WarehouseReceiptConfiguration.cs`. Nếu xoá nhầm bản CŨ (đang có seed `KHO01` đã chạy migration từ trước) thay vì bản MỚI → mất seed data đang production-relevant.

---

## Rule 2: Preserve Clean Architecture / MVVM Boundaries

```
❌ PROHIBITED
Fix lỗi build bằng cách nhét logic sai layer — vd gọi trực tiếp
AppDbContext từ Controller (BE) để né lỗi thiếu UseCase, hoặc gọi
HttpClient trực tiếp từ View code-behind (WPF) để né lỗi DI.

✅ ALLOWED
Fix đúng layer đang lỗi. Nếu thiếu registration DI → thêm DI, không
bypass toàn bộ layer.
```

**BE layer order:** `Api` → `Application` (UseCase/DTO) → `Domain` (Entity, zero deps) → `Infrastructure` (EF Core, Repository). DTO không bao giờ leak ra khỏi `Application`; Controller không gọi thẳng `Infrastructure`.

**WPF layer order:** `Views` (XAML) → `ViewModels` → `Domain/UseCases` → `Data/Repositories` → `Data/Services` (HttpClient) → BE API. ViewModel không gọi thẳng `HttpClient`; View code-behind không chứa business logic.

---

## Rule 3: Migration Đã Apply Thành Công — Không Xoá Lại

```
❌ PROHIBITED
`dotnet ef migrations remove` một migration ĐÃ có dòng trong
"__EFMigrationsHistory" (đã `dotnet ef database update` thành công).

✅ ALLOWED
- Nếu migration fail GIỮA CHỪNG (transaction rollback, không có dòng
  trong __EFMigrationsHistory) → an toàn để remove + tạo lại.
- Nếu migration đã apply thành công rồi mới phát hiện vấn đề khác →
  tạo MIGRATION MỚI để sửa tiếp, không remove migration cũ.
```

**Check trước khi remove:**
```bash
psql -h localhost -U hai.phan -d lamour_db \
  -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 5;"
```
Nếu migration đang định xoá xuất hiện trong list → **KHÔNG xoá**, tạo migration mới thay thế.

---

## Rule 4: Verify DB State Trước Khi Seed Id Cố Định

```
❌ PROHIBITED
Viết `HasData(new X { Id = 2 }, new X { Id = 3 })` cho 1 bảng đã có data
mà không kiểm tra Id nào đang trống trong DB thật.

✅ ALLOWED
Luôn `SELECT id FROM <table> ORDER BY id;` trước khi hardcode Id trong
HasData, đặc biệt với bảng có thể đã bị insert tay ngoài migration
(đã gặp thật: bảng `warehouses` có row `KHO02` không rõ nguồn gốc, không
qua HasData nào).
```

---

## Rule 5: Backup Before Multi-File Rename

```
❌ PROHIBITED
Đổi tên type xuyên nhiều file (Strategy 1B — namespace collision) mà
không track được đã sửa file nào, dễ bỏ sót gây lỗi cascade mới.

✅ ALLOWED
grep liệt kê TOÀN BỘ file bị ảnh hưởng trước khi bắt đầu đổi tên:
grep -rln "\bOldName\b" Features/FeatureName/
→ sửa từng file, rebuild sau mỗi vài file để bắt lỗi sớm thay vì đổi
hết 15 file rồi mới build 1 lần (khó debug lỗi nào gây ra bởi file nào).
```

---

## Rule 6: Confidence Threshold

```
❌ PROHIBITED
Auto-fix khi confidence < 70% (đặc biệt: giá trị mặc định nghiệp vụ,
quyết định OnDelete behavior cho FK mới, chọn field nào đóng vai trò
ISearchableItem.Name khi không có field nào rõ ràng tương đương).

✅ ALLOWED
Auto-fix chỉ khi confidence ≥ 70%. Dưới ngưỡng → suggest + hỏi user,
không tự áp dụng.
```

| Confidence | Action |
|---|---|
| 90-100% | Auto-fix ngay |
| 70-89% | Auto-fix + log rõ lý do |
| 50-69% | Đề xuất, chờ xác nhận |
| < 50% | Bỏ qua, báo cáo cần tay |

---

## Rule 7: Max Iterations Limit

```
❌ PROHIBITED
Lặp vô hạn build → fix → build khi lỗi không giảm hoặc lỗi mới xuất
hiện liên tục.

✅ ALLOWED
Max 3 iterations (mặc định, configurable qua MAX_ITERATIONS). Nếu sau
1 iteration mà error count KHÔNG giảm hoặc TĂNG → dừng ngay, không chờ
hết số iteration còn lại.
```

---

## Rule 8: Type-Safe, Domain-Aware Default Values

```
❌ PROHIBITED
Bịa giá trị mặc định nghiệp vụ khi fix "missing required argument"
(vd tự set `SellingPrice = 500000` cho có).

✅ ALLOWED
- Kiểu giá trị (`string`→`""`, `int`→`0`, `bool`→`false`, `decimal`→`0m`,
  nullable→`null`) → an toàn để auto-fix
- Giá trị NGHIỆP VỤ cụ thể (giá bán, mã tài khoản mặc định, kho mặc
  định) → CHỈ auto-fix khi có nguồn xác nhận rõ ràng (vd user đã cung
  cấp ảnh/spec — như trường hợp default TK kế toán 1561/5111/5211/...
  lấy từ ảnh mẫu MISA user gửi), không tự đoán khi không có căn cứ
```

---

## Rule 9: Validate After Each Batch, Not All-At-Once

```
❌ PROHIBITED
Áp dụng tất cả fix cho mọi category cùng lúc rồi mới build 1 lần.

✅ ALLOWED
Fix theo priority order trong ERROR_PATTERNS.md/FIX_STRATEGIES.md,
rebuild sau mỗi category để xác nhận không phát sinh lỗi mới trước khi
qua category tiếp theo.
```

---

## Rule 10: Respect Existing Code Style & Naming Convention

```
❌ PROHIBITED
"Tiện tay" đổi format, đổi convention đặt tên khi đang fix lỗi khác
(vd đang fix CS0246 lại tiện tay reorder using, đổi 4-space thành tab).

✅ ALLOWED
Chỉ sửa đúng phần gây lỗi. Giữ nguyên style file (BE: PascalCase field
alignment kiểu cột thẳng hàng đã thấy trong DTOs; WPF: XAML indentation
2-space, `<controls:AppXxx>` custom control thay vì raw WPF control).
```

---

## Rule 11: File Paths Must Be Within The 2 Known Repos

```
❌ PROHIBITED
Sửa file ngoài `be-window-lamour/` hoặc `desktop-lamour/`.

✅ ALLOWED
Chỉ động vào 2 workspace:
  /Users/hai.phan/Desktop/haiphan/be-window-lamour/
  /Users/hai.phan/Desktop/haiphan/desktop-lamour/
```

---

## Rule 12: Never Skip Runtime-Only Checks Just Because Build Is Green

```
❌ PROHIBITED
Báo "hoàn thành, build 0 lỗi" và dừng lại, trong khi 2 loại lỗi KHÔNG
lộ qua `dotnet build`:
  - DI resolution failure (chỉ crash lúc chạy app)
  - XAML resource not found (chỉ crash lúc load Window/UserControl)

✅ ALLOWED
Sau khi build xanh, luôn chạy thêm 2 bước check thủ công:
  1. grep mọi interface mới trong constructor UseCase/ViewModel/Controller
     → xác nhận có dòng đăng ký DI tương ứng
  2. grep mọi StaticResource key mới dùng trong .xaml vừa sửa/tạo →
     đối chiếu key thật có trong Shared/Styles/*.xaml, AppConverters.xaml
```

---

## Rule 13: Report All Actions With Before/After Context

```
❌ PROHIBITED
Sửa file lặng lẽ không giải thích.

✅ ALLOWED
Log rõ mỗi thay đổi:

✅ Domain/Models/AccountSetting.cs
   Trước: (không có property Name)
   Sau:   public string Name => Description;
   Lý do: CS0535 — ISearchableItem yêu cầu Name, Description là field
          gần nghĩa nhất đóng vai trò tên hiển thị.
```

---

## Rule 14: Never Modify Business Logic While Fixing Build Errors

```
❌ PROHIBITED
Đổi validation rule, đổi công thức tính toán, đổi business rule trong
lúc chỉ đang fix lỗi compile.

✅ ALLOWED
Chỉ sửa phần syntactic/mechanical để code build được. Nếu phát hiện
business logic có vấn đề trong lúc fix — báo riêng cho user, không tự
sửa gộp vào cùng lúc.
```

**Ví dụ:**
```csharp
// Lỗi: CategoryId type mismatch
var product = new Product(categoryId: category);  // category là object, cần int

// ĐÚNG — chỉ fix type
var product = new Product(categoryId: category.Id);

// SAI — tiện tay "cải thiện" thêm validation không liên quan
var product = new Product(categoryId: category.Id);
if (category.Id <= 0) throw new Exception("Invalid category");  // ❌ không được yêu cầu
```

---

## Rule 15: Fail Safe — Rollback Nếu Fix Không Thành Công

```
❌ PROHIBITED
Để code ở trạng thái half-fixed nếu 1 fix trong batch làm hỏng thêm.

✅ ALLOWED
Nếu 1 fix khiến error count TĂNG so với trước → revert đúng file đó
(dùng `git diff`/`git checkout -- <file>` nếu chưa commit, hoặc khôi
phục nội dung đã đọc trước khi sửa), rebuild lại để xác nhận về trạng
thái trước fix, rồi báo cáo thay vì tiếp tục đè thêm fix khác lên.
```

---

## Prohibited Patterns Summary

| Never | Why |
|---|---|
| Xoá định nghĩa mà chưa đọc cả 2 bản (CS0101) | Có thể mất seed data / logic đang dùng |
| `migrations remove` một migration đã apply thành công | Làm lệch `__EFMigrationsHistory` với schema thật |
| Seed `HasData` Id cố định không check DB trước | Gây `23505 duplicate key` lúc `database update` |
| Bịa giá trị mặc định nghiệp vụ không có căn cứ | Sai dữ liệu nghiệp vụ âm thầm, khó phát hiện |
| Đổi tên type xuyên file mà không grep hết trước | Bỏ sót → lỗi cascade mới, khó truy nguồn |
| Coi build xanh = xong (bỏ qua DI/XAML runtime check) | 2 lớp lỗi phổ biến nhất không lộ qua `dotnet build` |
| Vượt MAX_ITERATIONS | Lặp vô ích, có thể là dấu hiệu lỗi gốc chưa hiểu đúng |
| Sửa file ngoài 2 repo đã biết | Ngoài phạm vi, rủi ro không kiểm soát được |
| Trộn fix build với đổi business logic | Khó review, dễ lẫn lỗi domain vào lỗi compile |
| Để code half-fixed khi 1 fix gây thêm lỗi | Khó debug hơn trạng thái ban đầu |

---

## Red Flags — Stop Immediately

```
🚨 Cùng 1 lỗi vẫn còn sau 2 iteration liên tiếp
🚨 Error count TĂNG sau 1 batch fix
🚨 File mới xuất hiện trong danh sách lỗi (không có ở lần scan đầu)
🚨 Không parse được format lỗi (`dotnet build` đổi output format /
   SDK version khác)
🚨 Fix cần xoá > 10 dòng code không phải do chính tay vừa thêm
🚨 Fix ảnh hưởng > 15 file cùng lúc (namespace rename lớn) — dừng lại
   hỏi user xác nhận hướng đi trước khi làm tiếp, đừng tự chạy hết
🚨 Đang định `dotnet ef migrations remove` một migration đã có trong
   __EFMigrationsHistory
🚨 Confidence < 50% cho > 50% số lỗi còn lại trong 1 batch
```

**Action khi gặp red flag:** dừng, không tiếp tục fix hàng loạt, báo cáo rõ tình huống và hỏi hướng xử lý thay vì tự quyết.
