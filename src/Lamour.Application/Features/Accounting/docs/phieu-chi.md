# Phiếu Chi (Payment) — Feature Document (BE + WPF)

> **Branch:** `dev` | **Created:** 2026-04-29 (parallel to Phiếu Thu) | **Major update:** 2026-08-10 (Draft/Confirm lifecycle, TK Nợ/TK Có → Tài khoản kế toán FK, Khoản mục CP link) | **2026-08-11:** thêm trạng thái `Treo` giữa Draft và Confirmed | **2026-08-26:** "Đối tượng" mở rộng đa loại (Supplier/Customer/Employee) + so ảnh mẫu MISA, fix "Hoàn"/seed Khoản mục CP

---

## So ảnh mẫu MISA — các fix bổ sung (2026-08-26, sau khi thêm "Đối tượng" đa loại)

So `PaymentWindow` với ảnh mẫu MISA phát hiện thêm (đã fix hết, trừ các cột đã quyết định bỏ qua từ trước — xem "Known gaps"):

1. **"Hoàn" (Unconfirm) — hoàn toàn chưa có, đã thêm mới.** `IUnconfirmPaymentUseCase`/`UnconfirmPaymentUseCase` (mirror `UnconfirmWarehouseReceiptUseCase`): guard `Status == Confirmed`, xoá `CashTransaction` đã tạo lúc Confirm qua `ICashLedgerRepository.DeleteByPaymentNumberAsync(payment.DocumentNumber)` (method đã có sẵn, chưa ai gọi tới), set `Status = Treo`, `ConfirmedAt = null`. Endpoint mới `POST {id}/unconfirm`.
2. **Seed "Khoản mục CP" sai/rác — đã seed lại đúng ảnh mẫu.** Migration `SeedExpenseCategories`: insert 8 dòng `01`–`08` (PHÒNG SALES/MARKETING/KHO VẬN/TÀI CHÍNH-KẾ TOÁN/NHÂN SỰ/ĐÀO TẠO/SPA/KHÁC) khớp đúng ảnh mẫu. **Không xoá** dòng rác cũ `id=1, code="111", name="sale"` (dữ liệu người dùng tự tạo qua UI trước đó, không phải seed hệ thống — xoá dữ liệu người dùng không hỏi trước là hành động không nên làm).

Phần còn lại (bỏ combo "Loại đối tượng" thừa, auto-copy Đối tượng xuống dòng hạch toán, thêm cột "Đối tượng" (mã) trong grid, đổi thứ tự cột, context menu Ctrl+Insert/Ctrl+Delete/Ctrl+F trên grid) là các thay đổi WPF-only — xem doc WPF: `desktop-lamour/.../Accounting/docs/phieu-chi.md`.

**Bỏ qua khỏi lần so sánh này:** ảnh "Chứng từ hàng bán bị trả lại" (SalesReturn) gửi kèm không liên quan Phiếu Chi.

---

## "Đối tượng" đa loại (polymorphic) — 2026-08-26

Trước đây "Đối tượng" trên Phiếu Chi **chỉ là Supplier** (`SupplierId` int, FK bắt buộc). Yêu cầu mới: cho phép chọn **Nhà cung cấp / Khách hàng / Nhân viên** làm đối tượng nhận chi.

**Data model — discriminator + cached name** (không dùng 3 cột FK riêng, vì Postgres/EF không hỗ trợ 1 FK trỏ tới nhiều bảng khác nhau):

- `Payment.PartnerType` (`PaymentPartnerType` enum: `Supplier`/`Customer`/`Employee`, lưu `HasConversion<string>()`)
- `Payment.PartnerId` (int) — Id trong bảng tương ứng với `PartnerType`, **không có FK constraint thật** ở DB (không thể FK đa bảng) — validate ở tầng UseCase
- `Payment.PartnerName` (string) — tên đối tượng, **cache tại thời điểm Create/Update**, không tự đồng bộ lại nếu tên gốc đổi sau đó (giống cách `SalesReturnLine.CostPrice` cache `Product.CostPrice` — chấp nhận lệch nếu master data đổi sau khi phiếu đã lưu)

