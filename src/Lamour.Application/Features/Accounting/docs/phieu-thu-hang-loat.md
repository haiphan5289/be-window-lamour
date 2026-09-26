# Phiếu Thu Hàng Loạt (Thu tiền khách hàng hàng loạt) — Feature Document

> **Jira:** — (branch `dev` không có mã ticket) | **Branch:** `dev` | **Generated:** 2026-09-26
> **Nguồn:** đọc trực tiếp code BE (`be-window-lamour`) + WPF (`desktop-lamour`) ngày 2026-09-26.
> Jira/Confluence không lấy được (Atlassian MCP chưa đăng nhập).
> Lịch sử thiết kế (vì sao bỏ "group theo khách hàng ra nhiều phiếu"): xem mục
> "Phiếu thu tiền khách hàng hàng loạt (2026-08-26)" trong [phieu-thu.md](phieu-thu.md).

---

## PRD Summary

- **Goal:** Lập **1 phiếu thu duy nhất** để thu tiền cho **nhiều chứng từ bán hàng còn nợ**, kể cả của nhiều khách hàng khác nhau, thay vì lập từng phiếu thu một.
- **User story:** Là kế toán/thu ngân, tôi muốn tìm các chứng từ bán hàng còn nợ theo khoảng ngày và NV bán hàng, tick chọn nhiều chứng từ, sửa số tiền thu từng dòng (thu 1 phần được), rồi lưu thành 1 phiếu thu, để không phải lập phiếu thu cho từng khách hàng.
- **Acceptance criteria** (đối chiếu với code hiện tại):
  - [x] Lọc chứng từ còn nợ theo Khoảng thời gian (Hôm nay / Hôm qua / Tuần này / Đầu tháng đến hiện tại / Tùy chọn) + NV bán hàng
  - [x] Chỉ hiện chứng từ đã ghi sổ và còn nợ > 0
  - [x] Tick chọn nhiều dòng (có ô chọn tất cả)
  - [x] Chọn phương thức: Tiền mặt (TK 111) hoặc Tiền gửi (TK 112 + TK ngân hàng)
  - [x] Sửa số thu từng dòng, không cho vượt số còn nợ
  - [x] Tạo đúng 1 phiếu thu cho toàn bộ dòng đã chọn
  - [ ] Phiếu tạo ra được **ghi sổ ngay** như phiếu thu thường: **chưa**, xem "Notes" (phiếu nằm ở trạng thái Treo)

---

## Business Rules

| Rule | Description |
|------|-------------|
| Nguồn chứng từ còn nợ | `SalesOrder.Status == Normal` (đã ghi sổ), `AccountingDate` trong khoảng lọc (tính trọn ngày cuối), lọc thêm `EmployeeId` nếu chọn NV bán hàng. Sắp xếp theo `AccountingDate` rồi `DocumentNumber` |
| Số còn nợ | `GrandTotal − Σ ReceiptEntry.Amount (có SalesOrderId = đơn) − Σ DepositDeduction.Amount`. Chỉ trả về dòng còn nợ > 0 |
| Số còn nợ tính cả phiếu chưa ghi sổ | `Σ ReceiptEntry` không lọc theo trạng thái phiếu → phiếu thu đang **Treo** cũng đã "giữ chỗ" số tiền. Xóa phiếu thì số nợ tự quay lại |
| 1 phiếu duy nhất | `Receipt.CustomerId = null`. Mỗi dòng hạch toán tự mang khách hàng riêng qua `SubjectCode` / `SubjectName` (tên = `CustomerNameOverride` nếu có, không thì `Customer.Name`) |
| Lý do thu | Luôn `ThuCongNo`. WPF hiển thị cố định "Thu tiền khách hàng" |
| Tài khoản | TK Nợ = `Cash111` hoặc `Bank112` (chọn 1 lần, áp dụng cho mọi dòng). TK Có luôn `Receivable131` |
| Diễn giải từng dòng | `"Thu tiền khách hàng - {Số chứng từ bán hàng}"` |
| Tham chiếu | Nối các số chứng từ bán hàng đã chọn, phân tách bằng `", "` (không trùng) |
| Người nộp | Người dùng nhập → nếu trống: tên NV thu nợ → nếu trống: `"Thu tiền khách hàng hàng loạt"`. WPF tự điền tên NV thu nợ khi chọn NV, nếu ô Người nộp đang trống |
| Số chứng từ | BE tự sinh số `PT` tiếp theo lúc lưu (`IGetNextReceiptCodeUseCase`). WPF chỉ hiện **số dự đoán**, số thật có thể khác nếu có phiếu khác vừa tạo |
| Số tiền mỗi dòng | Mặc định = số còn nợ lúc chọn. WPF chặn `<= 0` hoặc `>` số còn nợ. BE chặn `<= 0` và kiểm tra lại số còn nợ **thực tế** trong DB lúc lưu (không tin số client gửi lên) |
| Ít nhất 1 dòng | BE: `"Phải chọn ít nhất 1 chứng từ để thu tiền."`. WPF: `"Vui lòng chọn ít nhất 1 chứng từ để thu tiền."` |
| Loại chứng từ trên sổ quỹ | `"Phiếu thu tiền mặt khách hàng hàng loạt"` (khi `CustomerId == null`) |

