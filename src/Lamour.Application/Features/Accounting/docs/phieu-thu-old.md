# Phiếu Thu — BE Documentation

> Feature: Phiếu Thu (Cash Receipt)
> Module: Accounting
> Rebuilt: 2026-04-29 (replaced old PaymentReceipt design)

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `GET`    | `/api/v1/accounting/receipts`       | Bearer | Lấy danh sách phiếu thu |
| `GET`    | `/api/v1/accounting/receipts/{id}`  | Bearer | Lấy phiếu thu theo ID |
| `POST`   | `/api/v1/accounting/receipts`       | Bearer | Tạo phiếu thu mới |
| `PUT`    | `/api/v1/accounting/receipts/{id}`  | Bearer | Cập nhật phiếu thu |
| `DELETE` | `/api/v1/accounting/receipts/{id}`  | Bearer | Xóa phiếu thu |
| `GET`    | `/api/v1/accounting/receipts/next-code` | Bearer | Số chứng từ "PT" tiếp theo |
| `GET`    | `/api/v1/accounting/receipts/outstanding-orders` | Bearer | Chứng từ bán hàng còn nợ (popup "Thu tiền khách hàng hàng loạt") |
| `POST`   | `/api/v1/accounting/receipts/bulk`  | Bearer | Tạo 1 phiếu thu hàng loạt (nhiều khách hàng, xem mục riêng bên dưới) |
| `POST`   | `/api/v1/accounting/receipts/{id}/confirm`   | Bearer | "Ghi sổ" — Draft → Confirmed, post `CashTransaction` (2026-09-01) |
| `POST`   | `/api/v1/accounting/receipts/{id}/unconfirm` | Bearer | "Bỏ ghi" — Confirmed → Draft, xóa `CashTransaction` (2026-09-01) |

## Request — POST / PUT

```json
{
  "customer_id": 1,
  "payer_name": "Nguyễn Văn A",
  "address": null,
  "payment_reason": "ThuKhac",
  "collector_employee_id": null,
  "attachment": null,
  "reference": null,
  "accounting_date": "2026-04-29T00:00:00",
  "document_date": "2026-04-29T00:00:00",
  "document_number": "PT00067",
  "entries": [
    {
      "id": 0,
      "description": "Thu tiền khách hàng",
      "debit_account": "Cash111",
      "credit_account": "Receivable131",
      "amount": 1000000,
      "subject_code": null,
      "subject_name": null,
      "bank_account": null
    }
  ]
}
```

**Validation:**
- `customer_id` phải tồn tại → `DomainException` → 400
- `document_number` required
- `payment_reason` phải là enum hợp lệ: `ThuKhac`, `ThuTienHang`, `ThuCongNo`
- `debit_account` / `credit_account` phải là enum hợp lệ: `Cash111`, `Bank112`, `Receivable131`, `Payroll334`
- **PUT chỉ cho phép khi `status = Draft`** (2026-09-01) — `DomainException` nếu đã `Confirmed` ("Chỉ chứng từ ở trạng thái Nháp mới được sửa. Bỏ ghi trước khi sửa.")

## Response — 201 Created / 200 OK

```json
{
  "id": 1,
  "customer_id": 1,
  "customer_name": "Nguyễn Văn A",
  "payer_name": "Nguyễn Văn A",
  "address": null,
  "payment_reason": "ThuKhac",
  "collector_employee_id": null,
  "collector_employee_name": null,
  "attachment": null,
  "reference": null,
  "accounting_date": "2026-04-29T00:00:00Z",
  "document_date": "2026-04-29T00:00:00Z",
  "document_number": "PT00067",
  "status": "Draft",
  "confirmed_at": null,
  "created_at": "2026-04-29T08:00:00Z",
  "entries": [
    {
      "id": 1,
      "description": "Thu tiền khách hàng",
      "debit_account": "Cash111",
      "credit_account": "Receivable131",
      "amount": 1000000,
      "subject_code": null,
      "subject_name": null,
      "bank_account": null
    }
  ]
}
```

## Enums

### PaymentReason
```
ThuKhac               — Thu khác
ThuTienHang           — Thu tiền hàng
ThuCongNo             — Thu công nợ
ThuKhachHangHangLoat  — Phiếu thu tiền mặt khách hàng hàng loạt (2026-09-28, chỉ BE tự gán khi
                         CreateBulkCustomerReceiptUseCase tạo phiếu — không có trong dropdown
                         "Lý do nộp" của popup Phiếu thu thường)
```

### AccountCode (TK Nợ / TK Có)
```
Cash111        — 111 Tiền mặt
Bank112        — 112 Tiền gửi ngân hàng
Receivable131  — 131 Phải thu khách hàng
Payroll334     — 334 Phải trả người lao động
```

## Số chứng từ

`document_number` là free-text do người dùng nhập thủ công (ví dụ: `PT00067`).
Không có auto-generate hay sequence trên BE.

## Side Effect — CashTransaction (Quỹ Tiền Mặt)