`PaymentPartnerResolver` (`UseCases/PaymentPartnerResolver.cs`, static helper dùng chung bởi `CreatePaymentUseCase`/`UpdatePaymentUseCase`): parse `PartnerType`, gọi đúng repository (`ISupplierRepository`/`ICustomerRepository`/`IEmployeeRepository`) theo `PartnerId`, throw `DomainException` nếu không tồn tại, trả về tên để cache vào `PartnerName`.

**Migration** `20260826093257_AddPaymentPartnerType`: rename cột `SupplierId` → `PartnerId` (giữ nguyên giá trị cũ), thêm `PartnerType`/`PartnerName`, backfill bằng SQL thô (`UPDATE payments ... FROM suppliers WHERE s.id = p."PartnerId"`, set `PartnerType = 'Supplier'`) vì mọi phiếu cũ đều là Supplier. Drop FK/index cũ tới `suppliers`, thêm index composite `(PartnerType, PartnerId)`.

**DTO đổi:** `supplier_id`/`supplier_name` → `partner_type`/`partner_id`/`partner_name` trên cả `Create`/`Update`/`Response` DTO (Create/Update không có `partner_name` — BE tự resolve).

**WPF:** "Đối tượng" là **1 ô tìm kiếm chung** (`PartnerItems` = `Suppliers.Concat(Customers).Concat(Employees)`, đều implement `ISearchableItem` sẵn) — **không có** combo "chọn loại đối tượng" riêng, khớp đúng UX ảnh mẫu MISA (gõ mã gì cũng tìm ra, không bắt chọn loại trước). Loại (`PartnerType` gửi lên BE) suy ra từ kiểu runtime của object đã chọn (`ResolvePartnerType`), không cần user chọn. (Bản đầu 2026-08-26 có thêm combo loại riêng — đã bỏ sau khi so ảnh mẫu, xem mục trên.) Xem chi tiết trong doc WPF: `desktop-lamour/.../Accounting/docs/phieu-chi.md`.

**Không đổi:** `PaymentEntry.SubjectCode`/`SubjectName` (cột "Tên đối tượng" ở **dòng hạch toán**, free-text, khác hoàn toàn với "Đối tượng" ở header) — giữ nguyên, không liên quan tới thay đổi này.

---

## PRD Summary

Phiếu Chi ban đầu chỉ là CRUD đơn giản (Create/Update/Delete/Duplicate), TK Nợ/TK Có là 1 enum cứng 4 giá trị (`Cash111/Bank112/Receivable131/Payroll334`), không phân biệt Nháp/Đã ghi số. Ngày 2026-08-10, theo yêu cầu review UI so với ảnh mẫu MISA, đã nâng cấp:

- **Trạng thái Nháp/Đã ghi số** (`PaymentStatus`) — Cất = lưu Nháp (chưa post sổ quỹ), Ghi số = xác nhận (post `CashTransaction`, sau đó bất biến — đúng rule "Confirmed invoices are immutable" trong `CLAUDE.md`).
- **TK Nợ / TK Có** đổi từ enum `AccountCode` (4 giá trị cứng) → **FK thật tới `AccountSetting`** ("Tài khoản kế toán") — xem [`account-settings.md`](../../AccountSettings/docs/account-settings.md).
- **Khoản mục CP** (`ExpenseCategoryId`, nullable) — FK tới `ExpenseCategory`, xem [`expense-categories.md`](../../ExpenseCategories/docs/expense-categories.md).
- **Lý do chi chi tiết** (`ReasonDetail`, free-text) — bổ sung cạnh dropdown `PaymentReason` cố định, khớp ảnh mẫu ("Chi khác" + ô nhập tự do VD "thuê lái xe 21/7").

---

## Business Rules