---

## Architecture Overview

### Key Components

| Layer | File | Role |
|-------|------|------|
| API | `Lamour.Api/Controllers/ReceiptsController.cs` | `GET outstanding-orders`, `POST bulk` |
| UseCase | `Lamour.Application/Features/Accounting/UseCases/GetOutstandingSalesOrdersUseCase.cs` | Map tuple repo → `OutstandingSalesOrderDto` |
| UseCase | `Lamour.Application/Features/Accounting/UseCases/CreateBulkCustomerReceiptUseCase.cs` | Validate dòng, dựng `CreateReceiptRequestDto`, gọi lại `ICreateReceiptUseCase` |
| UseCase (dùng lại) | `Lamour.Application/Features/Accounting/UseCases/CreateReceiptUseCase.cs` | Chặn thu quá số còn nợ, lưu `Receipt` (trạng thái Draft = Treo) |
| Repository | `Lamour.Infrastructure/Repositories/ReceiptRepository.cs` | `GetOutstandingSalesOrdersAsync`, `GetRemainingAmountAsync` |
| DTOs | `Lamour.Application/Features/Accounting/Dtos/BulkCustomerReceiptDtos.cs` | Request/Response + `OutstandingSalesOrderDto` |
| WPF — popup 1 | `desktop-lamour/.../Accounting/Views/BulkCustomerReceiptSearchWindow.xaml` + `ViewModels/BulkCustomerReceiptSearchViewModel.cs` | Lọc, tick chọn, chọn phương thức |
| WPF — popup 2 | `desktop-lamour/.../Accounting/Views/BulkCustomerReceiptWindow.xaml` + `ViewModels/BulkCustomerReceiptViewModel.cs` | Sửa số thu, thông tin chung, Cất |
| WPF — models | `Domain/Models/OutstandingSalesOrderCheckItem.cs`, `Domain/Models/BulkReceiptLineItem.cs` | Dòng có ô tick / dòng hạch toán có số thu sửa được |
| WPF — service | `Data/Services/ReceiptService.cs` (`IReceiptService`) | Gọi 2 endpoint trên |

### Data Flow

```
Màn Quỹ → ➕ Thêm ▾ → "Thu tiền khách hàng hàng loạt" (AccountingViewModel.OpenBulkCustomerReceipt)
  → Popup 1: BulkCustomerReceiptSearchWindow (ShowDialog)
      mở ra tự tải danh sách (InitializeAsync → LoadAsync)
      "🔍 Lấy dữ liệu" → GET /api/v1/accounting/receipts/outstanding-orders?from_date&to_date&employee_id
      tick chọn + chọn Tiền mặt/Tiền gửi → "✔ Thu tiền"
  → Popup 2: BulkCustomerReceiptWindow (ShowDialog)
      sửa Số thu từng dòng, NV thu nợ, Người nộp, Địa chỉ, Kèm theo
      "💾 Cất" → POST /api/v1/accounting/receipts/bulk
        → CreateBulkCustomerReceiptUseCase → CreateReceiptUseCase → Receipt (Draft = Treo)
      thành công: hộp thoại "Đã tạo phiếu thu PTxxxxx (n dòng, tổng …)" → đóng popup 2
  ← Popup 1 tự tải lại danh sách còn nợ
  ← Đóng popup 1 → màn Quỹ tự tải lại sổ quỹ (phiếu mới hiện với trạng thái "Treo")
```

---

## Key Files & Symbols