> **Đã đổi (2026-09-01):** Trước đây `CashTransaction` được sync ngay khi **tạo**/**cập nhật**
> Receipt. Nay side-effect này **chỉ xảy ra khi Confirm/Unconfirm** ("Ghi sổ"/"Bỏ ghi") — giống
> hệt pattern của `Payment`. Xem mục "Draft/Confirmed Status Workflow" bên dưới.

Khi **Confirm** ("Ghi sổ") 1 Receipt đang `Draft`, tạo mới 1 row `CashTransaction` (không sync lại
khi Update nữa vì Update chỉ được phép ở trạng thái `Draft`, thứ chưa từng có `CashTransaction`):

| Field           | Value                                                        |
|-----------------|--------------------------------------------------------------|
| `AccountingDate`| `receipt.AccountingDate`                                     |
| `DocumentDate`  | `receipt.DocumentDate`                                       |
| `ReceiptNumber` | `receipt.DocumentNumber`                                     |
| `Description`   | `receipt.PayerName` (chỉ tên người nộp, không có prefix)     |
| `Account`       | TK Nợ của entry đầu tiên → mapped "111"/"112"/"131"/"334"   |
| `CounterAccount`| TK Có của entry đầu tiên → mapped "111"/"112"/"131"/"334"   |
| `DebitAmount`   | tổng `entries.Sum(e => e.Amount)`                            |
| `CreditAmount`  | `0`                                                          |
| `PersonName`    | `receipt.PayerName`                                          |
| `PaymentReason` | `receipt.PaymentReason` (string?, thêm 2026-08-28)           |
| `DocumentType`  | `"Phiếu thu tiền mặt khách hàng"` (hoặc `"...hàng loạt"` nếu `CustomerId == null`) |

**Confirm flow** (`ConfirmReceiptUseCase`): validate `Status == Draft` → tạo `CashTransaction` →
`Status = Confirmed`, `ConfirmedAt = DateTime.UtcNow`.
**Unconfirm flow** (`UnconfirmReceiptUseCase`): validate `Status == Confirmed` → xóa
`CashTransaction` theo `DocumentNumber` (`ICashLedgerRepository.DeleteByReceiptNumberAsync`) →
`Status = Draft`, `ConfirmedAt = null`.
**Delete flow**: chỉ cho phép khi `Draft` (chưa từng có `CashTransaction`) → xóa thẳng Receipt,
không còn bước xóa `CashTransaction`.

**`PaymentReason`/`DocumentType` (2026-08-28):** thêm 2 cột lên `CashTransaction` (migration `AddCashTransactionReasonAndDocType`) để màn "Sổ Kế Toán Chi Tiết Quỹ Tiền Mặt" (`GetCashLedgerUseCase`/`CashLedgerEntryDto`) hiển thị được "Lý do thu/chi" và "Loại chứng từ" ngay trên danh sách gộp — trước đó 2 field này chỉ có trên `Receipt`/`Payment` riêng, không denormalize xuống `CashTransaction` nên bên gộp Draft/Treo/Confirmed không có cách nào hiển thị thống nhất. `ConfirmReceiptUseCase` set `PaymentReason = receipt.PaymentReason`, `DocumentType = "Phiếu thu tiền mặt khách hàng"` khi ghi `CashTransaction` (trước 2026-09-01 do `CreateReceiptUseCase`/`UpdateReceiptUseCase` đảm nhiệm, nay chuyển sang `ConfirmReceiptUseCase`). Xem [`phieu-chi.md`](phieu-chi.md) cho phía Payment (`ConfirmPaymentUseCase`, `DocumentType = "Phiếu chi"`) và `desktop-lamour/.../Accounting/docs/phieu-thu.md` cho phần WPF (cột mới + click-để-xem/sửa/xóa trên `AccountingView`).

## Domain Entities

### Receipt

```
src/Lamour.Domain/Entities/Receipt.cs
```

| Column               | Type          | Notes                        |
|----------------------|---------------|------------------------------|
| `Id`                 | int           | PK                           |
| `CustomerId`         | int?          | FK → Customers (Restrict), **nullable từ 2026-08-26** — null cho "Phiếu thu tiền khách hàng hàng loạt" (xem mục "Phiếu thu hàng loạt" bên dưới), non-null cho phiếu thu 1 khách hàng bình thường |
| `PayerName`          | string(200)   | Người nộp                    |
| `Address`            | string?(500)  | Địa chỉ                      |
| `PaymentReason`      | string(30)    | Enum stored as string        |
| `CollectorEmployeeId`| int?          | FK → Employees (SetNull)     |
| `Attachment`         | string?(500)  | Kèm theo                     |
| `Reference`          | string?(200)  | Tham chiếu                   |
| `AccountingDate`     | datetime      | Ngày hạch toán (UTC)         |
| `DocumentDate`       | datetime      | Ngày chứng từ (UTC)          |
| `DocumentNumber`     | string(50)    | Số chứng từ — user input     |
| `Status`             | int           | `ReceiptStatus`: `Draft=0` (default), `Confirmed=1` — thêm 2026-09-01 |
| `ConfirmedAt`        | datetime?     | UTC, set khi Confirm, null lại khi Unconfirm — thêm 2026-09-01 |
| `CreatedAt`          | datetime      | UTC                          |

### ReceiptEntry

```
src/Lamour.Domain/Entities/ReceiptEntry.cs
```

| Column          | Type         | Notes                             |
|-----------------|--------------|-----------------------------------|
| `Id`            | int          | PK                                |
| `ReceiptId`     | int          | FK → Receipts (Cascade delete)    |
| `Description`   | string(500)  | Diễn giải                         |
| `DebitAccount`  | string(20)   | TK Nợ — enum stored as string     |
| `CreditAccount` | string(20)   | TK Có — enum stored as string     |
| `Amount`        | decimal(18,2)| Số tiền                           |
| `SubjectCode`   | string?(50)  | Đối tượng — dùng làm "Mã khách hàng" per-dòng cho phiếu thu hàng loạt (xem dưới) |
| `SubjectName`   | string?(200) | Tên đối tượng — "Tên khách hàng" per-dòng, cùng cơ chế trên |
| `BankAccount`   | string?(100) | TK ngân hàng                      |
| `SalesOrderId`  | int?         | FK → SalesOrders (Restrict) — chứng từ bán hàng gốc đang thu tiền, null nếu không gắn đơn hàng cụ thể |

---

## Phiếu thu tiền khách hàng hàng loạt (2026-08-26 — so ảnh mẫu MISA)

**Trước 2026-08-26:** `CreateBulkCustomerReceiptUseCase` nhận danh sách `(SalesOrderId, Amount)` đã chọn, **group theo `CustomerId`** rồi gọi `ICreateReceiptUseCase` **1 lần mỗi khách hàng** → N khách hàng khác nhau ra N phiếu thu riêng biệt (do lúc đó `Receipt.CustomerId` là FK bắt buộc, 1 phiếu chỉ gắn được 1 khách hàng).

**Sau 2026-08-26 (khớp ảnh mẫu MISA):** tạo **đúng 1 `Receipt` duy nhất** cho toàn bộ danh sách đã chọn, bất kể có bao nhiêu khách hàng khác nhau:

- `Receipt.CustomerId = null` (đã đổi sang `int?` — xem bảng entity ở trên).
- `Receipt.PayerName` = tên người nộp/nhân viên thu do user nhập ở popup xác nhận (`request.PayerName`), fallback về tên `CollectorEmployee` nếu bỏ trống, fallback tiếp về `"Thu tiền khách hàng hàng loạt"` nếu cả hai đều không có — **không phải** tên 1 khách hàng cụ thể nào (khớp ảnh mẫu: "Người nộp" = tên nhân viên, không phải tên khách).
- `Receipt.Reference` = nối các `SalesOrder.DocumentNumber` đã chọn bằng `", "` (tự động, chỉ để xem).
- Mỗi `ReceiptEntry` tự mang khách hàng riêng qua `SubjectCode`/`SubjectName` (= `SalesOrder.Customer.Code`/`CustomerNameOverride ?? Customer.Name`) — **tái dùng field có sẵn**, không thêm cột `CustomerId` mới trên `ReceiptEntry` (không cần thiết: mọi truy vấn công nợ đều đi qua `ReceiptEntry.SalesOrderId → SalesOrder.CustomerId`, không phụ thuộc `Receipt.CustomerId`/entry-level CustomerId — xem `GetOutstandingSalesOrdersAsync`, hoàn toàn không đổi).
- 1 `DocumentNumber` duy nhất (gọi `IGetNextReceiptCodeUseCase` đúng 1 lần, không phải 1 lần/khách hàng).
- Vẫn tái dùng nguyên `ICreateReceiptUseCase` (validate còn nợ per-entry + tạo `CashTransaction` side-effect) — chỉ gọi 1 lần thay vì N lần.

**DTO đổi:**
- `CreateBulkCustomerReceiptRequestDto` — thêm `payer_name`/`address`/`attachment` (nhập ở popup xác nhận, trước đây các field này không tồn tại vì mỗi Receipt tự lấy `PayerName` = tên khách hàng).
- `CreateBulkCustomerReceiptResponseDto` — đổi `receipts: ReceiptResponseDto[]` → **`receipt: ReceiptResponseDto`** (1 object, không phải mảng).
- `OutstandingSalesOrderDto` — thêm `grand_total`/`payment_terms`/`payment_due_date` (lấy thẳng từ `SalesOrder`, phục vụ tab "2. Chứng từ" phía WPF — xem doc WPF).

**Không đổi / cố tình bỏ qua** (không có data model, tránh làm giả):
- "Số hóa đơn" (invoice number riêng, khác `DocumentNumber`) — `SalesOrder` không có field này.
- "Tỷ lệ CK (%)"/"Tiền chiết khấu"/"TK chiết khấu" ở tab "2. Chứng từ" — khái niệm chiết khấu thanh toán sớm ở mức chứng từ, khác hẳn `SalesOrderLine.DiscountRate` (chiết khấu theo dòng sản phẩm) đã có sẵn; `SalesOrder` không có field chiết khấu thanh toán sớm ở header.
- Màn hình danh sách "Thu tiền khách hàng hàng loạt" riêng (sidebar + Kỳ/Trạng thái/Loại) như ảnh mẫu — **không cần xây mới**: mọi phiếu thu hàng loạt vẫn post đúng 1 `CashTransaction` như phiếu thu thường khi Confirm, nên đã tự động hiện trong màn "Sổ Kế Toán Chi Tiết Quỹ Tiền Mặt" (`GetCashLedgerUseCase`) có sẵn — tái dùng hạ tầng đã có thay vì xây trùng lặp UI.

> **2026-09-01:** mục "Draft/Treo/Confirmed/'Hoàn' lifecycle cho Receipt" note ở trên đã lỗi thời —
> xem mục "Draft/Confirmed Status Workflow" bên dưới. `CreateBulkCustomerReceiptUseCase` tái dùng
> nguyên `ICreateReceiptUseCase` nên tự động thừa hưởng hành vi Create-luôn-Draft mới, **không cần
> đổi gì** ở file này. Lưu ý: phía WPF client cho luồng "Thu tiền khách hàng hàng loạt" sẽ cần thêm
> bước gọi Confirm sau khi Create để phiếu thu hàng loạt thực sự lên Sổ Kế Toán — việc này nằm
> ngoài phạm vi BE task này, cần wiring riêng ở `desktop-lamour`.

## Clean Architecture Layers

```
ReceiptsController              GET/POST/PUT/DELETE + POST /{id}/confirm + /{id}/unconfirm
        ↓                       /api/v1/accounting/receipts
IGetReceiptsUseCase             / GetReceiptsUseCase
IGetReceiptByIdUseCase          / GetReceiptByIdUseCase
ICreateReceiptUseCase           / CreateReceiptUseCase   (Status = Draft, no CashTransaction)
IUpdateReceiptUseCase           / UpdateReceiptUseCase   (chỉ khi Draft, no CashTransaction)
IDeleteReceiptUseCase           / DeleteReceiptUseCase   (chỉ khi Draft, no CashTransaction)
IConfirmReceiptUseCase          / ConfirmReceiptUseCase     (Draft → Confirmed, + CashTransaction)
IUnconfirmReceiptUseCase        / UnconfirmReceiptUseCase   (Confirmed → Draft, − CashTransaction)
        ↓
IReceiptRepository              GetAllAsync, GetByIdAsync, GetByIdTrackedAsync
                                AddAsync, UpdateAsync, DeleteAsync
ICashLedgerRepository           AddAsync, DeleteByReceiptNumberAsync
        ↓
ReceiptRepository               EF Core + AppDbContext
CashLedgerRepository            EF Core + AppDbContext
```

## Files

```
src/Lamour.Domain/
  Entities/Receipt.cs             (+ ReceiptStatus enum, + Status/ConfirmedAt — 2026-09-01)
  Entities/ReceiptEntry.cs
  Enums/PaymentReason.cs
  Enums/AccountCode.cs

src/Lamour.Application/Features/Accounting/
  Dtos/ReceiptEntryDto.cs
  Dtos/ReceiptResponseDto.cs      (+ status/confirmed_at — 2026-09-01)
  Dtos/CreateReceiptRequestDto.cs
  Dtos/UpdateReceiptRequestDto.cs
  Repositories/IReceiptRepository.cs
  UseCases/IGetReceiptsUseCase.cs + GetReceiptsUseCase.cs
  UseCases/IGetReceiptByIdUseCase.cs + GetReceiptByIdUseCase.cs
  UseCases/ICreateReceiptUseCase.cs + CreateReceiptUseCase.cs
  UseCases/IUpdateReceiptUseCase.cs + UpdateReceiptUseCase.cs
  UseCases/IDeleteReceiptUseCase.cs + DeleteReceiptUseCase.cs
  UseCases/IConfirmReceiptUseCase.cs + ConfirmReceiptUseCase.cs      (new — 2026-09-01)
  UseCases/IUnconfirmReceiptUseCase.cs + UnconfirmReceiptUseCase.cs  (new — 2026-09-01)

src/Lamour.Infrastructure/
  Persistence/Configurations/ReceiptConfiguration.cs  (Receipt + ReceiptEntry; Status/ConfirmedAt mapping — 2026-09-01)
  Repositories/ReceiptRepository.cs
  Migrations/..._RebuildReceipts.cs
  Migrations/..._ReceiptStatus.cs  (new — 2026-09-01)

src/Lamour.Api/
  Controllers/ReceiptsController.cs  (+ confirm/unconfirm actions — 2026-09-01)
  Controllers/AccountingController.cs  (trimmed — only GetCashLedger remains)
  Program.cs  (DI updated — 2026-09-01)
```

## Draft/Confirmed Status Workflow (2026-09-01)

Trước 2026-09-01, Receipt **không có** khái niệm status: `CashTransaction` được post ngay khi
Create, và tự re-sync (xóa cũ + tạo mới) mỗi lần Update, và xóa khi Delete. Nay đổi sang mirror
đúng pattern của `Payment` (`PaymentStatus.Draft/Treo/Confirmed`) và `SalesReturn`
(`SalesReturnStatus.Draft/Confirmed`) — riêng Receipt chỉ có 2 state như `SalesReturn`, không có
"Treo" ở giữa như `Payment`:

- `ReceiptStatus.Draft = 0` (mặc định khi tạo), `Confirmed = 1`.
- `Receipt.ConfirmedAt` — `DateTime?`, set khi Confirm, `null` lại khi Unconfirm.
- **Create** (`CreateReceiptUseCase`) — tạo mới ở `Status = Draft` (property default), **không**
  còn tạo `CashTransaction`.
- **Update** (`UpdateReceiptUseCase`) — chỉ cho phép khi `Status == Draft`
  (`DomainException("Chỉ chứng từ ở trạng thái Nháp mới được sửa. Bỏ ghi trước khi sửa.")` nếu
  đã `Confirmed`); replace toàn bộ `Entries`, **không** còn xóa/tạo lại `CashTransaction`.
- **Delete** (`DeleteReceiptUseCase`) — chỉ cho phép khi `Status == Draft`
  (`DomainException("Chỉ chứng từ ở trạng thái Nháp mới được xóa. Bỏ ghi trước khi xóa.")` nếu
  đã `Confirmed`); **không** còn xóa `CashTransaction` (Draft chưa từng có).
- **Confirm** (`POST /{id}/confirm`, "Ghi sổ", `ConfirmReceiptUseCase`) — validate
  `Status == Draft` (`DomainException("Chỉ chứng từ ở trạng thái Nháp mới có thể ghi sổ.")` nếu
  không) → tạo `CashTransaction` (field-mapping y hệt logic cũ từng nằm ở `CreateReceiptUseCase`,
  chỉ chuyển thời điểm thực thi) → `Status = Confirmed`, `ConfirmedAt = DateTime.UtcNow`.
- **Unconfirm** (`POST /{id}/unconfirm`, "Bỏ ghi", `UnconfirmReceiptUseCase`) — validate
  `Status == Confirmed` (`DomainException("Chỉ chứng từ đã ghi sổ mới có thể bỏ ghi.")` nếu
  không) → xóa `CashTransaction` theo `DocumentNumber`
  (`ICashLedgerRepository.DeleteByReceiptNumberAsync`) → `Status = Draft`, `ConfirmedAt = null`.
- Cả `ConfirmReceiptUseCase`/`UnconfirmReceiptUseCase` **không** dùng `IUnitOfWork` — mirror đúng
  convention hiện có của `ConfirmPaymentUseCase`/`UnconfirmPaymentUseCase` (module Accounting
  không dùng `IUnitOfWork` cho Receipt/Payment, khác với module SalesReturn có dùng).

**Backfill dữ liệu cũ:** rows Receipt đã tồn tại trước migration được backfill là `Confirmed` qua
column-level default của EF migration
(`HasDefaultValue(ReceiptStatus.Confirmed)` trong `ReceiptConfiguration.cs`) — vì chúng đã được
post `CashTransaction` tại thời điểm Create theo hành vi cũ, không cần fix data thủ công và không
được phép re-confirm (sẽ double-post cash-ledger).

**Gotcha `HasSentinel`:** `ReceiptStatus.Draft == 0` trùng CLR default của property — nếu không có
`HasSentinel((ReceiptStatus)(-1))`, EF Core coi giá trị `Draft` là "chưa set" và tự thay bằng column
default (`Confirmed`) khi INSERT, khiến mọi Receipt mới tạo bị lưu nhầm thành `Confirmed`. Comment
đầy đủ nằm trong `ReceiptConfiguration.cs` (copy nguyên lý từ `SalesReturnConfiguration.cs`, nơi
gotcha này từng xảy ra lần đầu).

**Endpoints mới:**
- `POST /api/v1/accounting/receipts/{id}/confirm` — "Ghi sổ", trả `ReceiptResponseDto` (200)
- `POST /api/v1/accounting/receipts/{id}/unconfirm` — "Bỏ ghi", trả `ReceiptResponseDto` (200)

`status` trong response DTO là **string** (`Status.ToString()` — `"Draft"` | `"Confirmed"`), cùng
convention với `PaymentResponseDto`/`SalesReturnResponseDto`, **không phải** số nguyên.

**`CreateBulkCustomerReceiptUseCase` không cần đổi gì** — tái dùng nguyên `ICreateReceiptUseCase`
nên tự động thừa hưởng hành vi Create-luôn-Draft mới (grep-confirmed, xem mục "Phiếu thu tiền
khách hàng hàng loạt" ở trên). **Lưu ý cho phần WPF client:** luồng "Thu tiền khách hàng hàng
loạt" hiện chỉ gọi Create — sau thay đổi này, phiếu thu hàng loạt sẽ dừng ở `Draft` và **không**
tự động lên Sổ Kế Toán nữa; WPF cần thêm bước gọi `POST /{id}/confirm` ngay sau khi Create để giữ
nguyên hành vi cũ (post cash-ledger ngay). Việc wiring này nằm ngoài phạm vi BE task, cần làm riêng
ở `desktop-lamour`.

Migration: `ReceiptStatus` (`src/Lamour.Infrastructure/Migrations/`).

---

## Removed (replaced by this rebuild)

- `PaymentReceipt` entity + `PaymentReceiptLine` entity
- All `PaymentReceipt*` DTOs, UseCases, Repository, Configuration
- DB tables: `payment_receipts`, `payment_receipt_lines` (dropped via `RebuildReceipts` migration)
- Endpoint: `POST /api/v1/accounting/payment-receipts`
- Endpoint: `GET /api/v1/accounting/payment-receipts`

## Update — 2026-09-26: Sổ quỹ hiện phiếu thu Nháp + trả id phiếu gốc

Theo yêu cầu màn Quỹ (WPF `AccountingView`) có thanh công cụ giống Chứng từ bán hàng (Thêm ▾ / Sửa / Ghi sổ / Bỏ ghi / Xóa / Xuất khẩu / Gửi email / Gửi Zalo).

| Thay đổi | Chi tiết |
|---|---|
| `GetCashLedgerUseCase` | Thêm dòng **phiếu thu chưa ghi sổ** (`IReceiptRepository.GetUnconfirmedByDateRangeAsync`), mapping giống `ConfirmReceiptUseCase` — chỉ để hiển thị/Ghi sổ, **không** làm đổi số tồn (giống phiếu chi Nháp/Treo có sẵn) |
| `CashLedgerEntryDto` | Thêm `receipt_id` / `payment_id` (nullable). Dòng đã ghi sổ tra id theo số chứng từ (`GetIdsByDocumentNumbersAsync` ở cả 2 repo — số trùng thì lấy id lớn nhất); dòng chưa ghi sổ lấy thẳng `Id` |
| Quy tắc nút (WPF) | Ghi sổ: chưa ghi sổ. Bỏ ghi: đã ghi sổ, không hỏi xác nhận. Sửa/Xóa: chỉ khi chưa ghi sổ (Xóa hỏi Yes/No). Dòng không có id → không thao tác được |

Không có migration. Test: `tests/Lamour.Application.Tests/Features/Accounting/UseCases/GetCashLedgerUseCaseTests.cs` (3 test).

### Cùng ngày — bỏ trạng thái "Nháp" khỏi Quỹ (khớp `Sales/docs/ChungTuTraHangBan-Review.html`)

Quy trình Chứng từ bán hàng chỉ có **Treo** (chưa ghi sổ) và **Đã ghi sổ** — áp dụng y hệt cho phiếu thu/chi trên màn Quỹ:

| Thay đổi | Chi tiết |
|---|---|
| Sổ quỹ | Mọi dòng chưa ghi sổ (phiếu thu `Draft`, phiếu chi `Draft`/`Treo`) trả `status = "Treo"`. Enum trong DB **không đổi** (không migration) — `ReceiptStatus.Draft` giờ chỉ mang nghĩa "Treo" |
| `ConfirmPaymentUseCase` | Nhận cả `Draft` lẫn `Treo`, chỉ chặn `Confirmed`. Trước đây bắt buộc `Treo` ⇒ nút "Cất" (= Ghi sổ) trên phiếu chi **mới tạo** luôn bị từ chối |
| Thông báo lỗi | Bỏ chữ "Nháp" ở `UpdateReceipt`/`DeleteReceipt`/`ConfirmReceipt`/`SetPaymentTreo` — nói theo "đã ghi sổ / Bỏ ghi trước" |
| WPF | Bộ lọc Trạng thái: Tất cả / Treo / Đã ghi sổ. Popup Phiếu chi: Cất ghi sổ thẳng, không còn "Vui lòng bấm Treo trước" |

Test: `ConfirmPaymentUseCaseTests.cs` (Draft + Treo ghi sổ được, Confirmed bị chặn).

### Cùng ngày — popup Phiếu thu / Phiếu chi theo đúng quy trình Chứng từ bán hàng (WPF)

| Bước | Hành vi mới (cả 2 popup) | Trước đây |
|---|---|---|
| Mở phiếu có sẵn | Form **khóa**, bấm ✏️ Sửa mới nhập (Sửa chỉ bật khi chưa ghi sổ) | Phiếu chưa ghi sổ sửa được ngay; phiếu thu không có nút Sửa |
| 💾 Cất | Lưu + Ghi sổ ngay, form tự khóa, **popup vẫn mở** | Đóng popup |
| 📗 Ghi sổ / ↩️ Bỏ ghi | 1 nút toggle, chỉ bật khi form đang khóa, **không hỏi xác nhận** | Chỉ có Bỏ ghi; phiếu thu hỏi Yes/No |
| 🗑️ Xóa | Chỉ khi chưa ghi sổ + form đang khóa, hỏi Yes/No, xóa xong đóng popup | Không hỏi, không đóng |
| ⏸ Treo | **Bỏ** khỏi popup Phiếu chi | Có |

---

## Update — 2026-09-26: Cất phiếu thu thường = CHỈ LƯU (giống phiếu thu hàng loạt)

Kế toán chốt (review trang "Quỹ & Phiếu Thu Hàng Loạt"): phiếu thu thường làm **giống** phiếu thu hàng loạt, khớp MISA. Thay thế dòng "💾 Cất = Lưu + Ghi sổ ngay" ở bảng phía trên.

| Nút | Trước | Sau |
|---|---|---|
| 💾 Cất | Lưu + Ghi sổ ngay | **Chỉ lưu**, phiếu ở **Treo**, form khóa, popup vẫn mở |
| Ghi sổ / Bỏ ghi (popup) hoặc 📗 Ghi sổ (màn Quỹ) | Chỉ dùng sau khi Bỏ ghi | Bước **bắt buộc** để phiếu lên sổ quỹ và tính vào số tồn |

- Chỉ đổi WPF (`ReceiptViewModel.SaveAsync` bỏ lệnh gọi `IConfirmReceiptUseCase` sau Create/Update). BE không đổi: `CreateReceiptUseCase`/`UpdateReceiptUseCase` vốn để phiếu ở `Draft` (= Treo).
- Phiếu chi **chưa đổi**, vẫn Cất = Ghi sổ ngay.

## Update — 2026-09-28: Cất phiếu thu (thường + hàng loạt) = Lưu + Ghi sổ ngay (ĐẢO NGƯỢC 2026-09-26)

Theo yêu cầu: sau khi Cất phải thấy nút "Bỏ ghi" như Chứng từ bán hàng — mục 2026-09-26 ngay trên đã **lỗi thời**.
Giờ Phiếu thu thường, Phiếu thu hàng loạt, Phiếu chi và Chứng từ bán hàng đều cùng một quy tắc.

| Nút | Sau Cất |
|---|---|
| 💾 Cất | Lưu (Create/Update → `Draft`) rồi gọi `ConfirmReceiptUseCase` ngay → `Confirmed`, form khóa, popup vẫn mở |
| Ghi sổ / Bỏ ghi | Nhãn thành **"Bỏ ghi"**; Sửa/Xóa tắt cho tới khi Bỏ ghi |
| Ghi sổ lỗi sau khi lưu | Phiếu vẫn đã lưu ở Treo, banner "Đã lưu phiếu (Treo) nhưng ghi sổ thất bại: …" — bấm Ghi sổ lại |

- Chỉ đổi WPF: `ReceiptViewModel.SaveAsync`, `BulkCustomerReceiptViewModel.SaveAsync`. BE không đổi.

## Update — 2026-09-28: PaymentReason riêng cho Phiếu thu hàng loạt — "Lý do thu/chi" không còn hiện "Thu công nợ"

Theo yêu cầu: cột "Lý do thu/chi" trên màn Quỹ cho dòng Phiếu thu tiền mặt khách hàng **hàng loạt**
phải hiện đúng "Phiếu thu tiền mặt khách hàng hàng loạt", không phải "Thu công nợ" — dù các dòng hạch
toán vẫn dùng `CreditAccount = Receivable131` (bản chất kế toán vẫn là thu công nợ, chỉ đổi CÁCH HIỂN
THỊ theo yêu cầu).

| Thay đổi | Chi tiết |
|---|---|
| `PaymentReason` enum | Thêm `ThuKhachHangHangLoat` (giữ nguyên `ThuCongNo` cho phiếu thu thường chọn tay lý do này) |
| `CreateBulkCustomerReceiptUseCase` | `PaymentReason = "ThuKhachHangHangLoat"` thay vì `"ThuCongNo"` |
| WPF `BulkCustomerReceiptViewModel.SaveAsync` (nhánh Update) | Cùng đổi `PaymentReason` gửi lên khi sửa phiếu hàng loạt |
| WPF `PaymentReasonDisplayConverter`, `AccountingViewModel` (filter label + xuất Excel) | Map `"ThuKhachHangHangLoat"` → "Phiếu thu tiền mặt khách hàng hàng loạt" |
| `ReceiptViewModel.PaymentReasons` (dropdown "Lý do nộp" popup Phiếu thu thường) | **Không đổi** — vẫn chỉ `ThuKhac`/`ThuTienHang`/`ThuCongNo`, user không tự chọn được giá trị mới này |

`HasConversion<string>()` trên `Receipt.PaymentReason` (`HasMaxLength(30)`) — không cần EF migration,
thêm enum member mới là an toàn (giống ghi chú `PaymentStatus` ở `phieu-chi.md`). Verify: `dotnet
build`/`dotnet test` (BE, 20/20 pass) và `dotnet build -p:EnableWindowsTargeting=true` (WPF) đều sạch.
Chưa test qua UTM thật.

## Update — 2026-10-01: Phiếu thu theo luồng MISA (giống Phiếu chi) — Đối tượng KH/NV, Lý do nộp chi tiết, TK từ danh mục

Theo yêu cầu kế toán (7 bước: Đối tượng → Lý do nộp → nội dung → Diễn giải/TK Nợ/Đối tượng tự điền → TK Có → Số tiền → Cất, rồi In mẫu 01-TT). **Các mục "Request", "Enums → AccountCode", "Domain Entities" ở đầu file đã lỗi thời — lấy mục này làm chuẩn.**

| Thay đổi | Chi tiết |
|---|---|
| Đối tượng đa loại | `Receipt.PartnerType` (`Customer`/`Employee`, dùng chung enum `PaymentPartnerType`; `Supplier` bị từ chối) + `PartnerId` + `PartnerName` (cache). Không FK thật. Phiếu của Khách hàng vẫn điền `CustomerId` (= `PartnerId`); phiếu của Nhân viên có `CustomerId = null` |
| Nhận biết phiếu hàng loạt | **`Receipt.IsBulk` = `PartnerType == null`**, thay cho `CustomerId == null` (phiếu của Nhân viên cũng có `CustomerId == null`). Áp dụng ở `ConfirmReceiptUseCase`, `GetCashLedgerUseCase`, `ReceiptRepository.GetBulkReceiptIdsAsync`, WPF `BulkCustomerReceiptViewModel` |
| Lý do nộp | Thêm `RutTienGuiVeNopQuy`, `ThuHoanThueGTGT`, `ThuHoanUng` (cuối enum `PaymentReason`). WPF cho chọn 4: Rút tiền gửi về nộp quỹ · Thu hoàn thuế GTGT · Thu hoàn ứng · Thu khác (mặc định). `ThuTienHang`/`ThuCongNo` chỉ còn cho phiếu cũ |
| Lý do nộp chi tiết | `Receipt.ReasonDetail` (500, BE trim, rỗng → `null`). WPF: chọn lý do thì ô nội dung tự điền nhãn lý do (khi ô trống hoặc còn mang nhãn cũ); Diễn giải các dòng đi theo ô nội dung |
| TK Nợ / TK Có | `ReceiptEntry.DebitAccountSettingId` / `CreditAccountSettingId` (FK `AccountSetting`, Restrict) thay cho enum `AccountCode`. Mặc định TK Nợ **1111**, TK Có **1388** (đổi tay được; migration `AddReceiptDefaultCreditAccount1388` chèn TK 1388 "Phải thu khác" nếu thiếu). Lưới mở ra trống; dòng đầu tự điền sau khi chọn Đối tượng, dòng sau tự điền khi gõ Số tiền |
| DTO | Request: `partner_type`, `partner_id`, `reason_detail` (bỏ `customer_id`). Entry: `debit_account_id/code/description`, `credit_account_id/code/description`. Response thêm `partner_type/id/name`, `reason_detail` (vẫn có `customer_id`, `customer_name`) |
| Phiếu thu hàng loạt | Contract `POST /bulk` **không đổi**. Entry của phiếu hàng loạt vẫn gửi `debit_account: "Cash111"\|"Bank112"`, `credit_account: "Receivable131"`; BE (`ReceiptEntryBuilder`) tra sang danh mục khi `*_account_id = 0`: Cash111→1111, Bank112→1121, Receivable131→131, Payroll334→334. Không có `partner_type` chỉ hợp lệ với `ThuKhachHangHangLoat` |
| Sổ quỹ | `CashTransaction.Account` = TK cấp 1 của TK Nợ dòng đầu (1111→`111`, 1121→`112`), `CounterAccount` = mã TK Có (vd `1388`). Diễn giải = `ReasonDetail`, trống thì nhãn lý do. `DocumentType`: Nhân viên → "Phiếu thu", Khách hàng → "Phiếu thu tiền mặt khách hàng", hàng loạt như cũ |
| Bản in 01-TT | WPF nối nút In vào `ReceiptWindow`; Nợ/Có in mã TK thật, "Lý do nộp" = `ReasonDetail` (trống thì nhãn) |
| Code dùng chung | `UseCases/ReceiptEntryBuilder.cs` — parse lý do, resolve đối tượng, build + validate dòng, `LedgerAccount`, `DocumentType` (Create/Update/Confirm/GetCashLedger cùng dùng) |

**Migration `ReceiptPartnerAndAccountSettings` (sửa dữ liệu đã lưu — backup DB trước khi deploy):** chèn TK 1111/1121/131/334 nếu thiếu → thêm cột mới → đổi `DebitAccount`/`CreditAccount` cũ sang id danh mục → điền `PartnerType='Customer'`/`PartnerId`/`PartnerName` cho phiếu có `CustomerId` → xoá 2 cột cũ, gắn FK. `Down` map ngược (mã ngoài 4 giá trị cũ rơi về Cash111/Receivable131). Đã chạy up → down → up trên DB local.

**Lỗi BE mới:** "Vui lòng chọn đối tượng." · "Khách hàng/Nhân viên không tồn tại." · "Đối tượng của phiếu thu chỉ là Khách hàng hoặc Nhân viên." · "Vui lòng chọn tài khoản Nợ/Có." · "Tài khoản Nợ/Có không tồn tại." · "Danh mục tài khoản chưa có TK {mã}." (phiếu hàng loạt).

**Test:** `CreateReceiptUseCaseTests` (7), `ConfirmReceiptUseCaseTests` (4), `GetCashLedgerUseCaseTests` (+1). Chưa test giao diện trên UTM.

**Chưa làm:** chặn số tiền âm, chặn trùng Số chứng từ, lọc sổ quỹ theo TK 111/112 (như Phiếu chi). `AccountSettingRepository.IsInUseAsync` chưa kiểm tra dòng phiếu thu/chi — xoá TK đang được dùng sẽ bị DB chặn (FK Restrict) với lỗi chưa thân thiện.

## Update — 2026-10-01: gõ mã TK Nợ / TK Có / Khoản mục CP trực tiếp (Phiếu thu + Phiếu chi, WPF)

Theo kế toán: bấm vào ô TK rồi gõ số tài khoản là ra luôn, không phải kéo danh sách. Chỉ đổi WPF, BE không đổi.

**Cách làm (bản cuối):** ô TK trong lưới Hạch toán là `AppSearchableComboBox` (control tìm kiếm dùng chung, cùng loại với ô Mã hàng ở Chứng từ bán hàng) đặt thẳng trong `CellTemplate`, với 3 cờ: `IsCompact` (không viền, nền trong suốt, thấp, không mũi tên — dòng trống trông trống hẳn), `CodeOnlyDisplay` (ô chỉ hiện mã), `CommitTypedText`.

| Hành vi | Chi tiết |
|---|---|
| Bấm vào ô / đang gõ | Danh sách mở và lọc theo chữ đã gõ (mã hoặc tên chứa chữ đó, không phân biệt dấu) |
| Enter / rời ô mà chưa bấm chọn dòng nào (`CommitTypedText`) | Chọn mã khớp hẳn → không thì mã bắt đầu bằng chữ đã gõ → không thì dòng đầu danh sách đang lọc |
| Không có dòng nào khớp | Trả ô về TK trước khi gõ |
| Xoá trắng ô | Bỏ chọn TK (bấm Cất sẽ báo thiếu TK nếu dòng có số tiền) |

**Đã thử và bỏ:** `ComboBox` gốc của WPF với `IsEditable="True"` (attached behavior `TypeToSelectComboBehavior`, đã xoá). Lỗi trên UTM: mở danh sách thì WPF tự bôi đen chữ trong ô nên ký tự kế tiếp đè mất ký tự trước (nặng hơn khi bật bộ gõ tiếng Việt), và danh sách không lọc. Đừng quay lại hướng này.

`IsCompact` / `CommitTypedText` mặc định `false` nên các màn hình khác đang dùng `AppSearchableComboBox` không đổi hành vi. `ExpenseCategory` implement `ISearchableItem` để dùng được cho ô Khoản mục CP. **Phiếu thu hàng loạt không đổi:** TK Nợ chọn ở popup tìm kiếm (Tiền mặt / Tiền gửi), TK Có luôn 131, hai cột trên lưới chỉ để xem.
