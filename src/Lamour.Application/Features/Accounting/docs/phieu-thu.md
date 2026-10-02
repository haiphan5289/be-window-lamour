# Phiếu Thu (Receipt) — Feature Document

> **Jira:** — (branch `dev` không có mã ticket) | **Branch:** `dev` | **Generated:** 2026-10-02 (viết lại theo code hiện tại, chưa commit)
> **Nguồn:** đọc trực tiếp code BE (`be-window-lamour`) + WPF (`desktop-lamour`). Jira/Confluence không lấy được (Atlassian MCP chưa đăng nhập).
> Bản cũ dạng nhật ký thay đổi (2026-04 → 2026-10-01): [phieu-thu-old.md](phieu-thu-old.md).
> Tài liệu liên quan: [quy.md](quy.md) (Sổ quỹ) · [phieu-chi.md](phieu-chi.md) · [phieu-thu-hang-loat.md](phieu-thu-hang-loat.md) · [bao-cao-quy.md](bao-cao-quy.md)

---

## PRD Summary

- **Goal:** Lập phiếu thu tiền mặt theo đúng luồng MISA: chọn đối tượng → lý do nộp → nội dung → dòng hạch toán tự điền → TK Có → Số tiền → Cất (lưu + ghi sổ quỹ ngay) → In mẫu 01-TT.
- **User story:** Là kế toán/thu ngân, tôi muốn lập phiếu thu cho khách hàng hoặc nhân viên, để khoản thu được ghi vào sổ quỹ tiền mặt và in ra phiếu có chữ ký.
- **Acceptance criteria** (đối chiếu code hiện tại):
  - [x] Đối tượng là 1 ô tìm chung Khách hàng + Nhân viên
  - [x] Lý do nộp: Rút tiền gửi về nộp quỹ · Thu hoàn thuế GTGT · Thu hoàn ứng · Thu khác (mặc định)
  - [x] Chọn lý do → ô nội dung tự điền nhãn lý do; Diễn giải dòng hạch toán đi theo ô nội dung
  - [x] Dòng tự điền TK Nợ 1111 / TK Có 1388, Đối tượng, Tên đối tượng
  - [x] Gõ mã TK trực tiếp trong ô (lọc danh sách, Enter/rời ô là chọn)
  - [x] Cất = lưu + ghi sổ ngay; Ghi sổ/Bỏ ghi là 1 nút; Sửa/Xóa chỉ khi chưa ghi sổ
  - [x] In mẫu 01-TT giống bản in MISA
  - [ ] Chặn Số tiền âm: **chưa**
  - [ ] Chặn trùng Số chứng từ: **chưa**
  - [ ] Kiểm chứng giao diện trên UTM: **chưa** (mới build trên Mac)

---

## Business Rules