### BE (`be-window-lamour/src`)
- [`ReceiptsController.cs`](../../../../Lamour.Api/Controllers/ReceiptsController.cs): `GetOutstandingOrders(from_date, to_date, employee_id)`, `CreateBulkCustomerReceipt(request)`
- [`GetOutstandingSalesOrdersUseCase.cs`](../UseCases/GetOutstandingSalesOrdersUseCase.cs): `ExecuteAsync(DateOnly fromDate, DateOnly toDate, int? employeeId, ct)`
- [`CreateBulkCustomerReceiptUseCase.cs`](../UseCases/CreateBulkCustomerReceiptUseCase.cs): `ExecuteAsync(CreateBulkCustomerReceiptRequestDto, ct)`
- [`CreateReceiptUseCase.cs`](../UseCases/CreateReceiptUseCase.cs): kiểm tra lại số còn nợ khi dòng có `SalesOrderId`
- [`IReceiptRepository.cs`](../Repositories/IReceiptRepository.cs) / [`ReceiptRepository.cs`](../../../../Lamour.Infrastructure/Repositories/ReceiptRepository.cs): `GetOutstandingSalesOrdersAsync`, `GetRemainingAmountAsync`
- [`BulkCustomerReceiptDtos.cs`](../Dtos/BulkCustomerReceiptDtos.cs): `OutstandingSalesOrderDto`, `BulkReceiptLineRequestDto`, `CreateBulkCustomerReceiptRequestDto`, `CreateBulkCustomerReceiptResponseDto`

### WPF (`desktop-lamour/src/DesktopLamour/Features/HomePage/Accounting`)
- `ViewModels/BulkCustomerReceiptSearchViewModel.cs`: `InitializeAsync`, `LoadAsync`, `Collect` ("Thu tiền"), `Cancel`, `PeriodOptions`, `PaymentMethod`, `AreAllSelected`
- `ViewModels/BulkCustomerReceiptViewModel.cs`: `Initialize(selected, debitAccount, bankAccount, collectorEmployeeId)`, `SaveAsync` ("Cất")
- `Views/BulkCustomerReceiptSearchWindow.xaml` (+ `.cs`), `Views/BulkCustomerReceiptWindow.xaml` (+ `.cs`, trả `DialogResult = true` khi lưu xong)
- `Domain/Models/OutstandingSalesOrderCheckItem.cs`, `Domain/Models/BulkReceiptLineItem.cs`
- `Domain/UseCases/GetOutstandingSalesOrdersUseCase.cs`, `Domain/UseCases/CreateBulkCustomerReceiptUseCase.cs`
- `Data/Services/Dtos/BulkCustomerReceiptDtos.cs`

### Các cột trên màn hình
- **Popup 1:** ô tick · Ngày chứng từ · Số chứng từ · Mã khách hàng · Tên khách hàng · Diễn giải · Còn nợ
- **Popup 2, tab "1. Hạch toán":** Diễn giải · TK Nợ · TK Có · Số tiền · Mã khách hàng · Tên khách hàng
- **Popup 2, tab "2. Chứng từ":** Ngày chứng từ · Số chứng từ · Hạn thanh toán · Điều khoản TT · Số phải thu · Số chưa thu · Số thu · TK phải thu

---

## API Contracts

| Method | Endpoint | Input | Output |
|--------|----------|-------|--------|
| `GET` | `/api/v1/accounting/receipts/outstanding-orders` | query `from_date` (DateOnly), `to_date` (DateOnly), `employee_id` (int, tùy chọn) | `OutstandingSalesOrderDto[]` |
| `POST` | `/api/v1/accounting/receipts/bulk` | `CreateBulkCustomerReceiptRequestDto` | `CreateBulkCustomerReceiptResponseDto` (`{ "receipt": ReceiptResponseDto }`) |

### Request — `POST /bulk`

```json
{
  "accounting_date": "2026-09-26T00:00:00",
  "document_date": "2026-09-26T00:00:00",
  "debit_account": "Cash111",
  "bank_account": null,
  "collector_employee_id": 3,
  "payer_name": null,
  "address": null,
  "attachment": null,
  "lines": [
    { "sales_order_id": 118, "amount": 3971000 },
    { "sales_order_id": 117, "amount": 1000000 }
  ]
}
```

### Response item — `GET /outstanding-orders`