| Rule | Description |
|------|-------------|
| Draft khi tạo mới | `CreatePaymentUseCase` luôn set `Status = PaymentStatus.Draft` — **không** tạo `CashTransaction` ngay (khác hành vi cũ trước 2026-08-10) |
| Draft → Treo | `SetPaymentTreoUseCase` (**mới**, 2026-08-11): chỉ chạy khi `Status == Draft` → set `Status = Treo`. Không tạo `CashTransaction`, không bắt buộc phải có dòng hạch toán (khác Confirm) |
| Draft/Treo đều sửa/xoá được | `UpdatePaymentUseCase`/`DeletePaymentUseCase` throw `DomainException` nếu `Status == Confirmed` ("Phiếu chi đã ghi số, không thể sửa/xoá") — Treo vẫn mutable như Draft |
| Ghi số chỉ từ Treo = bất biến | `ConfirmPaymentUseCase`: chỉ chạy khi `Status == Treo` (**đổi từ `Draft` sau 2026-08-11** — phải qua Treo trước) và có ≥1 dòng hạch toán; tạo `CashTransaction` (Credit — giảm tiền mặt) tại thời điểm này; set `Status = Confirmed`, `ConfirmedAt = UtcNow` |
| TK Nợ/TK Có required, FK thật | Validate `AccountSetting` tồn tại qua `IAccountSettingRepository.GetByIdAsync` — throw `DomainException` "Tài khoản Nợ/Có không tồn tại" nếu không có |
| Khoản mục CP optional | `ExpenseCategoryId` nullable — validate tồn tại nếu có giá trị, không bắt buộc |
| Duplicate luôn ra Draft mới | `DuplicatePaymentUseCase` set `Status = Draft` bất kể trạng thái phiếu gốc (Draft/Treo/Confirmed), **không** tạo `CashTransaction` cho bản sao |

---

## Architecture Overview

### Key Components (BE)

| Layer | File | Role |
|-------|------|------|
| Entity | `Lamour.Domain/Entities/Payment.cs` | + `PaymentStatus` enum (`Draft=0`/`Treo=1`/`Confirmed=2`), `Status`, `ConfirmedAt`, `ReasonDetail` |
| Entity | `Lamour.Domain/Entities/PaymentEntry.cs` | `DebitAccountSettingId`/`DebitAccountSetting` + `CreditAccountSettingId`/`CreditAccountSetting` (thay `AccountCode DebitAccount/CreditAccount` cũ), `ExpenseCategoryId`/`ExpenseCategory` (nullable) |
| Config | `Lamour.Infrastructure/Persistence/Configurations/PaymentConfiguration.cs` | `PaymentEntryConfiguration`: FK `Restrict` tới `AccountSetting` (2 lần — Debit/Credit), FK `SetNull` tới `ExpenseCategory`. `Status` lưu `HasConversion<string>()` — thêm giá trị enum mới an toàn, không cần migration |
| UseCase | `UseCases/CreatePaymentUseCase.cs` | Validate `PaymentReason`, TK Nợ/Có tồn tại, Khoản mục CP (nếu có) → `Status = Draft`, **không** tạo `CashTransaction` |
| UseCase | `UseCases/SetPaymentTreoUseCase.cs` | **Mới** (2026-08-11) — Guard `Status == Draft` → set `Status = Treo` |
| UseCase | `UseCases/UpdatePaymentUseCase.cs` | Guard `Status == Confirmed` (đổi từ `!= Draft`) trước khi block sửa — Draft/Treo đều sửa được |
| UseCase | `UseCases/DeletePaymentUseCase.cs` | Guard `Status == Confirmed` (đổi từ `!= Draft`) trước khi block xoá — **không còn** cleanup `CashTransaction` (Draft/Treo chưa từng có) |
| UseCase | `UseCases/ConfirmPaymentUseCase.cs` | Guard `Status == Treo` (**đổi từ `Draft`**) — mirror `ConfirmWarehouseReceiptUseCase`: Treo→Confirmed, tạo `CashTransaction` tại đây |
| UseCase | `UseCases/DuplicatePaymentUseCase.cs` | Copy `DebitAccountSettingId`/`CreditAccountSettingId`/`ExpenseCategoryId`, luôn `Status = Draft` |
| Repository | `Lamour.Infrastructure/Repositories/PaymentRepository.cs` | `.Include(Entries).ThenInclude(DebitAccountSetting/CreditAccountSetting/ExpenseCategory)` trên mọi query. `GetUnconfirmedByDateRangeAsync` (đổi tên từ `GetDraftsByDateRangeAsync`, filter `Status != Confirmed`) — dùng cho Sổ Quỹ Tiền Mặt |
| Controller | `Lamour.Api/Controllers/PaymentsController.cs` | + `POST {id}/confirm`, `POST {id}/treo` |