| Rule | Description |
|------|-------------|
| Đối tượng đa loại | `Receipt.PartnerType` (`Customer`/`Employee`, dùng chung enum `PaymentPartnerType`; `Supplier` bị từ chối) + `PartnerId` + `PartnerName` (cache lúc lưu). Không FK thật. Phiếu của Khách hàng vẫn điền `CustomerId` (= `PartnerId`); phiếu của Nhân viên có `CustomerId = null` |
| Phiếu hàng loạt | `Receipt.IsBulk` = `PartnerType == null` (**không** dùng `CustomerId == null` nữa). Không có đối tượng chỉ hợp lệ khi `payment_reason = ThuKhachHangHangLoat`. Chi tiết: [phieu-thu-hang-loat.md](phieu-thu-hang-loat.md) |
| Người nộp / Địa chỉ | WPF tự điền `PayerName` = tên đối tượng; Địa chỉ chỉ tự điền khi đối tượng là Khách hàng. Sửa tay được |
| Lý do nộp | Enum `PaymentReason` lưu dạng chuỗi. Chọn được: `RutTienGuiVeNopQuy`, `ThuHoanThueGTGT`, `ThuHoanUng`, `ThuKhac` (mặc định). `ThuTienHang`/`ThuCongNo` chỉ còn cho phiếu cũ (mở vẫn hiện đúng, không chọn mới). `ThuKhachHangHangLoat` chỉ BE gán cho phiếu hàng loạt |
| Lý do nộp chi tiết | `ReasonDetail` (tối đa 500, BE trim, rỗng → `null`). WPF: đổi lý do thì ô nội dung tự điền nhãn lý do **nếu** ô đang trống hoặc còn mang nhãn lý do trước |
| Lưới trống khi mở | Form mở sẵn 50 dòng trống, hiện trống hẳn (Số tiền 0 = ô trống, ẩn nút ✕). Dòng **đầu** tự điền sau khi chọn Đối tượng; dòng khác tự điền khi gõ Số tiền. Đổi Đối tượng / nội dung chỉ cập nhật dòng đầu và dòng đã có số tiền |
| TK mặc định | TK Nợ **1111**, TK Có **1388** (đổi tay được). Danh mục chưa có mã đó thì ô để trống |
| TK là FK thật | `ReceiptEntry.DebitAccountSettingId` / `CreditAccountSettingId` → `AccountSetting` (Restrict). BE: "Vui lòng chọn tài khoản Nợ/Có." (id = 0) · "Tài khoản Nợ/Có không tồn tại." |
| Gõ mã TK | Ô TK là `AppSearchableComboBox` dạng gọn: gõ là lọc; Enter/rời ô chọn mã khớp hẳn → mã bắt đầu bằng chữ đã gõ → dòng đầu danh sách lọc; không khớp thì trả về TK cũ; xoá trắng = bỏ chọn |
| Dòng gửi lên | Chỉ dòng có `Amount != 0`. WPF chặn trước: chưa chọn đối tượng, không có dòng nào, dòng có tiền thiếu TK |
| Số chứng từ | WPF lấy từ `GET receipts/next-code` (`PT` + 5 số) khi bấm Thêm; sửa tay được. BE **không** kiểm tra trùng |
| Trạng thái | `ReceiptStatus.Draft` (= "Treo", chưa ghi sổ) · `Confirmed`. Không có trạng thái Treo riêng như phiếu chi |
| Cất = Lưu + Ghi sổ | WPF gọi Create/Update rồi Confirm ngay. Confirm lỗi thì phiếu vẫn lưu ở Treo, banner báo lỗi |
| Ghi sổ | `ConfirmReceiptUseCase`: chỉ khi `Draft`; tạo 1 `CashTransaction` (Nợ = tổng các dòng) rồi `Confirmed` |
| Bỏ ghi | `UnconfirmReceiptUseCase`: chỉ khi `Confirmed`; xoá `CashTransaction` theo số phiếu, về `Draft` |
| Bất biến sau ghi sổ | Sửa/Xóa phiếu `Confirmed` bị chặn ở WPF (tắt nút) và BE |
| Sổ quỹ | `CashTransaction.Account` = TK cấp 1 của TK Nợ dòng đầu (1111 → `111`, 1121 → `112`); `CounterAccount` = mã TK Có dòng đầu. `DocumentType`: Nhân viên → "Phiếu thu", Khách hàng → "Phiếu thu tiền mặt khách hàng", hàng loạt → "Phiếu thu tiền mặt khách hàng hàng loạt". Diễn giải trên màn Quỹ = `ReasonDetail`, trống thì nhãn lý do |
| Bản in 01-TT | Nợ/Có = mã TK thật của các dòng; Số tiền = Σ dòng; Lý do nộp = `ReasonDetail`, trống thì nhãn lý do; 5 chữ ký (Giám đốc · Kế toán trưởng · Người nộp tiền · Người lập phiếu · Thủ quỹ) |
| Trước/Sau | Cửa sổ Phiếu thu chỉ duyệt phiếu thường (`PartnerType != null`); phiếu hàng loạt có cửa sổ riêng |

### Vòng đời