```json
{
  "sales_order_id": 118,
  "document_number": "XK00118",
  "accounting_date": "2026-09-12T00:00:00Z",
  "document_date": "2026-09-12T00:00:00Z",
  "customer_id": 1,
  "customer_code": "KH00001",
  "customer_name": "Phương Hoa Spa",
  "description": null,
  "remaining_amount": 3971000,
  "grand_total": 3971000,
  "payment_terms": null,
  "payment_due_date": null
}
```

> Số liệu trong 2 ví dụ trên chỉ để minh họa.

---

## Edge Cases & Error Handling

| Scenario | Expected Behavior | Handled? |
|----------|------------------|----------|
| Không tick dòng nào mà bấm "Thu tiền" | Hộp thoại "Vui lòng chọn ít nhất 1 chứng từ để thu tiền." | ✅ WPF |
| Gửi lên 0 dòng | BE: `DomainException` "Phải chọn ít nhất 1 chứng từ để thu tiền." | ✅ BE |
| Số thu `<= 0` hoặc lớn hơn số còn nợ | Báo lỗi ngay trên popup 2, không gọi BE | ✅ WPF |
| Số thu `<= 0` gửi thẳng lên BE | `DomainException` "Số tiền thu phải lớn hơn 0 …" | ✅ BE |
| Có phiếu thu khác vừa thu cùng đơn (số còn nợ đã giảm) | BE kiểm tra lại số còn nợ trong DB: "Số tiền thu (…) vượt quá số còn nợ thực tế (…)" | ✅ BE |
| `sales_order_id` không tồn tại | `DomainException` "Không tìm thấy chứng từ bán hàng id=…" | ✅ BE |
| Cùng 1 đơn xuất hiện 2 lần trong `lines` | Chỉ tra đơn 1 lần, nhưng **vẫn tạo 2 dòng hạch toán**. Dòng lặp thứ 2 **bỏ qua cả kiểm tra `amount <= 0`** (vòng lặp `continue` trước bước kiểm tra), và mỗi dòng chỉ so riêng với số còn nợ, không cộng dồn | ⚠️ WPF không tạo được trường hợp này, BE không chặn |
| Khoảng ngày không có chứng từ còn nợ | Hiện "Không có chứng từ nào còn nợ trong khoảng ngày đã chọn." | ✅ WPF |
| Lỗi tải danh sách | Banner "Không thể tải dữ liệu: …" | ✅ WPF |
| Lỗi khi lưu | Banner "Không thể tạo phiếu thu: …", popup vẫn mở | ✅ WPF |
| Chọn Tiền gửi nhưng bỏ trống TK ngân hàng | Không chặn, phiếu lưu với `bank_account = null` | ⚠️ Chưa kiểm tra |

---

## Test Coverage Notes

| Component | Test File | Coverage |
|-----------|-----------|----------|
| `CreateBulkCustomerReceiptUseCase` (BE) | — | ❌ Chưa có |
| `GetOutstandingSalesOrdersUseCase` (BE) | — | ❌ Chưa có |
| `ReceiptRepository.GetOutstandingSalesOrdersAsync` / `GetRemainingAmountAsync` | — | ❌ Chưa có (cần DB thật hoặc SQLite in-memory) |
| `BulkCustomerReceiptSearchViewModel` / `BulkCustomerReceiptViewModel` (WPF) | — | ❌ Chưa có (test project WPF đang lỗi build sẵn: `RegisterViewModelTests.cs` thiếu `using Xunit`) |

**Suggested test cases:**
- [ ] 0 dòng → `DomainException`
- [ ] Dòng có `amount <= 0` → `DomainException`
- [ ] 2 đơn của 2 khách hàng khác nhau → đúng 1 `CreateReceiptRequestDto`, `CustomerId = null`, 2 entry có `SubjectCode` khác nhau, `Reference` = 2 số chứng từ
- [ ] `payer_name` trống + có NV thu → `PayerName` = tên NV; không NV → `"Thu tiền khách hàng hàng loạt"`
- [ ] `debit_account = "Bank112"` → mọi entry có `DebitAccount = "Bank112"` và `BankAccount` được truyền xuống
- [ ] Repository: đơn đã thu đủ (còn nợ = 0) không xuất hiện; đơn chưa ghi sổ (Held) không xuất hiện

---

## Notes