### State Machine

```
  Create           Treo (nút "Nạp")        Ghi số
Draft ──────► Draft ──────────────► Treo ──────► Confirmed (bất biến)
                ↺ Update/Delete       ↺ Update/Delete
```

Update/Delete cho phép ở cả `Draft` và `Treo` — chỉ `Confirmed` mới bất biến.

---

## API Contracts

Base route: `api/v1/accounting/payments`

| Method | Endpoint | Ghi chú |
|--------|----------|---------|
| `GET` | `/` | Tất cả payments |
| `GET` | `/{id}` | 1 payment |
| `POST` | `/` | Tạo mới — luôn Draft |
| `PUT` | `/{id}` | Sửa — 400 nếu đã Confirmed |
| `DELETE` | `/{id}` | Xoá — 400 nếu đã Confirmed |
| `POST` | `/{id}/duplicate` | Sao chép — bản mới luôn Draft |
| `POST` | `/{id}/treo` | **Mới** (2026-08-11) — Draft → Treo, 400 nếu không phải Draft |
| `POST` | `/{id}/confirm` | Treo → Confirmed (**đổi từ Draft**), tạo `CashTransaction` |
| `POST` | `/{id}/unconfirm` | **Mới** (2026-08-26) — Confirmed → Treo ("Hoàn"), xoá `CashTransaction`, 400 nếu không phải Confirmed |

### `PaymentEntryDto` (request + response dùng chung)

```json
{
  "id": 1,
  "description": "thuê lái xe 21/7",
  "debit_account_id": 40,
  "debit_account_code": "111",
  "debit_account_description": "Tiền mặt",
  "credit_account_id": 42,
  "credit_account_code": "131",
  "credit_account_description": "Phải thu của khách hàng",
  "amount": 500000,
  "subject_code": null,
  "subject_name": null,
  "bank_account": null,
  "expense_category_id": 3,
  "expense_category_name": "PHÒNG NHÂN SỰ"
}
```

Request (Create/Update) chỉ cần `debit_account_id`/`credit_account_id`/`expense_category_id` (int, int?) — các field `*_code`/`*_description`/`*_name` chỉ có ở response.

### `PaymentResponseDto` (bổ sung so với trước)

```json
{
  "...": "...",
  "reason_detail": "thuê lái xe 21/7",
  "status": "Draft",
  "confirmed_at": null
}
```

---

## Seed Data — 4 tài khoản cash-flow bổ sung cho `AccountSetting`

`AccountSetting` (39 tài khoản cũ, toàn bộ thuộc nhóm hàng hoá/doanh thu — xem [`account-settings.md`](../../AccountSettings/docs/account-settings.md)) **không có** 111/112/131/334 mà `AccountCode` enum cũ dùng cho Payment. Seed thêm 4 dòng để Payment không bị gãy sau khi chuyển sang FK:

| Id | Code | Description |
|---|---|---|
| 40 | 111 | Tiền mặt |
| 41 | 112 | Tiền gửi ngân hàng |
| 42 | 131 | Phải thu của khách hàng |
| 43 | 334 | Phải trả người lao động |

Enum `AccountCode` (`Lamour.Domain.Enums.AccountCode`) **vẫn còn** trong codebase — chỉ Payment không dùng nữa, `Receipt`/`ReceiptEntry` (Phiếu thu) vẫn dùng enum này y như cũ (chưa migrate).

---

## EF Migrations (theo thứ tự)