```
        Cất (Create/Update + Confirm)
(mới) ───────────────────────────────► Confirmed ──── Bỏ ghi ────► Draft (Treo)
                                           ▲                          │
                                           └──────── Ghi sổ ──────────┤
                                                                      │ Sửa → Cất
                                                                      ▼
                                                                (Update + Confirm)
```

---

## Architecture Overview

### Key Components

| Layer | File | Role |
|-------|------|------|
| Domain | `Lamour.Domain/Entities/Receipt.cs` | `Receipt` + `ReceiptStatus`, `IsBulk` |
| Domain | `Lamour.Domain/Entities/ReceiptEntry.cs` | Dòng hạch toán (TK Nợ/Có FK, Số tiền, Đối tượng, `SalesOrderId`) |
| Domain | `Lamour.Domain/Enums/PaymentReason.cs`, `PaymentPartnerType.cs` | Lý do, loại đối tượng |
| API | `Lamour.Api/Controllers/ReceiptsController.cs` | `api/v1/accounting/receipts`, `[Authorize]` |
| UseCase | `CreateReceiptUseCase`, `UpdateReceiptUseCase`, `DeleteReceiptUseCase` | Lưu / sửa / xoá |
| UseCase | `ReceiptEntryBuilder` (static) | Dùng chung: parse lý do, resolve đối tượng, build + validate dòng, `LedgerAccount`, `DocumentType` |
| UseCase | `ConfirmReceiptUseCase`, `UnconfirmReceiptUseCase` | Ghi sổ / Bỏ ghi |
| UseCase | `GetReceiptsUseCase` (`MapToDto`), `GetReceiptByIdUseCase`, `GetNextReceiptCodeUseCase` | Đọc |
| Repository | `Lamour.Infrastructure/Repositories/ReceiptRepository.cs` | CRUD + `GetUnconfirmedByDateRangeAsync`, `GetIdsByDocumentNumbersAsync`, `GetByDocumentNumbersAsync`, `GetReasonDetailsByIdsAsync`, `GetBulkReceiptIdsAsync` |
| EF | `Persistence/Configurations/ReceiptConfiguration.cs` | Bảng `receipts`, `receipt_entries` |
| Migration | `20261001131614_ReceiptPartnerAndAccountSettings` | Thêm đối tượng + `ReasonDetail`, đổi TK enum → FK, **chuyển dữ liệu cũ** |
| Migration | `20261001133908_AddReceiptDefaultCreditAccount1388` | Chèn TK 1388 "Phải thu khác" nếu thiếu |
| WPF — cửa sổ | `Accounting/Views/ReceiptWindow.xaml(.cs)` | `DocumentToolbar`, Thông tin chung, lưới Hạch toán |
| WPF — ViewModel | `Accounting/ViewModels/ReceiptViewModel.cs` | Logic nhập liệu / lưu / ghi sổ / in |
| WPF — model dòng | `Accounting/Domain/Models/ReceiptEntryItem.cs` | `SelectedDebitAccount`, `SelectedCreditAccount`, `IsEmpty` |
| WPF — bản in | `Accounting/Views/ReceiptPrintWindow.xaml(.cs)` + `CashVoucherDocumentBuilder.cs` | Mẫu 01-TT |
| WPF — control | `Shared/Controls/AppSearchableComboBox.xaml(.cs)` | Ô TK (`IsCompact`, `CodeOnlyDisplay`, `CommitTypedText`) |
| WPF — nhãn | `Shared/Converters/PaymentReasonDisplayConverter.cs` | `Label(reason)` |

### Data Flow

