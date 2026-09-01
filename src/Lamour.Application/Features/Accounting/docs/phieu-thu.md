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
ThuKhac      — Thu khác
ThuTienHang  — Thu tiền hàng
ThuCongNo    — Thu công nợ
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