1. `20260810082526_AddDepartmentsAndExpenseCategories` — tạo bảng `departments`/`expense_categories` (xem [`expense-categories.md`](../../ExpenseCategories/docs/expense-categories.md))
2. `20260810094907_AddPaymentStatusAndExpenseCategoryLink` — `payments.status`/`confirmed_at`/`reason_detail`, `payment_entries.expense_category_id` (FK `SetNull`)
   - ⚠️ Lần đầu generate có `HasDefaultValue(PaymentStatus.Confirmed)` trên model → EF cảnh báo "sentinel value" (giá trị CLR default `Draft`=0 sẽ luôn bị ghi đè bởi DB default khi insert, vì EF coi giá trị bằng CLR-default là "chưa set"). Đã bỏ `HasDefaultValue` khỏi `Configure()`, chỉ giữ `defaultValue: "Confirmed"` ở migration `AddColumn` (dùng 1 lần để backfill — bảng `payments` rỗng lúc đó nên không ảnh hưởng dữ liệu thật). **Bài học: không dùng `HasDefaultValue` khi giá trị default trùng CLR-default (0/null/false) của property.**
3. `20260810125950_ConvertPaymentAccountsToAccountSettingFk` — drop cột string `DebitAccount`/`CreditAccount`, add FK int `DebitAccountSettingId`/`CreditAccountSettingId`, insert 4 dòng `account_settings` (111/112/131/334)

---

## Sổ Kế Toán Chi Tiết Quỹ Tiền Mặt — cột Status (2026-08-11)

Màn hình "Sổ Kế Toán Chi Tiết Quỹ Tiền Mặt" (`AccountingView.xaml`) trước đây chỉ đọc `CashTransaction` (luôn đã ghi số). Bổ sung hiển thị cả Payment ở trạng thái `Draft` (chưa ghi số) để người dùng thấy trước các khoản chi sắp tới:

- `IPaymentRepository.GetUnconfirmedByDateRangeAsync(from, to, ct)` — query `Payment` có `Status != Confirmed` (Draft **hoặc** Treo, đổi từ chỉ-Draft sau khi thêm trạng thái Treo 2026-08-11) trong khoảng `AccountingDate`, `Include(Entries.DebitAccountSetting)`.
- `CashLedgerEntryDto` + `GetCashLedgerUseCase`: merge entries từ `CashTransaction` (`status = "Confirmed"`) với Draft/Treo payments map ngược lại theo đúng công thức `ConfirmPaymentUseCase` dùng để tạo `CashTransaction` (`CreditAmount = tổng Entries.Amount`, `CounterAccount` = code TK Nợ dòng đầu, `DebitAmount = 0`), gắn `status = p.Status.ToString()` ("Draft" hoặc "Treo"). Payment chưa có dòng hạch toán nào bị loại khỏi kết quả (không có amount để hiển thị).
- **Running balance chỉ cộng/trừ trên dòng `Confirmed`** — dòng `Draft`/`Treo` giữ nguyên số dư hiện tại, không ảnh hưởng `ClosingBalance`.
- WPF: `CashLedgerEntryDto.Status` (string, khớp BE) + `CashLedgerStatusDisplayConverter` (`Shared/Converters/`) map `"Draft"` → "Nháp", `"Treo"` → "Treo", `"Confirmed"` → "Đã ghi số", cột mới nằm ngay sau "Số phiếu chi" trong `AccountingView.xaml`.

---

## WPF Client (`desktop-lamour`)

### UI — khớp lại theo ảnh mẫu MISA (2026-08-10)

- **Toolbar**: `Trước/Sau | Thêm/Sửa/Xóa/Treo/Ghi số | Làm mới | In/Đóng` — bỏ hẳn `Sửa nhanh/Tiện ích/Mẫu/Giúp` (không có logic thật, ẩn đi theo yêu cầu "action nào không có thì ẩn đi" thay vì hiện placeholder).
  - ⚠️ **2026-08-11**: nút `Nạp` cũ (`LoadAsync2Command`) chỉ refresh list từ server — vô nghĩa vì list đã tự reload sau mọi Sửa/Xoá/Ghi số. Đổi thành nút **"📌 Treo"** (`TreoCommand`) — set `CurrentPayment` đang `Draft` sang `Treo` qua `POST /{id}/treo`, bắt buộc phải Treo trước khi Ghi số được. Refresh-list thủ công dời sang nút mới **"🔄 Làm mới"** (giữ nguyên logic `LoadAsync2` cũ, đổi tên method).
  - ⚠️ **2026-08-11 (tiếp)**: bỏ hẳn nút `Cất` (`SaveCommand`/`SaveAsync`, xoá khỏi ViewModel) — logic lưu (`PersistAsync`) gộp vào `TreoCommand`: đang Nháp thì lưu + chuyển Treo; đã Treo rồi thì `TreoAsync` chỉ chạy `PersistAsync` để lưu lại thay đổi, **không** gọi lại `POST /{id}/treo` (tránh lỗi 400 "Chỉ phiếu chi ở trạng thái Nháp mới có thể chuyển Treo"), không đổi trạng thái, không đóng popup.