```
Màn Quỹ → Thêm ▾ → Phiếu thu (AccountingViewModel.OpenReceipt → ReceiptWindow.Show)
  ReceiptViewModel.LoadAsync → nạp Khách hàng, Nhân viên, Tài khoản kế toán → GET receipts
  ➕ Thêm → form trống + 50 dòng trống, GET receipts/next-code
     chọn Đối tượng → Người nộp, Địa chỉ, dòng đầu (Diễn giải, 1111/1388, Đối tượng)
     đổi Lý do nộp → ô nội dung → Diễn giải
  💾 Cất → POST receipts | PUT receipts/{id} → POST receipts/{id}/confirm → CashTransaction
        → tải lại, khoá form, báo màn Quỹ tải lại
  Ghi sổ / Bỏ ghi → POST {id}/confirm | {id}/unconfirm
  🗑️ Xóa → hỏi Yes/No → DELETE {id} → đóng cửa sổ
  🖨 In → ReceiptPrintWindow
```

---

## Key Files & Symbols

### BE (`be-window-lamour/src`)
- [`ReceiptsController.cs`](../../../../Lamour.Api/Controllers/ReceiptsController.cs)
- [`CreateReceiptUseCase.cs`](../UseCases/CreateReceiptUseCase.cs) / [`UpdateReceiptUseCase.cs`](../UseCases/UpdateReceiptUseCase.cs) / [`DeleteReceiptUseCase.cs`](../UseCases/DeleteReceiptUseCase.cs)
- [`ReceiptEntryBuilder.cs`](../UseCases/ReceiptEntryBuilder.cs): `ParseReason`, `ResolvePartnerAsync`, `ApplyPartner`, `BuildEntriesAsync`, `LedgerAccount`, `DocumentType`
- [`ConfirmReceiptUseCase.cs`](../UseCases/ConfirmReceiptUseCase.cs) / [`UnconfirmReceiptUseCase.cs`](../UseCases/UnconfirmReceiptUseCase.cs)
- [`IReceiptRepository.cs`](../Repositories/IReceiptRepository.cs) / [`ReceiptRepository.cs`](../../../../Lamour.Infrastructure/Repositories/ReceiptRepository.cs)
- DTOs: [`CreateReceiptRequestDto.cs`](../Dtos/CreateReceiptRequestDto.cs), [`UpdateReceiptRequestDto.cs`](../Dtos/UpdateReceiptRequestDto.cs), [`ReceiptResponseDto.cs`](../Dtos/ReceiptResponseDto.cs), [`ReceiptEntryDto.cs`](../Dtos/ReceiptEntryDto.cs)

### WPF (`desktop-lamour/src/DesktopLamour`)
- `Features/HomePage/Accounting/ViewModels/ReceiptViewModel.cs`: `AddNewAsync`, `Edit`, `SaveAsync` (Cất), `ToggleConfirmAsync`, `DeleteAsync`, `Print`, `AddEntry`, `IsActiveEntry`, `ApplyEntryDefaults`, `AttachEntryHandlers`, `OnSelectedPartnerChanged`, `OnSelectedPaymentReasonChanged`, `OnReasonDetailChanged`, `RefreshPaymentReasons`, `ResolvePartnerType`
- `Features/HomePage/Accounting/Views/ReceiptWindow.xaml`: style `EntryTextCell`; cột TK dùng `controls:AppSearchableComboBox`
- `Features/HomePage/Accounting/Data/Services/ReceiptService.cs` + `Dtos/` (cùng shape với BE)

### Màn hình

**Cửa sổ "Phiếu thu"**
- Toolbar (`DocumentToolbar`): Trước · Sau · Thêm · Sửa · Xóa · Cất · Ghi sổ/Bỏ ghi · In · Đóng
- Thông tin chung: Đối tượng · Người nộp · Địa chỉ · Lý do nộp + nội dung · Nhân viên thu (nút ➕) · Kèm theo · Tham chiếu
- Chứng từ: Ngày hạch toán · Ngày chứng từ · Số chứng từ
- "1. Hạch toán": Diễn giải · TK Nợ · TK Có · Số tiền · Đối tượng · Tên đối tượng · TK ngân hàng · ✕. Phím: F9 thêm dòng; chuột phải / Ctrl+Insert / Ctrl+Delete thêm/xoá dòng