- **Phiếu thu hàng loạt lưu ở trạng thái Treo, chưa ghi sổ — ĐÃ XÁC NHẬN đúng ý MISA (2026-09-26).** Ảnh mẫu MISA cho thấy popup xác nhận là 1 cửa sổ chứng từ đầy đủ với toolbar riêng: `Trước · Sau · Thêm · Sửa · Sửa nhanh | Cất · Xóa · Hoàn · Ghi sổ | Nạp · Tiện ích | Mẫu | In · Xuất khẩu`. **"Cất" và "Ghi sổ" là 2 nút TÁCH RIÊNG** — khác hẳn luồng "Cất = Ghi sổ ngay" của Phiếu thu/Chi thường và Chứng từ bán hàng. Vậy hành vi hiện tại của `CreateBulkCustomerReceiptUseCase` (chỉ tạo Draft, không tự Confirm) **là đúng theo MISA, không phải thiếu sót** — không cần sửa thành tự ghi sổ. Comment cũ trong file (nói "ghi CashTransaction side-effect đã có sẵn") vẫn lỗi thời, nên xoá.
- **Mở phiếu thu hàng loạt bằng popup Phiếu thu thường (Sửa → Cất) sẽ báo lỗi** "Vui lòng chọn đối tượng (khách hàng)." vì phiếu hàng loạt có `CustomerId = null`. Ghi sổ, Bỏ ghi, Xóa trên màn Quỹ hay trong popup vẫn dùng được vì không cần khách hàng.
- Tiêu đề popup 2 luôn ghi "Phiếu thu **tiền mặt** khách hàng hàng loạt", kể cả khi chọn Tiền gửi (TK 112). Tên "Loại chứng từ" trên sổ quỹ cũng vậy.
- Popup 2 không có nút In; app chưa có mẫu in phiếu thu.
- Trong popup 1, đổi Khoảng thời gian chỉ tính lại Từ/Đến, **không tự tải lại**: phải bấm "🔍 Lấy dữ liệu".

---

## So sánh chi tiết với ảnh mẫu MISA (review 2026-09-26)

7 ảnh MISA: popup tìm kiếm (trống → có dữ liệu → tick chọn) → popup xác nhận (mới mở, còn trống Người nộp) → 1 phiếu **đã Cất xong**, cả 2 tab "1. Hạch toán" và "2. Chứng từ". File đọc: `BulkCustomerReceiptSearchWindow.xaml`, `BulkCustomerReceiptWindow.xaml`, 2 ViewModel tương ứng.

### Popup 1 — Tìm kiếm (`BulkCustomerReceiptSearchWindow`)

| Chi tiết | MISA | App hiện tại | Khớp? |
|---|---|---|---|
| Phương thức thanh toán (Tiền mặt/Tiền gửi) | Có | Có (`PaymentMethod` radio) | ✅ |
| Khoảng thời gian / Từ / Đến | Có | Có (`SelectedPeriod`/`FromDate`/`ToDate`) | ✅ |
| **Ngày thu tiền** (field riêng, đổ vào Ngày hạch toán + Ngày chứng từ của phiếu tạo ra) | Có | **Không có** — popup 2 tự mặc định `DateTime.Today` | ❌ Thiếu |
| NV bán hàng | Có | Có (`SelectedEmployee`) | ✅ |
| **Số tiền** (tổng số tiền các dòng ĐÃ TICK, cập nhật sống) | Có — đổi ngay khi tick/bỏ tick | **Không có** trường này ở popup 1 | ❌ Thiếu |
| Cột lưới: Ngày chứng từ, Số chứng từ, Mã KH, Tên KH, Diễn giải | Có | Có | ✅ |
| Cột "Số hóa đơn" | Có (luôn trống trong ảnh mẫu) | Không có | ⚪ Không áp dụng — app chưa có số hoá đơn điện tử riêng khỏi số chứng từ |
| Cột "Còn nợ" | Không thấy trong khung hình (có thể ở ngoài vùng cuộn ngang) | Có | ⚪ Chưa kết luận được — không đủ ảnh để khẳng định MISA có/không |
| "Số dòng = N" ở footer (đếm tổng dòng tải về, không phải dòng đã tick) | Có | Có | ✅ |
| Nút "Xem chứng từ thu tiền chưa ghi sổ" | Có | Không có | ⚪ Tính năng phụ, không chặn luồng chính |

### Popup 2 — Xác nhận (`BulkCustomerReceiptWindow`)