- **Banner header + subtitle**: giữ nguyên style cũ, chỉ sửa lại subtitle bug copy-paste "Lập và quản lý phiếu **thu**" → "...phiếu **chi**".
- **Thông tin chung**: "Lý do nộp"→"Lý do chi" (đúng thuật ngữ), thêm ô `ReasonDetail` tự do cạnh dropdown; "Nhân viên thu"→"Nhân viên"; bố cục lại Nhân viên+Kèm theo chung 1 dòng, Tham chiếu xuống dòng riêng (icon 🔍).
- **Tab**: `TabControl` với style `AppTabControl.Modern`/`AppTabItem.Modern` copy từ popup "Thêm vật tư hàng hoá" — tab "1. Hạch toán" (grid) + tab "2. Thuế" (placeholder, Payment chưa có field thuế).
- **Grid**: thêm cột "Khoản mục CP"; TK Nợ/TK Có đổi nguồn từ list string cứng → `AccountSettings` (load qua `IGetAccountSettingsUseCase`).
- **In**: `PaymentPrintWindow` mới — FlowDocument A5 + `PrintDialog`, mirror `SalesOrderPrintWindow` (logo công ty, bảng hạch toán có cột Khoản mục CP, chữ ký 4 vai: Người lập phiếu/Người nhận tiền/Thủ quỹ/Kế toán trưởng).

### ⚠️ Bug lớn nhất & cách sửa cuối cùng (đọc trước khi đổi UI cột combo trong DataGrid)

Cột TK Nợ/TK Có/Khoản mục CP trải qua **4 lần đổi cách bind** trước khi ổn định — ghi lại để không lặp lại sai lầm:

1. **`DataGridComboBoxColumn` + `SelectedValueBinding`/`SelectedValuePath="Id"`** — ItemsSource `{Binding AccountSettings}` (không `RelativeSource`) **không nhận được DataContext của DataGrid trong app này** → dropdown rỗng hoàn toàn. *Kết luận: đừng dùng `DataGridComboBoxColumn` cho cột trong file này, dù đây là cách "chuẩn" theo tài liệu Microsoft.*
2. **`DataGridTemplateColumn` (CellTemplate hiện Text, CellEditingTemplate là ComboBox) + `SelectedValue`/`SelectedValuePath="Id"` (kiểu `int?`)** — `ItemsSource` load đúng (có data), nhưng `SelectedValue` TwoWay **không đẩy được** giá trị ngược lại property `int?` nguồn khi list chỉ có 1 item (xác nhận qua debug log: `SelectionChanged` bắn đúng `SelectedValue` nhưng `PropertyChanged` trên entry không bao giờ fire).
3. Đổi `SelectedValue` → **`SelectedItem`** (bind cả object `ISearchableItem?`/`ExpenseCategory?` thay vì `int?`) — vẫn còn `DataGridTemplateColumn` (Cell/CellEditingTemplate tách biệt). `SelectedItem` binding **hoạt động đúng khi bấm Enter**, nhưng **không commit khi chỉ click sang ô/cột khác** — DataGrid chỉ chịu đẩy binding CellEditingTemplate xuống nguồn khi nhận tín hiệu commit rõ ràng (Enter/Tab), không tự làm khi cell mất focus do click cell khác trong cùng row. Thử ép `grid.CommitEdit(DataGridEditingUnit.Cell, true)` (cả gọi đồng bộ và trì hoãn qua `Dispatcher.BeginInvoke`) — **không hoạt động**. Thử giả lập phím Enter bằng `RaiseEvent(new KeyEventArgs(..., Key.Enter) { RoutedEvent = Keyboard.KeyDownEvent })` — **cũng không hoạt động**.
4. **Fix cuối cùng**: bỏ hẳn khái niệm "cell editing mode" cho 3 cột này — **ComboBox nằm thẳng trong `CellTemplate`, không có `CellEditingTemplate` riêng**. ComboBox luôn hiện sẵn, luôn tương tác được ngay, không cần DataGrid "vào chế độ sửa" rồi "commit" gì cả → không còn gì để mất. Đây là cách ổn định nhất cho ComboBox trong `DataGridTemplateColumn` khi không cần phân biệt display/edit riêng.