**Bản in "PHIẾU THU" (mẫu 01-TT)**: Header công ty + Mẫu số 01 - TT · Ngày · Quyển số · Số · Nợ · Có · Họ tên người nộp tiền · Địa chỉ · Lý do nộp · Số tiền · Viết bằng chữ · Kèm theo · 5 chữ ký

---

## API Contracts

Base route: `api/v1/accounting/receipts`

| Method | Endpoint | Input | Output | Lỗi |
|--------|----------|-------|--------|-----|
| `GET` | `/` | — | `ReceiptResponseDto[]` | |
| `GET` | `/{id}` | — | `ReceiptResponseDto` | 404 |
| `POST` | `/` | `CreateReceiptRequestDto` | `ReceiptResponseDto` (Draft) | 400 lý do / đối tượng / TK sai |
| `PUT` | `/{id}` | `UpdateReceiptRequestDto` | `ReceiptResponseDto` | 404; 400 nếu đã ghi sổ |
| `DELETE` | `/{id}` | — | 204 | 404; 400 nếu đã ghi sổ |
| `POST` | `/{id}/confirm` | — | `ReceiptResponseDto` (Confirmed) | 400 đã ghi sổ |
| `POST` | `/{id}/unconfirm` | — | `ReceiptResponseDto` (Draft) | 400 chưa ghi sổ |
| `GET` | `/next-code` | — | số chứng từ PT kế tiếp | |
| `GET` `/outstanding-orders` · `POST` `/bulk` · `GET` `/sales-orders` | xem [phieu-thu-hang-loat.md](phieu-thu-hang-loat.md) | | | |

### Request — `POST /` (`PUT /{id}` cùng field)

```json
{
  "partner_type": "Employee",
  "partner_id": 16,
  "payer_name": "TRẦN THỊ NHI TRÚC",
  "address": null,
  "payment_reason": "ThuKhac",
  "reason_detail": "nhập quỹ",
  "collector_employee_id": null,
  "attachment": null,
  "reference": null,
  "accounting_date": "2026-09-30T00:00:00",
  "document_date": "2026-09-30T00:00:00",
  "document_number": "PT00106",
  "entries": [
    {
      "description": "nhập quỹ",
      "debit_account_id": 45,
      "credit_account_id": 47,
      "amount": 50000000,
      "subject_code": "NV016",
      "subject_name": "TRẦN THỊ NHI TRÚC",
      "bank_account": null,
      "sales_order_id": null
    }
  ]
}
```

> Id tài khoản là minh hoạ (khác nhau giữa các máy). Entry của **phiếu hàng loạt** không gửi `*_account_id` mà gửi `debit_account: "Cash111"|"Bank112"`, `credit_account: "Receivable131"`; BE tra sang danh mục (Cash111→1111, Bank112→1121, Receivable131→131, Payroll334→334).

### Response (field thêm so với request)

`id`, `partner_name`, `customer_id`, `customer_name`, `collector_employee_name`, `status` (`Draft`/`Confirmed`), `confirmed_at`, `created_at`; mỗi entry thêm `id`, `debit_account_code`, `debit_account_description`, `credit_account_code`, `credit_account_description`.

---

## Edge Cases & Error Handling