| Chi tiết | MISA | App hiện tại | Khớp? |
|---|---|---|---|
| Toolbar | **Trước·Sau·Thêm·Sửa·Sửa nhanh \| Cất·Xóa·Hoàn·Ghi sổ \| Nạp·Tiện ích \| Mẫu \| In·Xuất khẩu** — cửa sổ chứng từ đầy đủ, browse/sửa lại được | Chỉ **💾 Cất** + **Hủy bỏ** — popup tạo-1-lần-rồi-đóng | ❌ Thiếu rất nhiều: không Trước/Sau, không Sửa/Xóa/Hoàn riêng, không In, không Xuất khẩu |
| Người nộp / Địa chỉ / Lý do nộp / NV thu nợ / Kèm theo / Tham chiếu | Có đủ | Có đủ, đúng tên field | ✅ |
| Tham chiếu hiện dạng link bấm được (BH05457, BH05459…) | Có (link xanh) | Chữ thường, không bấm được | ⚪ Polish nhỏ |
| Ngày hạch toán / Ngày chứng từ / Số chứng từ | Có | Có | ✅ |
| **Tab "1. Hạch toán" — GỘP theo khách hàng**: 1 khách hàng có nhiều chứng từ → **CHỈ 1 dòng**, Số tiền = tổng các chứng từ của khách đó (vd ảnh 6: Loan Spa 8.249.800 = 2.386.800 + 5.863.000 của 2 chứng từ BH05132+BH05168) | Có gộp | **Không gộp** — `Lines` là 1 dòng/1 chứng từ đã chọn, tab "Hạch toán" bind thẳng cùng collection với tab "Chứng từ" nên số dòng 2 tab LUÔN BẰNG NHAU | ❌ **Khác biệt lớn nhất** — xem "Cách xử lý" bên dưới |
| Tab "1. Hạch toán" cột: Diễn giải, TK Nợ, TK Có, Số tiền, Mã KH, Tên KH | Có + thêm cột "Khoản mục CP" (luôn trống) | Có, thiếu cột thừa đó | ⚪ Không đáng ngại — cột MISA cũng trống |
| Tab "2. Chứng từ" — 1 dòng/1 chứng từ gốc, đủ cột Hạn TT/Số phải thu/Số chưa thu/Số thu/TK phải thu/Điều khoản TT | Có | Có, đúng thứ tự cột | ✅ |
| Cột "Số hóa đơn", "Tỷ lệ CK(%)", "Tiền chiết khấu", "TK chiết khấu" ở tab Chứng từ | Có (luôn trống/0 trong ảnh mẫu) | Không có | ⚪ Không áp dụng — app chưa có chiết khấu khi thu nợ |
| Cất = chỉ lưu Draft, KHÔNG tự ghi sổ | Đúng — nút Cất và Ghi sổ tách riêng | Đúng — `CreateBulkCustomerReceiptUseCase` chỉ tạo Draft | ✅ **Xác nhận đúng, không phải bug** |
| Nút Ghi sổ ngay trong popup 2 | Có | Không có — phải đóng popup, ra màn Quỹ mới Ghi sổ được | ❌ Thiếu |

### Cách xử lý phần "gộp theo khách hàng" (điểm khác biệt lớn nhất)

Không thể gộp thật ở tầng DB/BE: `ReceiptEntry.SalesOrderId` là 1 giá trị (không phải mảng) — `GetRemainingAmountAsync` dựa vào `ReceiptEntries.Where(e => e.SalesOrderId == salesOrderId)` để tính lại số còn nợ, nên **mỗi chứng từ bán hàng bắt buộc phải có 1 `ReceiptEntry` riêng** để còn theo dõi đúng nợ từng đơn — gộp cứng ở BE sẽ làm hỏng việc tính công nợ.

→ Cách khớp MISA an toàn: **gộp chỉ ở hiển thị**, riêng cho tab "1. Hạch toán" — nhóm `Lines` theo `CustomerId`, hiển thị 1 dòng/khách hàng (Số tiền = tổng), còn dữ liệu gửi lên BE (`Lines.Select(...)` trong `SaveAsync`) **giữ nguyên 1 dòng/chứng từ như hiện tại**. Tab "2. Chứng từ" không đổi gì.

### Việc cần quyết định trước khi code