```xml
<DataGridTemplateColumn Header="TK Nợ" Width="160">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <ComboBox ItemsSource="{Binding DataContext.AccountSettings, RelativeSource={RelativeSource AncestorType=DataGrid}}"
                      DisplayMemberPath="DisplayText"
                      SelectedItem="{Binding SelectedDebitAccount, Mode=TwoWay}"
                      BorderThickness="0" Background="Transparent"/>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
    <!-- Không có CellEditingTemplate -->
</DataGridTemplateColumn>
```

`PaymentEntryItem.cs` (WPF grid row model) theo đó chỉ giữ `SelectedDebitAccount`/`SelectedCreditAccount` (`ISearchableItem?`) + `SelectedExpenseCategory` (`ExpenseCategory?`) — không cần property "Name" riêng để đồng bộ tay, vì `CellTemplate` đọc trực tiếp qua property path (`SelectedDebitAccount.DisplayText`).

### Ghi nhớ TK Nợ/TK Có lần cuối chọn

`ILastUsedPaymentAccountsStore`/`LastUsedPaymentAccountsStore` (`Data/Storage/`, `AddSingleton`) — lưu `LastDebitAccountId`/`LastCreditAccountId` mỗi khi user đổi lựa chọn, dòng mới ("+ Thêm dòng") tự mặc định theo đó. **Chỉ lưu trong RAM (session hiện tại)** — app này chưa có cơ chế lưu file settings nào cả (kể cả JWT token cũng chỉ lưu RAM qua `InMemoryAuthTokenStorage`), nên giá trị này **mất khi tắt app**, không phải bug.

### Files mới/đổi chính (WPF)

```
Features/HomePage/Accounting/
  Domain/Models/PaymentEntryItem.cs        — SelectedDebitAccount/SelectedCreditAccount/SelectedExpenseCategory
  Domain/UseCases/{I}ConfirmPaymentUseCase.cs — mới
  Data/Services/{I}PaymentService.cs       — + ConfirmAsync, TreoAsync (2026-08-11), sửa EnsureSuccessOrThrowAsync (đọc {"error":...} thay vì EnsureSuccessStatusCode() nuốt message)
  Data/Storage/{I}LastUsedPaymentAccountsStore.cs — mới
  ViewModels/PaymentViewModel.cs           — CanEdit (gate theo Status), ConfirmCommand, TreoCommand (2026-08-11, thay LoadAsync2Command + SaveCommand cũ — gộp lưu vào Treo), RefreshCommand (mới, giữ logic refresh-list cũ), EditCommand, PrintCommand
  Views/PaymentWindow.xaml                 — toolbar/tab/grid mới
  Views/PaymentPrintWindow.xaml(.cs)       — mới, in FlowDocument A5
```

### Known gaps

- Tab "2. Thuế" chỉ là placeholder — Payment chưa có field thuế nào.
- Cột "Mục thu/chi", "Đối tượng THCP", "Công trình" trong ảnh mẫu **chưa làm** — chưa có data model, user chọn bỏ qua lần này.
- Chưa có unit test nào cho `ConfirmPaymentUseCase`/lifecycle mới.
- "Sửa nhanh"/"Tiện ích"/"Mẫu"/"Giúp" hoàn toàn không có trên UI (theo yêu cầu ẩn action không có thật), không phải bug thiếu sót.

---

*Cập nhật lần cuối: 2026-08-26*