| Scenario | Expected Behavior | Handled? |
|----------|------------------|----------|
| Chưa chọn Đối tượng mà bấm Cất | "Vui lòng chọn đối tượng." | ✅ WPF + BE |
| Không dòng nào có Số tiền | "Vui lòng nhập ít nhất một dòng hạch toán." | ✅ WPF |
| Dòng có tiền nhưng thiếu TK Nợ/Có | "Vui lòng chọn TK Nợ và TK Có cho mọi dòng có số tiền." | ✅ WPF (BE cũng chặn) |
| Đối tượng là Nhà cung cấp | "Đối tượng của phiếu thu chỉ là Khách hàng hoặc Nhân viên." | ✅ BE |
| Đối tượng đã bị xoá | "Khách hàng / Nhân viên không tồn tại." | ✅ BE |
| Lưu được nhưng Ghi sổ lỗi | Phiếu ở Treo, banner "Đã lưu phiếu (Treo) nhưng ghi sổ thất bại: …" | ✅ WPF |
| Sửa / Xóa phiếu đã ghi sổ | WPF tắt nút; BE: "Chứng từ đã ghi sổ, không thể sửa/xóa. Bỏ ghi trước khi sửa/xóa." | ✅ |
| Mở phiếu cũ có lý do đã bỏ | Ô Lý do nộp vẫn hiện đúng giá trị cũ | ✅ WPF |
| Gõ mã TK không có trong danh mục | Ô trả về TK trước khi gõ | ✅ WPF (chưa kiểm chứng trên UTM) |
| Số tiền âm | Được lưu và ghi sổ → làm **giảm** quỹ | ⚠️ Chưa chặn |
| Trùng Số chứng từ | Lưu được; sổ quỹ tra id theo số → lấy phiếu tạo sau cùng; Bỏ ghi xoá `CashTransaction` theo số → ảnh hưởng cả 2 phiếu | ⚠️ Chưa chặn |
| TK Nợ là 112x (thu bằng tiền gửi) | Vẫn hiện trên màn Quỹ và cộng vào tồn quỹ; **không** vào Báo cáo sổ quỹ tiền mặt | ⚠️ Hai nơi lệch nhau |
| Xoá TK đang được phiếu dùng | DB chặn (FK Restrict), lỗi chưa thân thiện (`IsInUseAsync` chưa kiểm tra dòng phiếu) | ⚠️ |

---

## Test Coverage Notes

| Component | Test File | Coverage |
|-----------|-----------|----------|
| `CreateReceiptUseCase` + `ReceiptEntryBuilder` | `tests/Lamour.Application.Tests/Features/Accounting/UseCases/CreateReceiptUseCaseTests.cs` | ✅ 5 test (NV không là hàng loạt; KH điền `CustomerId`; đối tượng sai; thiếu TK Có; phiếu hàng loạt tra TK từ tên cũ) |
| `ConfirmReceiptUseCase` | `ConfirmReceiptUseCaseTests.cs` | ✅ 2 test (TK sổ quỹ + Loại chứng từ theo đối tượng; đã ghi sổ bị chặn) |
| Diễn giải / hàng loạt trên sổ quỹ | `GetCashLedgerUseCaseTests.cs` | ✅ |
| `UpdateReceiptUseCase` / `DeleteReceiptUseCase` / `UnconfirmReceiptUseCase` | — | ❌ Chưa có |
| `ReceiptViewModel` (WPF) | — | ❌ Chưa có (project test WPF đang lỗi build sẵn) |
| Migration chuyển dữ liệu | — | Chạy tay up → down → up trên DB local |

**Suggested test cases:**
- [ ] Update / Delete phiếu `Confirmed` → `DomainException`
- [ ] Unconfirm xoá đúng `CashTransaction` theo số phiếu, trả về `Draft`
- [ ] WPF: `OnSelectedPaymentReasonChanged` không ghi đè nội dung người dùng tự gõ
- [ ] WPF: dòng thứ hai chỉ tự điền khi gõ Số tiền

---

## Notes

- **Deploy:** phải chạy 2 migration (`ReceiptPartnerAndAccountSettings`, `AddReceiptDefaultCreditAccount1388`). Migration đầu **sửa dữ liệu phiếu thu đã lưu** — backup DB trước.
- **Chưa kiểm chứng trên UTM:** toàn bộ giao diện Phiếu thu mới (ô TK gõ được, lưới trống, in). Mac chỉ build được, không chạy được WPF.
- Số tiền âm và trùng Số chứng từ chưa bị chặn — cần quyết định với kế toán trước khi thêm ràng buộc.
- Số tiền nhập tay; chưa có cơ chế "tự động nhập số tiền" (bước 6 trong yêu cầu gốc chưa rõ nguồn số tiền).

---

*Generated by `/ct-ai-document` on 2026-10-02*