1. Có cần **gộp hiển thị theo khách hàng** ở tab "1. Hạch toán" không? (đổi UI thuần, không đổi dữ liệu gửi BE)
2. Có cần thêm **"Ngày thu tiền"** vào popup 1 để chọn 1 lần, đổ sẵn vào popup 2 không?
3. Có cần thêm **tổng tiền sống** ("Số tiền") ở popup 1 khi tick/bỏ tick không?
4. Toolbar đầy đủ kiểu MISA (Trước/Sau để browse phiếu cũ, Sửa/Xóa/Hoàn/Ghi sổ riêng, In, Xuất khẩu) là thay đổi **lớn** — biến popup 2 từ "tạo 1 lần rồi đóng" thành 1 cửa sổ chứng từ đầy đủ như `ReceiptWindow`. Có muốn làm việc này không, hay giữ nguyên đơn giản và chỉ Ghi sổ từ màn Quỹ như hiện tại?

---

*Generated by `/ct-ai-document` on 2026-09-26*

---

## Update — 2026-09-26: đã triển khai theo review ở trên

Cả 4 điểm review đều đã làm, theo đúng câu trả lời của bạn (tất cả đều "Có"):

| Việc | Trạng thái |
|---|---|
| Gộp tab "1. Hạch toán" theo khách hàng | ✅ Xong — chỉ gộp lúc hiển thị (`BulkReceiptGroupedLine`, WPF), **không đổi** dữ liệu gửi lên BE — vẫn 1 `ReceiptEntry`/1 hóa đơn để không phá công thức tính số còn nợ |
| "Ngày thu tiền" + tổng tiền sống ở popup tìm kiếm | ✅ Xong — `BulkCustomerReceiptSearchViewModel.CollectionDate`/`SelectedTotal` |
| Popup xác nhận → cửa sổ chứng từ đầy đủ | ✅ Xong — `BulkCustomerReceiptWindow` giờ dùng chung `DocumentToolbar` (Trước/Sau/Thêm/Sửa/Xóa/Cất/Ghi sổ-Bỏ ghi), thêm nút Xuất khẩu riêng. **Không có nút In** — app chưa có mẫu in phiếu thu nào (kể cả phiếu thu thường) |
| "Tham chiếu" dạng link | ✅ Xong — mỗi số chứng từ trong Tham chiếu bấm mở lại đúng `SalesOrderWindow` (chỉ xem), qua `IGetSalesOrderByIdUseCase` (cross-feature, cùng cách `InventoryDetailViewModel` đã làm) |

### Thay đổi kiến trúc quan trọng

- **BE mới:** `GET /api/v1/accounting/receipts/sales-orders?ids=1,2,3` (`GetSalesOrdersByIdsUseCase`) — dựng lại tab "2. Chứng từ" khi Sửa 1 phiếu đã lưu (không lọc còn nợ/ngày, khác `outstanding-orders`).
- **Popup tìm kiếm giờ CHỈ LÀ BỘ CHỌN** — "✔ Thu tiền" không tự tạo/mở phiếu nữa, chỉ đóng lại và trả `SelectedItems`/`PaymentMethod`/`BankAccount`/`SelectedEmployee`/`CollectionDate` cho `BulkCustomerReceiptViewModel.AddNewAsync` tự dựng dòng + lưu.
- **Sửa 1 phiếu đã lưu** dùng thẳng `IUpdateReceiptUseCase` (generic, sẵn có) — **CHƯA có** validate "không vượt số còn nợ" như lúc tạo mới (`CreateReceiptUseCase` có check này, `UpdateReceiptUseCase` thì không) — lỗ hổng có sẵn từ trước, không phải do đợt này, nhưng giờ dễ gặp hơn vì Sửa phiếu hàng loạt giờ khả thi.
- **Chưa fix:** double-click 1 dòng phiếu thu hàng loạt trên sổ quỹ vẫn mở nhầm `ReceiptWindow` thường (báo lỗi "Vui lòng chọn đối tượng") — cần thêm `customer_id` vào `CashLedgerEntryDto` để `AccountingViewModel.ViewEntry()` biết đường định tuyến đúng cửa sổ. Ngoài phạm vi đợt review này.
- **Cố ý không làm:** "Sửa nhanh", "Nạp", "Tiện ích", "Mẫu" — không có tính năng tương ứng ở bất kỳ đâu khác trong app.

Chưa chạy thử trên UTM — xem checklist kiểm tra ở tin nhắn chat.
